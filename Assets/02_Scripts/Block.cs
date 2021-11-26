using UnityEngine;

namespace MarketBalance
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private OrderType _OrderType;

        public OrderType OrderType => _OrderType;

        public Vector2Int GridPos { get; set; }
        
    }
}
