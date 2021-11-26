using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MarketBalance
{
    public class EnemyAI : MonoBehaviour
    {
        public event Action EnemyServiceDidSuccess;
        
        [SerializeField] private BoardManager _BoardManager;
        [SerializeField] private CustomerManager _CustomerManager;
        [SerializeField] private float _SwapInterval;
        
        private float _lastSwapTime;
        
        private OrderType? CurrentOrder => _CustomerManager.FirstOrder;
        public CustomerManager CustomerManager => _CustomerManager;
        public BoardManager BoardManager => _BoardManager;
        
        private void Awake()
        {
            _lastSwapTime = 0f;
            
            _BoardManager.OrderDidService += OrderDidService;
            _BoardManager.AutoServiceDidStop += _CustomerManager.AddCustomers;
        }
        private void Update()
        {
            if (_BoardManager.IsInputAllowed && _lastSwapTime + _SwapInterval < Time.time)
            {
                SwapTiles();
            }
        }
        
        private void OrderDidService(OrderType service)
        {
            if (CurrentOrder != service) return;
            
            _CustomerManager.RemoveFirstCustomer();
            EnemyServiceDidSuccess?.Invoke();
        }
        
        private void SwapTiles()
        {
            BoardManager.BlockToSwapResult blocksToSwap;
            blocksToSwap.Position = Vector2Int.zero;
            blocksToSwap.Direction = Vector2Int.zero;
            blocksToSwap.Type = null;
            blocksToSwap.IsNull = true;
            
            var blocksToSwapResults = _BoardManager.FindTheBlocksToSwap();
            if (blocksToSwapResults.Any())
                foreach (var blockToSwapResult in blocksToSwapResults.Where(blockToSwapResult =>
                    blockToSwapResult.Type == CurrentOrder))
                {
                    blocksToSwap = blockToSwapResult;
                    break;
                }
            else
                return;

            if (blocksToSwap.IsNull)
            {
                blocksToSwap = blocksToSwapResults[Random.Range(0, blocksToSwapResults.Count)];
            }
            
            _BoardManager.SwapBlocks(blocksToSwap.Position, blocksToSwap.Direction);
            
            _lastSwapTime = Time.time;
        }
    }
}
