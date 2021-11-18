using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MarketBalance
{
    public enum OrderType
    {
        Yellow,
        Blue,
        Black,
        Green
    }
    public class Customer : MonoBehaviour
    {
        [SerializeField] private OrderType _Order;
        
        public OrderType Order => _Order;
        
        private void Awake()
        {
            _Order = (OrderType) Random.Range( 0, Enum.GetNames(typeof(OrderType)).Length);
        }
    }
}
