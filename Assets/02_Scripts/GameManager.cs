using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarketBalance
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private CustomerManager _CustomerManager;
        [SerializeField] private BoardManager _BoardManager;
        

        private void Awake()
        {
            //_CustomerManager.DidOrderMatch += OnOrderMatch;
        }

        private void OnOrderMatch()
        {
            
        }
    }
}
