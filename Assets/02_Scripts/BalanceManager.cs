using UnityEngine;

namespace MarketBalance
{
    public class BalanceManager : MonoBehaviour
    {
        [SerializeField] private float _ServiceEffectOnBalance;
        [SerializeField] private float _MaxRotationAngle;
        
        [SerializeField] private float _BalanceValue;   // Between (0, 1)
        
        [SerializeField] private float _RotationSpeed;

        private void Awake()
        {
            _BalanceValue = .5f;
        }
        
        public void BalanceChange(bool isPlayerDidService)
        {
            _BalanceValue += (isPlayerDidService ? 1 : -1) * _ServiceEffectOnBalance;
            _BalanceValue = Mathf.Clamp(_BalanceValue, 0, 1);
            
            RotateBoardToCurrentBalance();

            if (_BalanceValue >= 1f)
            {
                // TODO: Player Wins
                Debug.Log("Player Wins");
            }

            if (_BalanceValue <= 0)
            {
                // TODO: Enemy Wins
                Debug.Log("Enemy Wins");
            }
        }

        public void SetBalance(float val)
        {
            _BalanceValue = val;
            RotateBoardToCurrentBalance();
        }

        private void RotateBoardToCurrentBalance()
        {
            var newAngle = Mathf.Lerp(-_MaxRotationAngle, _MaxRotationAngle, _BalanceValue);
            var angle = transform.rotation.eulerAngles.x;

            if (angle > 180)
            {
                angle -= 360;
            }

            var angleDistance = Mathf.Abs(newAngle - angle) / _RotationSpeed;
            var rotationEuler = new Vector3(newAngle, 0, 0);

            LeanTween.cancel(gameObject);
            LeanTween.rotateLocal(gameObject, rotationEuler, angleDistance);
    }
    }
}
