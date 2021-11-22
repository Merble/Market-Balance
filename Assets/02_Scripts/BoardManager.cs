using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MarketBalance
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private Block[] _BlockPrefabs = new Block[4];
        private Block[,] _blocks;
        public Block[,] Blocks => _blocks;

        private Block _firstBlockOfSwipe;
        private Block _lastBlockOfSwipe;
        
        public delegate void OrderEvent(OrderType order);
        public event OrderEvent OnOrderService;
        public event Action OnAutoServiceStop;

        [SerializeField] private Vector2 _TileSize;
        [SerializeField] private int _MatchCount = 3;
        [SerializeField] private Vector2Int _GridSize;
        public int MatchCount => _MatchCount;
        public Vector2Int GridSize => _GridSize;
        
        public bool IsInputAllowed { get; private set; }

        private void Awake()
        {
            IsInputAllowed = true;
            CreateBoard();
        }
        
        private void CreateBoard ()
        {
            _blocks = new Block[_GridSize.x, _GridSize.y];
            
            for (var x = 0; x < _GridSize.x; x++) 
            {
                for (var y = 0; y < _GridSize.y; y++)
                {
                    CreateRandomBlockAtPos(x, y);
                }
            }
        }
        
        private void CreateRandomBlockAtPos(int x, int y)
        {
            var newBlock = CreateRandomBlock();
            _blocks[x, y] = newBlock;
            
            newBlock.GridPos = new Vector2Int(x, y);
            newBlock.transform.position = GetWorldPosForGridPos(x, y);
            
            LeanTween.scale(newBlock.gameObject, Vector3.one * .8f, .2f);
        }

        private Vector3 GetWorldPosForGridPos(int x, int y)
        {
            var startPosition = transform.position;

            return startPosition + new Vector3(_TileSize.x * x, 0, _TileSize.y * y);
        }

        private Block CreateRandomBlock()
        {
            var randomBlockPrefab = _BlockPrefabs[Random.Range(0, _BlockPrefabs.Length)];
            var newObject = Instantiate(randomBlockPrefab, transform, true);
            
            newObject.transform.localScale = Vector3.one / 6f;
            
            return newObject;
        }
        
        private void RemoveItem(Vector2Int gridPos)
        {
            LeanTween.scale(_blocks[gridPos.x, gridPos.y].gameObject, Vector3.one / 6f, .2f);
            
            StartCoroutine(DoAfter(.2f, () =>
            {
                Destroy(_blocks[gridPos.x, gridPos.y].gameObject);
                _blocks[gridPos.x, gridPos.y] = null;
            }));
        }
        
        [Button]
        private void FindEmptySpaces()
        {
            var count = 0; // To make sure if re-evaluation is necessary
            
            for (var x = 0; x < _GridSize.x; x++) 
            {
                for (var y = 0; y < _GridSize.y; y++)
                {
                    if (_blocks[x, y] != null) continue;
                    DropToEmptySpace(x, y);
                    count++;
                    break;
                }
            }
            // Refill the new empty spaces
            StartCoroutine(DoAfter(.8f, RefillTheBoard));
            
            // Re-evaluate board
            if (count > 0)
            {
                StartCoroutine(DoAfter(1.5f, EvaluateBoard));
            }
            else
            {
                IsInputAllowed = true;
            }
        }
        
        private void DropToEmptySpace(int posX, int posY)
        {
            var nullCount = 1;
            
            for (var y = posY + 1; y < _GridSize.y; y++)
            {
                var block = _blocks[posX, y];
                
                if (block == null) 
                {
                    nullCount++;
                }
                else
                {
                    var newYPos = y - nullCount;
                    _blocks[posX, newYPos] = block;
                    _blocks[posX, y] = null;
                    _blocks[posX, newYPos].GridPos = new Vector2Int(posX, newYPos);

                    LeanTween.move(_blocks[posX, newYPos].gameObject, GetWorldPosForGridPos(posX, newYPos), 0.4f);
                }
            }
        } 
        
        private void RefillTheBoard()
        {
            for (var x = 0; x < _GridSize.x; x++)
            {
                for (var y = 0; y < _GridSize.y; y++)
                {
                    var tile = _blocks[x, y];
                    
                    if (!tile)
                    {
                        CreateRandomBlockAtPos(x, y);
                    }
                }
            }
        }

        [Button]
        private void ClearAllMatches()
        {
            while (true)
            {
                var matchingBlocks = GetMatchingBlocks();
                
                if(matchingBlocks.Count <= 0) break;
            }
        }
        
        [Button]
        private void EvaluateBoard()    // Find all 3 or more matches and destroy them
        {
            IsInputAllowed = false;
            
            var sameBlocks = GetMatchingBlocks();

            if (!sameBlocks.Any())    // Control if no removal needed
            {
                OnAutoServiceStop?.Invoke();
                IsInputAllowed = true;
                return;
            }
            
            StartCoroutine(DoAfter(.2f, () =>
            {
                // Emit events for removed block types
                var uniqueBlockTypes = sameBlocks.Select(block => block.OrderType).Distinct();
                foreach (var uniqueBlockType in uniqueBlockTypes)
                {
                    OnOrderService?.Invoke(uniqueBlockType);
                }

                // Remove blocks
                foreach (var block in sameBlocks)
                {
                    RemoveItem(block.GridPos);
                }
            }));
            
            // Find and fill new empty spaces 
            StartCoroutine(DoAfter(.6f, FindEmptySpaces));
        }

        private List<Block> GetMatchingBlocks()
        {
            var sameBlocks = RightFirst();
            var newBlocks = UpFirst();

            foreach (var block in newBlocks)
            {
                if (!sameBlocks.Contains(block))
                    sameBlocks.Add(block);
            }

            return sameBlocks;
        }

        private List<Block> RightFirst()
        {
            var sameBlocks = new List<Block>();
            
            for (var y = 0; y < _GridSize.y; y++)
            {
                for (var x = 0; x < _GridSize.x; x++)
                {
                    // Skip unnecessary blocks
                    if (x > _GridSize.x - _MatchCount)
                        continue;

                    var currentBlock = _blocks[x, y];

                    // Skip if block is already found
                    if (sameBlocks.Contains(currentBlock))
                        continue;

                    // Check if next matchCount - 1 blocks are the same
                    var isAllMatch = true;
                    for (var i = 1; i < _MatchCount; i++)
                    {
                        var blockToCheck = _blocks[x + i, y];

                        var isMatch = currentBlock.OrderType == blockToCheck.OrderType;
                        isAllMatch = isMatch;
                        if (!isMatch) break;
                    }

                    if (isAllMatch)
                    {
                        // Run Flood fill algorithm and add all blocks (including the root) to the sameBlocks list.

                        sameBlocks.Add(currentBlock);

                        var newBlocks = FloodFill(x, y);

                        foreach (var block in newBlocks)
                        {
                            if (!sameBlocks.Contains(block))
                                sameBlocks.Add(block);
                        }
                    }
                }
            }
            return sameBlocks;
        }
        
        private List<Block> UpFirst()
        {
            var sameBlocks = new List<Block>();
            
            for (var x = 0; x < _GridSize.x; x++)
            {
                for (var y = 0; y < _GridSize.y; y++)
                {
                    // Skip unnecessary blocks
                    if (y > _GridSize.y - _MatchCount)
                        continue;

                    var currentBlock = _blocks[x, y];

                    // Skip if block is already found
                    if (sameBlocks.Contains(currentBlock))
                        continue;

                    // Check if next matchCount - 1 blocks are the same
                    var isAllMatch = true;
                    for (var i = 1; i < _MatchCount; i++)
                    {
                        var blockToCheck = _blocks[x, y + i];

                        var isMatch = currentBlock.OrderType == blockToCheck.OrderType;
                        isAllMatch = isMatch;
                        if (!isMatch) break;
                    }

                    if (isAllMatch)
                    {
                        // Run Flood fill algorithm and add all blocks (including the root) to the sameBlocks list.

                        sameBlocks.Add(currentBlock);

                        var newBlocks = FloodFill(x, y);

                        foreach (var block in newBlocks)
                        {
                            if (!sameBlocks.Contains(block))
                                sameBlocks.Add(block);
                        }
                    }
                }
            }
            return sameBlocks;
        }

        private List<Block> FloodFill(int x, int y)
        {
            var blockList = new List<Block>();

            var initialBlock = GetBlockAtPos(new Vector2Int(x, y));
            var lookupList = new List<Block> {initialBlock};
            
            while (lookupList.Count > 0)
            {
                var lookupPos = lookupList[lookupList.Count - 1].GridPos;
                var lookupBlock = GetBlockAtPos(lookupPos);
                
                lookupList.Remove(lookupBlock);
                blockList.Add(lookupBlock);
                
                var neighbors = new List<Block>();
                
                var left = GetBlockAtPos(lookupPos + Vector2Int.left);
                if(left) neighbors.Add(left);
                
                var right = GetBlockAtPos(lookupPos + Vector2Int.right);
                if(right) neighbors.Add(right);
                
                var up = GetBlockAtPos(lookupPos + Vector2Int.up);
                if(up) neighbors.Add(up);
                
                var down = GetBlockAtPos(lookupPos + Vector2Int.down);
                if(down) neighbors.Add(down);

                foreach (var neighbor in neighbors)
                {
                    if (lookupList.Contains(neighbor)) continue;
                    if (blockList.Contains(neighbor)) continue;
                    if (neighbor.OrderType != lookupBlock.OrderType) continue;
                    
                    lookupList.Add(neighbor);
                }
            }
            return blockList;
        }

        private Block GetBlockAtPos(Vector2Int pos)
        {
            if (pos.x < 0) return null;
            if (pos.y < 0) return null;
            if (pos.x >= _GridSize.x) return null;
            if (pos.y >= _GridSize.y) return null;

            return _blocks[pos.x, pos.y];
        }
        
        public void SwapBlocks(Vector2Int firstGridPos, Vector2Int swipeDir)
        {
            _firstBlockOfSwipe = _blocks[firstGridPos.x, firstGridPos.y];

            var lastGridPos = _firstBlockOfSwipe.GridPos + swipeDir;
            
            if (( (lastGridPos.x < 0) || (lastGridPos.x >= _GridSize.x) || (lastGridPos.y < 0) ||
                  (lastGridPos.y >= _GridSize.y) ))    return;
            
            _lastBlockOfSwipe = _blocks[lastGridPos.x, lastGridPos.y];
            
            // Move the chosen blocks
            MoveSwipedBlocks();

            StartCoroutine(DoAfter(0.1f, EvaluateBoard));
            
            // Time to check if they have match
            StartCoroutine(DoAfter(1f, () =>
            {
                if (!(_firstBlockOfSwipe == null || _lastBlockOfSwipe == null)) // If there is no matches
                {
                    MoveSwipedBlocks();
                }
            }));
        }
        
        private void MoveSwipedBlocks()
        {
            // First make sure to store GridPos change infos
            var firstGridPos = _firstBlockOfSwipe.GridPos;
            var lastGridPos = _lastBlockOfSwipe.GridPos;
            _firstBlockOfSwipe.GridPos = new Vector2Int(lastGridPos.x, lastGridPos.y);
            _lastBlockOfSwipe.GridPos = new Vector2Int(firstGridPos.x, firstGridPos.y);
            
            // Then change the world positions
            _firstBlockOfSwipe.transform.position = GetWorldPosForGridPos(lastGridPos.x, lastGridPos.y);
            _lastBlockOfSwipe.transform.position = GetWorldPosForGridPos(firstGridPos.x, firstGridPos.y);
            
            // Last but not least: actually changing board array
            _blocks[firstGridPos.x, firstGridPos.y] = _lastBlockOfSwipe;
            _blocks[lastGridPos.x, lastGridPos.y] = _firstBlockOfSwipe;
        }
        
        private IEnumerator DoAfter(float waitTime, Action callback)
        {
            yield return new WaitForSeconds(waitTime);
            
            callback?.Invoke();
        }
    }
}
