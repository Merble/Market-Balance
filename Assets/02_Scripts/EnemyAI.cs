using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarketBalance
{
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private BoardManager _BoardManager;
        [SerializeField] private CustomerManager _CustomerManager;
        [FormerlySerializedAs("_SwapTime")] [SerializeField] private float _SwapInterval;

        private Vector2Int _firstGridPos;
        private Vector2Int _swipeDir;
        
        private float _lastSwapTime;
        
        private OrderType? CurrentOrder => _CustomerManager.FirstOrder;
        private Vector2Int GridSize => _BoardManager.GridSize;
        private int MatchCount => _BoardManager.MatchCount;
        
        private void Awake()
        {
            //_lastSwapTime = 0f;
            
            _BoardManager.OnOrderService += OnOrderService;
            _BoardManager.OnAutoServiceStop += _CustomerManager.AddCustomers;
            
            //FindTheBlocksToSwap();
        }
        private void Update()
        {
            if (_BoardManager.IsInputAllowed && _lastSwapTime + _SwapInterval < Time.time)
            {
                SwapTiles();
            }
                
        }
        
        [Button]
        private void SwapTiles()
        {
            // TODO: This method will be done when the valid swipe finder algorithm is done.
            FindTheBlocksToSwap();

            _lastSwapTime = Time.time;
        }
        
        private void FindTheBlocksToSwap()
        {
            // TODO: Write an algorithm that will find a valid swipe option on the board.
            var blocks = _BoardManager.Blocks;
            
            // RightFirst
            for (var y = 0; y < GridSize.y; y++)
            {
                for (var x = 0; x < GridSize.x; x++)
                {
                    Block otherBlock = null;
                    
                    if (x > GridSize.y - MatchCount)
                        continue;
                    
                    var sameBlocks = new List<Block>();
                    
                    var currentBlock = blocks[x, y];
                    sameBlocks.Add(currentBlock);
                    
                    for (var i = 1; i < MatchCount; i++)
                    {
                        if (currentBlock.OrderType == blocks[x + i, y].OrderType)
                            sameBlocks.Add(blocks[x + i, y]);
                        else if (!otherBlock)
                            otherBlock = blocks[x + i, y];
                    }
                    if (!(otherBlock == null) && sameBlocks.Count == MatchCount - 1)
                    {
                        var blockPos = otherBlock.GridPos;
                            
                        var left = GetBlockAtPos(blockPos + Vector2Int.left);
                        if(left)
                            if (!sameBlocks.Contains(left) && left.OrderType == currentBlock.OrderType)
                            {
                                _firstGridPos = otherBlock.GridPos;
                                _swipeDir = Vector2Int.left;
                                return;
                            }
                
                        var right = GetBlockAtPos(blockPos + Vector2Int.right);
                        if (right)
                            if (!sameBlocks.Contains(right) && right.OrderType == currentBlock.OrderType)
                            {
                                _firstGridPos = otherBlock.GridPos;
                                _swipeDir = Vector2Int.right;
                                return;
                            }
                
                        var up = GetBlockAtPos(blockPos + Vector2Int.up);
                        if(up)
                            if (!sameBlocks.Contains(up) && up.OrderType == currentBlock.OrderType)
                            {
                                _firstGridPos = otherBlock.GridPos;
                                _swipeDir = Vector2Int.up;
                                return;
                            }
                
                        var down = GetBlockAtPos(blockPos + Vector2Int.down);
                        if(down)
                            if (!sameBlocks.Contains(down) && down.OrderType == currentBlock.OrderType)
                            {
                                _firstGridPos = otherBlock.GridPos;
                                _swipeDir = Vector2Int.down;
                                return;
                            }
                    }
                }
            }
            
            // UpFirst
            for (var x = 0; x < GridSize.x; x++)
            {
                for (var y = 0; y < GridSize.y; y++)
                {
                    if (y > GridSize.y - MatchCount)
                        continue;
                    
                    Block otherBlock = null;
                    var sameBlocks = new List<Block>();
                    
                    var currentBlock = blocks[x, y];
                    sameBlocks.Add(currentBlock);
                    
                    for (var i = 1; i < MatchCount; i++)
                    {
                        if (currentBlock.OrderType == blocks[x, y + i].OrderType)
                            sameBlocks.Add(blocks[x, y + i]);
                        else if(!otherBlock)
                            otherBlock = blocks[x, y + i];
                    }
                    if (!(otherBlock == null) && sameBlocks.Count == MatchCount - 1)
                    {
                         var blockPos = otherBlock.GridPos;
                            
                            var left = GetBlockAtPos(blockPos + Vector2Int.left);
                            if(left)
                                if (!sameBlocks.Contains(left) && left.OrderType == currentBlock.OrderType)
                                {
                                    _firstGridPos = otherBlock.GridPos;
                                    _swipeDir = Vector2Int.left;
                                    return;
                                }
                
                            var right = GetBlockAtPos(blockPos + Vector2Int.right);
                            if (right)
                                if (!sameBlocks.Contains(right) && right.OrderType == currentBlock.OrderType)
                                {
                                    _firstGridPos = otherBlock.GridPos;
                                    _swipeDir = Vector2Int.right;
                                    return;
                                }
                
                            var up = GetBlockAtPos(blockPos + Vector2Int.up);
                            if(up)
                                if (!sameBlocks.Contains(up) && up.OrderType == currentBlock.OrderType)
                                {
                                    _firstGridPos = otherBlock.GridPos;
                                    _swipeDir = Vector2Int.up;
                                    return;
                                }
                
                            var down = GetBlockAtPos(blockPos + Vector2Int.down);
                            if(down)
                                if (!sameBlocks.Contains(down) && down.OrderType == currentBlock.OrderType)
                                {
                                    _firstGridPos = otherBlock.GridPos;
                                    _swipeDir = Vector2Int.down;
                                    return;
                                }
                    }
                }
            }
        }
        private Block GetBlockAtPos(Vector2Int pos)
        {
            if (pos.x < 0) return null;
            if (pos.y < 0) return null;
            if (pos.x >= GridSize.x) return null;
            if (pos.y >= GridSize.y) return null;

            return _BoardManager.Blocks[pos.x, pos.y];
        }

        private void OnOrderService(OrderType service)
        {
            if (CurrentOrder == service)
            {
                _CustomerManager.RemoveFirstCustomer();
            }
        }
    }
}
