using UnityEngine;

namespace MarketBalance
{
    public class InputHandler : MonoBehaviour
    {
        public delegate void DragEvent(Vector3 deltaInput);

        public event DragEvent OnDragStart;
        public event DragEvent OnDragEnd;
        
        private Vector3 _lastMousePosition;
        private Vector3  _mousePos;
        
        private bool _wasDown;
            
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