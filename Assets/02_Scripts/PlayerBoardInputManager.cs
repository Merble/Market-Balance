using UnityEngine;

namespace MarketBalance
{
    public class PlayerBoardInputManager : MonoBehaviour
    {
        [SerializeField] private InputManager _InputManager;
        [SerializeField] private BoardManager _BoardManager;
        
        private Vector3 _mouseStartPos;

        private void Awake()
        { 
            _InputManager.OnDragStart += MouseClickStart;
            _InputManager.OnDragEnd += MouseClickEnd;
        }
        
        private void MouseClickStart(Vector3 mousePos)
        {
            _mouseStartPos = mousePos;
        }

        private void MouseClickEnd(Vector3 mousePos)
        {
            var dir = GetSwipeDirection(mousePos.x, mousePos.y);
            
            if (Camera.main is null) return; // Check if there is a camera
            
            var ray = Camera.main.ScreenPointToRay(_mouseStartPos);
            if (!Physics.Raycast(ray, out var hit)) return; // Check if raycast hit anything

            if (!hit.collider.GetComponent<Block>()) return; // Check if the hit object has "Block" component
            
            var gridPos = hit.collider.GetComponent<Block>().GridPos;
            
            // Check if mouse input allowed
            if (_BoardManager.IsInputAllowed) 
                _BoardManager.SwapBlocks(gridPos, dir);
        }

        private Vector2Int GetSwipeDirection(float x, float y)
        {
            var isHorizontal = Mathf.Abs(x) > Mathf.Abs(y);

            return isHorizontal ? new Vector2Int(Mathf.RoundToInt(Mathf.Sign(x)), 0) : new Vector2Int(0, Mathf.RoundToInt(Mathf.Sign(y)));
            
        }
    }
}
