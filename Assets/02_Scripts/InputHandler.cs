using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MarketBalance
{
    public class InputHandler : MonoBehaviour
    {
        private bool _wasDown;
        
        private Vector3 _lastMousePosition;
        private Vector3  _mousePos;
        
        public delegate void DragEvent(Vector3 deltaInput);

        public event DragEvent OnDragStart;
        public event DragEvent OnDragEnd;
            
        void Update()
        {
            var isDown = Input.GetMouseButton(0);

            // Drag start
            if (isDown && !_wasDown)
            {
                _mousePos = Input.mousePosition;
                OnDragStart?.Invoke(_mousePos);
            }
            
            // Drag end
            if (!isDown && _wasDown)
            {
                _lastMousePosition = Input.mousePosition;
                
                var dir = _lastMousePosition - _mousePos;
                
                OnDragEnd?.Invoke(dir);
            }
            
            _wasDown = isDown;
        }
    }
}