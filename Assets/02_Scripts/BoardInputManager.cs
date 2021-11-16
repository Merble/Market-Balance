using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MarketBalance
{
    public class BoardInputManager : MonoBehaviour
    {
        [SerializeField] private InputHandler _InputHandler;
        [SerializeField] private BoardManager _BoardManager;

        private Vector3 _mouseStartPos;
        private void Awake()
        { 
            _InputHandler.OnDragStart += MouseClickStart;
            _InputHandler.OnDragEnd += MouseClickEnd;
        }
        
        private void MouseClickStart(Vector3 mousePos)
        {
            _mouseStartPos = mousePos;
        }

        private void MouseClickEnd(Vector3 mousePos)
        {
            var dir = GetSwipeDirection(mousePos.x, mousePos.y);

            _BoardManager.SwapBlocks(_mouseStartPos, dir);
        }

        private Vector2Int GetSwipeDirection(float x, float y)
        {
            var isHorizontal = Mathf.Abs(x) > Mathf.Abs(y);
            if (isHorizontal)
            {
                return new Vector2Int(Mathf.RoundToInt(Mathf.Sign(x)), 0);
            }
            return new Vector2Int(0, -Mathf.RoundToInt(Mathf.Sign(x)));
            
            /*switch (x >= 0)
            {
                case true when y >= 0:
                {
                    if (Mathf.Abs(x) > Mathf.Abs(y))
                        return Vector2Int.right;

                    if (Mathf.Abs(x) < Mathf.Abs(y))
                        return Vector2Int.up;
                    break;
                }
                case true when y <= 0:
                {
                    if (Mathf.Abs(x) > Mathf.Abs(y))
                        return Vector2Int.right;

                    if (Mathf.Abs(x) < Mathf.Abs(y))
                        return Vector2Int.down;
                    break;
                }
            }
            switch (x <= 0)
            {
                case true when y <= 0:
                {
                    if (Mathf.Abs(x) > Mathf.Abs(y))
                        return Vector2Int.left;

                    if (Mathf.Abs(x) < Mathf.Abs(y))
                        return Vector2Int.down;
                    break;
                }
                case true when y >= 0:
                {
                    if (Mathf.Abs(x) > Mathf.Abs(y))
                        return Vector2Int.left;

                    if (Mathf.Abs(x) < Mathf.Abs(y))
                        return Vector2Int.up;
                    break;
                }
            }
            return Vector2Int.zero;*/
        }
    }
}
