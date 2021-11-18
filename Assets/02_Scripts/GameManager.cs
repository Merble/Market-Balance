using Sirenix.OdinInspector;
using UnityEngine;

namespace MarketBalance
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private CustomerManager _CustomerManager;
        [SerializeField] private BoardManager _BoardManager;

        private OrderType? CurrentOrder => _CustomerManager.FirstOrder;
        
        private void Awake()
        {
            _BoardManager.OnOrderService += OnOrderServiceMatch;
        }
        
        private void OnOrderServiceMatch(OrderType service)
        {
            if (CurrentOrder == service)
            {
                _CustomerManager.RemoveFirstCustomer();
            }
        }
    }
}
