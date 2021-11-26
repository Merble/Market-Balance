using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace MarketBalance
{
    public class BoardManager : MonoBehaviour
    {
        public delegate void OrderEvent(OrderType order);
        public event OrderEvent OrderDidService;
        public event Action AutoServiceDidStop;
        
        [SerializeField] private Block[] _BlockPrefabs = new Block[4];
        [SerializeField] private Vector2 _TileSize;
        [SerializeField] private Vector2Int _GridSize;
        [SerializeField] private int _MatchCount = 3;
        [Space]
        [SerializeField] private float _BlockDropDuration = .4f;
        [SerializeField] private float _BlockSwipeDuration = .2f;
        [SerializeField] private float _BlockCreationDuration = .2f;
        [SerializeField] private float _BlockMinScale = .16f;
        [SerializeField] private float _BlockMaxScale = .9f;
        [Space]
        [SerializeField] private bool _isLog;
        
        
        private Block[,] _blocks;

        // private Block _firstBlockOfSwipe;
        // private Block _lastBlockOfSwipe;

        private bool _isInputAllowed;
        public bool IsInputAllowed
        {
            get=> _isInputAllowed;
            private set
            {
                if(_isLog)
                    Debug.Log($"Input allowed: {value}");
                
                _isInputAllowed = value;
            }
        }

        private void Awake()
        {
            CreateBoard(false);
        }

        private void Start()
        {
            ClearAllMatches();
            IsInputAllowed = true;
        }

        private void CreateBoard (bool isAnimated)
        {
            _blocks = new Block[_GridSize.x, _GridSize.y];
            
            for (var x = 0; x < _GridSize.x; x++) 
            {
                for (var y = 0; y < _GridSize.y; y++)
                {
                    CreateRandomBlockAtPos(x, y, isAnimated);
                }
            }
        }
        private void CreateRandomBlockAtPos(int x, int y, bool isAnimated)
        {
            var newBlock = GetRandomBlock();
            _blocks[x, y] = newBlock;
            
            newBlock.GridPos = new Vector2Int(x, y);
            newBlock.transform.localPosition = GetLocalPosForGridPos(x, y);

            if (!isAnimated) return;
            newBlock.transform.localScale = Vector3.one * _BlockMinScale;
            LeanTween.scale(newBlock.gameObject, Vector3.one * _BlockMaxScale, _BlockCreationDuration);
        }

        private Vector3 GetLocalPosForGridPos(int x, int y)
        {
            return new Vector3(_TileSize.x * x, 0, _TileSize.y * y);
        }

        private Block GetRandomBlock()
        {
            var randomBlockPrefab = _BlockPrefabs[Random.Range(0, _BlockPrefabs.Length)];
            var newObject = Instantiate(randomBlockPrefab, transform, false);
            return newObject;
        }
        
        private void RemoveBlock(Vector2Int gridPos, bool isAnimated)
        {
            void DestroyBlock()
            {
                Destroy(_blocks[gridPos.x, gridPos.y].gameObject);
                _blocks[gridPos.x, gridPos.y] = null;
            }
            
            if(isAnimated)
            {
                LeanTween.scale(_blocks[gridPos.x, gridPos.y].gameObject, Vector3.one *_BlockMinScale, _BlockCreationDuration).setOnComplete(DestroyBlock);
            }
            else DestroyBlock();
        }
        
        private void DropAllBlocks(bool isAnimated)
        {
            for (var x = 0; x < _GridSize.x; x++) 
            {
                for (var y = 0; y < _GridSize.y; y++)
                {
                    if (_blocks[x, y] != null) continue;
                    
                    DropToEmptySpace(x, y, isAnimated);
                    break;
                }
            }
        }
        private void DropToEmptySpace(int posX, int posY, bool isAnimated)
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
                    
                    block.GridPos = new Vector2Int(posX, newYPos);

                    var blockLocalPos = GetLocalPosForGridPos(posX, newYPos);
                    if (isAnimated)
                    {
                        LeanTween.moveLocal(block.gameObject, blockLocalPos, _BlockDropDuration);
                    }
                    else
                        block.transform.localPosition = blockLocalPos;

                }
            }
        }

        private void RefillTheBoard(bool isAnimated)
        {
            for (var x = 0; x < _GridSize.x; x++)
            {
                for (var y = 0; y < _GridSize.y; y++)
                {
                    var tile = _blocks[x, y];
                    
                    if (!tile)
                    {
                        CreateRandomBlockAtPos(x, y, isAnimated);
                    }
                }
            }
        }

        private void ClearAllMatches()
        {
            while (true)
            {
                var matchingBlocks = GetMatchingBlocks();
                
                if(matchingBlocks.Count <= 0) break;

                foreach (var matchingBlock in matchingBlocks)
                {
                    RemoveBlock(matchingBlock.GridPos, false);
                }
                
                RefillTheBoard(false);
            }
        }

        private void EvaluateBoardTillEnd()
        {
            StartCoroutine(EvaluateBoardTillEndRoutine());
        }
        
        private IEnumerator EvaluateBoardTillEndRoutine()
        {
            IsInputAllowed = false;
            while (true)
            {
                var foundMatches = EvaluateBoardOnce();
                if (!foundMatches) break;
                yield return new WaitForSeconds(.6f);
            
                DropAllBlocks(true);
                yield return new WaitForSeconds(_BlockDropDuration);
                
                RefillTheBoard(true);
                yield return new WaitForSeconds(_BlockCreationDuration);
            }

            var blocksToSwap = FindTheBlocksToSwap();
            if (!blocksToSwap.Any())  // If there is no valid moves
            {
                Debug.Log("No more valid move");
                StartCoroutine(DoAfter(3f, () =>
                {
                    ShuffleTheBlocks();
                    IsInputAllowed = true;
                }));
            }
            else
            {
                IsInputAllowed = true;
            }
        }

        private void ShuffleTheBlocks()
        {
            foreach (var block in _blocks)
            {
                RemoveBlock(block.GridPos, true);
            }

            StartCoroutine(DoAfter(1f, () => { CreateBoard(true); }));
            StartCoroutine(DoAfter(2f, ClearAllMatches));
        }

        // Returns true if finds any match
        private bool EvaluateBoardOnce()    // Find all 3 or more matches and destroy them
        {
            var sameBlocks = GetMatchingBlocks();

            if (!sameBlocks.Any())    // Control if removal needed
            {
                AutoServiceDidStop?.Invoke();
                return false;
            }
            StartCoroutine(DoAfter(.2f, () =>
            {
                // Emit events for removed block types
                var uniqueBlockTypes = sameBlocks.Select(block => block.OrderType).Distinct();
                foreach (var uniqueBlockType in uniqueBlockTypes)
                {
                    OrderDidService?.Invoke(uniqueBlockType);
                }

                // Remove blocks
                foreach (var block in sameBlocks)
                {
                    RemoveBlock(block.GridPos, true);
                }
            }));
            
            return true;
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
            StartCoroutine(SwapBlocksRoutine(firstGridPos, swipeDir));
        }
        public IEnumerator SwapBlocksRoutine(Vector2Int firstGridPos, Vector2Int swipeDir)
        {
            IsInputAllowed = false;

            var first = GetBlockAtPos(firstGridPos);
            var lastGridPos = first.GridPos + swipeDir;
            var second = GetBlockAtPos(lastGridPos);
            if(!first || !second) yield break;

            // Move the chosen blocks
            MoveSwappedBlocks(first, second, () =>
            {
                if (GetMatchingBlocks().Count > 0)
                {
                    EvaluateBoardTillEnd();
                }
                else
                    MoveSwappedBlocks(second, first, () =>
                    {
                        IsInputAllowed = true;
                    });
            });
            
            // Time to check if they have match
            // StartCoroutine(DoAfter(1f, () =>
            // {
            //     if (!(first == null || second == null)) // If there is no matches
            //     {
            //         MoveSwappedBlocks();
            //     }
            // }));

            // StartCoroutine(DoAfter(1.3f, () => { IsInputAllowed = true; }));
        }
        
        private void MoveSwappedBlocks(Block first, Block second, Action callback = null)
        {
            // First make sure to store GridPos change infos
            var firstGridPos = first.GridPos;
            var lastGridPos = second.GridPos;
            first.GridPos = new Vector2Int(lastGridPos.x, lastGridPos.y);
            second.GridPos = new Vector2Int(firstGridPos.x, firstGridPos.y);
            
            // Then change the world positions
            var firstBlockPos = GetLocalPosForGridPos(lastGridPos.x, lastGridPos.y);
            LeanTween.moveLocal(first.gameObject, firstBlockPos, _BlockSwipeDuration).setOnComplete(callback);
            var lastBlockPos = GetLocalPosForGridPos(firstGridPos.x, firstGridPos.y);
            LeanTween.moveLocal(second.gameObject, lastBlockPos, _BlockSwipeDuration);
            
            // Last but not least: actually changing board array
            _blocks[firstGridPos.x, firstGridPos.y] = second;
            _blocks[lastGridPos.x, lastGridPos.y] = first;
        }
        
        private IEnumerator DoAfter(float waitTime, Action callback)
        {
            yield return new WaitForSeconds(waitTime);
            
            callback?.Invoke();
        }
        
        public struct BlockToSwapResult
        {
            public Vector2Int Position;
            public Vector2Int Direction;
            public OrderType? Type;
            public bool IsNull;
        }
        public List<BlockToSwapResult> FindTheBlocksToSwap()     // An algorithm that will find a valid swipe option on the board.
        {
            var resultList = new List<BlockToSwapResult>();
            
            // RightFirst
            for (var y = 0; y < _GridSize.y; y++)
            {
                for (var x = 0; x < _GridSize.x; x++)
                {
                    if (x > _GridSize.x - _MatchCount)
                        continue;
                    
                    // Gather all matchSize number of blocks
                    var checkingBlocks = new List<Block>();
                    for (var i = 0; i < _MatchCount; i++)
                    {
                        checkingBlocks.Add(_blocks[x + i, y]);
                    }

                    var otherBlockIndex = GetSingleDifferentBlockIndex(checkingBlocks);
                    if (otherBlockIndex < 0) continue;

                    var otherBlock = checkingBlocks[otherBlockIndex];
                    checkingBlocks.RemoveAt(otherBlockIndex);

                    var blocksToSwap = GetBlocksToSwap(otherBlock, checkingBlocks);
                    if(!blocksToSwap.IsNull) 
                        resultList.Add(blocksToSwap);
                }
            }
            
            // UpFirst
            for (var x = 0; x < _GridSize.x; x++)
            {
                for (var y = 0; y < _GridSize.y; y++)
                {
                    if (y > _GridSize.y - _MatchCount)
                        continue;
                    
                    // Gather all matchSize number of blocks
                    var checkingBlocks = new List<Block>();
                    for (var i = 0; i < _MatchCount; i++)
                    {
                        checkingBlocks.Add(_blocks[x, y + i]);
                    }

                    var otherBlockIndex = GetSingleDifferentBlockIndex(checkingBlocks);
                    if (otherBlockIndex < 0) continue;

                    var otherBlock = checkingBlocks[otherBlockIndex];
                    checkingBlocks.RemoveAt(otherBlockIndex);

                    var blocksToSwap = GetBlocksToSwap(otherBlock, checkingBlocks);
                    if(!blocksToSwap.IsNull) 
                        resultList.Add(blocksToSwap);
                }
            }
            
            return resultList;
        }

        private int GetSingleDifferentBlockIndex(List<Block> blocksToCheck)
        {
            Assert.IsTrue(blocksToCheck.Count >= 3);

            OrderType? mainType;

            // Evaluate 3 cases to find main type
            if (blocksToCheck[0].OrderType == blocksToCheck[1].OrderType)
            {
                mainType = blocksToCheck[0].OrderType;
            }
            else if (blocksToCheck[1].OrderType == blocksToCheck[2].OrderType)
            { 
                mainType = blocksToCheck[1].OrderType;
            }
            else if (blocksToCheck[0].OrderType == blocksToCheck[2].OrderType)
            {
                mainType = blocksToCheck[0].OrderType;
            }
            else return -1;
            
            var unMatchCount = 0;
            var unMatchBlockIndex = 0;
            for (var index = 0; index < blocksToCheck.Count; index++)
            {
                var block = blocksToCheck[index];
                if (block.OrderType != mainType)
                {
                    unMatchBlockIndex = index;
                    unMatchCount++;
                }

                if (unMatchCount > 1) return -1;
            }

            return unMatchBlockIndex;
        }

        private BlockToSwapResult GetBlocksToSwap(Block otherBlock, List<Block> sameBlocks)
        {
            BlockToSwapResult blocksToSwap;
            var sameType = sameBlocks[0].OrderType;
            var blockPos = otherBlock.GridPos;

            var left = GetBlockAtPos(blockPos + Vector2Int.left);
            if (left)
                if (!sameBlocks.Contains(left) && left.OrderType == sameType)
                {
                    blocksToSwap.Position = otherBlock.GridPos;
                    blocksToSwap.Direction = Vector2Int.left;
                    blocksToSwap.Type = sameType;
                    blocksToSwap.IsNull = false;
                    
                    return blocksToSwap;
                }

            var right = GetBlockAtPos(blockPos + Vector2Int.right);
            if (right)
                if (!sameBlocks.Contains(right) && right.OrderType == sameType)
                {
                    blocksToSwap.Position = otherBlock.GridPos;
                    blocksToSwap.Direction = Vector2Int.right;
                    blocksToSwap.Type = sameType;
                    blocksToSwap.IsNull = false;

                    return blocksToSwap;
                }

            var up = GetBlockAtPos(blockPos + Vector2Int.up);
            if (up)
                if (!sameBlocks.Contains(up) && up.OrderType == sameType)
                {
                    blocksToSwap.Position = otherBlock.GridPos;
                    blocksToSwap.Direction = Vector2Int.up;
                    blocksToSwap.Type = sameType;
                    blocksToSwap.IsNull = false;

                    return blocksToSwap;
                }

            var down = GetBlockAtPos(blockPos + Vector2Int.down);
            if (down)
                if (!sameBlocks.Contains(down) && down.OrderType == sameType)
                {
                    blocksToSwap.Position = otherBlock.GridPos;
                    blocksToSwap.Direction = Vector2Int.down;
                    blocksToSwap.Type = sameType;
                    blocksToSwap.IsNull = false;

                    return blocksToSwap;
                }
            
            blocksToSwap.Position = Vector2Int.zero;
            blocksToSwap.Direction = Vector2Int.zero;
            blocksToSwap.Type = null;
            blocksToSwap.IsNull = true;

            return blocksToSwap;
        }
    }
}
