using UnityEngine;

namespace MarketBalance
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Player _Player;
        [SerializeField] private EnemyAI _Enemy;
        [SerializeField] private BalanceManager _BalanceManager;
        

        private void Awake()
        {
            _Player.PlayerServiceDidSuccess += () => { _BalanceManager.BalanceChange(true); };
            _Enemy.EnemyServiceDidSuccess += () => { _BalanceManager.BalanceChange(false); };
            
            _Player.CustomerManager.CustomerDidEnd += () => { _BalanceManager.SetBalance(1);};
            _Enemy.CustomerManager.CustomerDidEnd += () => { _BalanceManager.SetBalance(0);};
        }
        
    }
}
