using UnityEngine;

namespace MarketBalance
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private CustomerManager _CustomerManager;
        [SerializeField] private BoardManager _BoardManager;

        private OrderType? CurrentOrder => _CustomerManager.FirstOrder;
        
        private void Awake()
        {
            _BoardManager.OnOrderService += OnOrderService;
            _BoardManager.OnAutoServiceStop += _CustomerManager.AddCustomers;
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
