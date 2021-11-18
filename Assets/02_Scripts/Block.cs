using UnityEngine;
using UnityEngine.Serialization;

namespace MarketBalance
{
    public class Block : MonoBehaviour
    {
        [FormerlySerializedAs("_Type")] [SerializeField] private OrderType _OrderType;

        public OrderType OrderType => _OrderType;

        public Vector2Int GridPos { get; set; }
        
    }
}
