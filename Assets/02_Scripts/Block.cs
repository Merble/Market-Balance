using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarketBalance
{
    public enum BlockType
    {
        Yellow,
        Blue,
        Black,
        Green
    }
    
    public class Block : MonoBehaviour
    {
        [SerializeField] private BlockType _Type;

        public BlockType Type => _Type;

        public Vector2Int GridPos { get; set; }
        
    }
}
