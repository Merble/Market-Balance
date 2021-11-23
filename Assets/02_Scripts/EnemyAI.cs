using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace MarketBalance
{
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private BoardManager _BoardManager;
        [SerializeField] private CustomerManager _CustomerManager;
        [SerializeField] private float _SwapInterval;
        
        private float _lastSwapTime;
        
        private OrderType? CurrentOrder => _CustomerManager.FirstOrder;
        private Vector2Int GridSize => _BoardManager.GridSize;
        private int MatchCount => _BoardManager.MatchCount;
        
        private void Awake()
        {
            _lastSwapTime = 0f;
            
            _BoardManager.OnOrderService += OnOrderService;
            _BoardManager.OnAutoServiceStop += _CustomerManager.AddCustomers;
        }
        private void Update()
        {
            if (_BoardManager.IsInputAllowed && _lastSwapTime + _SwapInterval < Time.time)
            {
                SwapTiles();
            }
        }
        
        private void SwapTiles()
        {
            var blocksToSwap = FindTheBlocksToSwap();
            
            if(!blocksToSwap.IsNull)
                _BoardManager.SwapBlocks(blocksToSwap.Position, blocksToSwap.Direction);
            _lastSwapTime = Time.time;
        }

        private struct BlockToSwapResult
        {
            public Vector2Int Position;
            public Vector2Int Direction;
            public bool IsNull;
        }
        
        private BlockToSwapResult FindTheBlocksToSwap()     // An algorithm that will find a valid swipe option on the board.
        {
            var blocks = _BoardManager.Blocks;
            BlockToSwapResult blocksToSwap;
            
            // RightFirst
            for (var y = 0; y < GridSize.y; y++)
            {
                for (var x = 0; x < GridSize.x; x++)
                {
                    if (x > GridSize.x - MatchCount)
                        continue;
                    
                    // Gather all matchSize number of blocks
                    var checkingBlocks = new List<Block>();
                    for (var i = 0; i < MatchCount; i++)
                    {
                        checkingBlocks.Add(blocks[x + i, y]);
                    }

                    var otherBlockIndex = GetSingleDifferentBlockIndex(checkingBlocks);
                    if (otherBlockIndex < 0) continue;

                    var otherBlock = checkingBlocks[otherBlockIndex];
                    checkingBlocks.RemoveAt(otherBlockIndex);

                    blocksToSwap = GetBlocksToSwap(otherBlock, checkingBlocks);
                    if(!blocksToSwap.IsNull) 
                        return blocksToSwap;
                }
            }
            
            // UpFirst
            for (var x = 0; x < GridSize.x; x++)
            {
                for (var y = 0; y < GridSize.y; y++)
                {
                    if (y > GridSize.y - MatchCount)
                        continue;
                    
                    // Gather all matchSize number of blocks
                    var checkingBlocks = new List<Block>();
                    for (var i = 0; i < MatchCount; i++)
                    {
                        checkingBlocks.Add(blocks[x, y + i]);
                    }

                    var otherBlockIndex = GetSingleDifferentBlockIndex(checkingBlocks);
                    if (otherBlockIndex < 0) continue;

                    var otherBlock = checkingBlocks[otherBlockIndex];
                    checkingBlocks.RemoveAt(otherBlockIndex);

                    blocksToSwap = GetBlocksToSwap(otherBlock, checkingBlocks);
                    if(!blocksToSwap.IsNull) 
                        return blocksToSwap;
                }
            }
            
            blocksToSwap.Position = Vector2Int.zero;
            blocksToSwap.Direction = Vector2Int.zero;
            blocksToSwap.IsNull = true;
            
            return blocksToSwap;
        }

        private int GetSingleDifferentBlockIndex(List<Block> blocksToCheck)
        {
            Assert.IsTrue(blocksToCheck.Count >= 3);

            OrderType? mainType = null;

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
                    blocksToSwap.IsNull = false;
                    
                    return blocksToSwap;
                }

            var right = GetBlockAtPos(blockPos + Vector2Int.right);
            if (right)
                if (!sameBlocks.Contains(right) && right.OrderType == sameType)
                {
                    blocksToSwap.Position = otherBlock.GridPos;
                    blocksToSwap.Direction = Vector2Int.right;
                    blocksToSwap.IsNull = false;

                    return blocksToSwap;
                }

            var up = GetBlockAtPos(blockPos + Vector2Int.up);
            if (up)
                if (!sameBlocks.Contains(up) && up.OrderType == sameType)
                {
                    blocksToSwap.Position = otherBlock.GridPos;
                    blocksToSwap.Direction = Vector2Int.up;
                    blocksToSwap.IsNull = false;

                    return blocksToSwap;
                }

            var down = GetBlockAtPos(blockPos + Vector2Int.down);
            if (down)
                if (!sameBlocks.Contains(down) && down.OrderType == sameType)
                {
                    blocksToSwap.Position = otherBlock.GridPos;
                    blocksToSwap.Direction = Vector2Int.down;
                    blocksToSwap.IsNull = false;

                    return blocksToSwap;
                }
            
            blocksToSwap.Position = Vector2Int.zero;
            blocksToSwap.Direction = Vector2Int.zero;
            blocksToSwap.IsNull = true;

            return blocksToSwap;
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
