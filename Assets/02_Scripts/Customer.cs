using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MarketBalance
{
    public enum Order
    {
        Yellow,
        Blue,
        Black,
        Green
    }
    public class Customer : MonoBehaviour
    {
        [SerializeField] private Order _Order;

        private void Awake()
        {
            _Order = (Order) Random.Range( 0, Enum.GetNames(typeof(Order)).Length);
        }
    }
}
