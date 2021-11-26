using System;
using UnityEngine;

namespace MarketBalance
{
    public class Player : MonoBehaviour
    {
        public event Action PlayerServiceDidSuccess;
        
        [SerializeField] private CustomerManager _CustomerManager;
        [SerializeField] private BoardManager _BoardManager;
        
        private OrderType? CurrentOrder => _CustomerManager.FirstOrder;
        public CustomerManager CustomerManager => _CustomerManager;
        public BoardManager BoardManager => _BoardManager;

        private void Awake()
        {
            _BoardManager.OrderDidService += OrderDidService;
            _BoardManager.AutoServiceDidStop += _CustomerManager.AddCustomers;
        }
        
        private void OrderDidService(OrderType service)
        {
            if (CurrentOrder != service) return;
            
            _CustomerManager.RemoveFirstCustomer();
            PlayerServiceDidSuccess?.Invoke();
        }
    }
}
