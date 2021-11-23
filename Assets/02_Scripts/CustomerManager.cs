using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MarketBalance
{
    public class CustomerManager : MonoBehaviour
    {
        private const float MoveDuration = 0.4f;
        
        public event BoardManager.OrderEvent OnOrderChange;
        
        [SerializeField] private Customer[] _CustomerPrefabs = new Customer[1];
        [SerializeField] private int _CustomerList;
        [SerializeField] private int _CustomersToAdd;
        
        private readonly List<Customer> _customers = new List<Customer>();
        
        private List<OrderType> OrderList => _customers.Select(customer => customer.Order).ToList();
        
        public OrderType? FirstOrder
        {
            get
            {
                if (!OrderList.Any()) return null;
                return OrderList[0];
            }
        }

        private void Awake()
        {
            CreateCustomerLine();
        }
        
        private void CreateCustomerLine()
        {
            for (var y = 0; y < _CustomerList; y++)
            {
                CreateRandomCustomerAtPos(y);
            }
        }
        private void CreateRandomCustomerAtPos(int y)
        {
            var newCustomer = CreateRandomCustomer();
            _customers.Add(newCustomer);

            newCustomer.transform.position = GetWorldPosition(y);
        }
        
        private Vector3 GetWorldPosition(int index)
        {
            var startPosition = transform.position;

            return startPosition + new Vector3(0, 0,  -index);
        }
        
        private Customer CreateRandomCustomer()
        {
            var randomCustomer = _CustomerPrefabs[Random.Range(0, _CustomerPrefabs.Length)];
            var newObject = Instantiate(randomCustomer, transform, true);

            return newObject;
        }
        
        public void AddCustomers()
        {
            for (var y = _customers.Count; y < _CustomerList; y++)
            {
                if (_CustomersToAdd <= 0)
                {
                    // TODO: Let the game know that this controller did it. End of the level for enemy or player :)
                    break;
                }
                
                _CustomersToAdd--;
                CreateRandomCustomerAtPos(y);
            }
            OnOrderChange?.Invoke(_customers[0].Order);
        }
        private void UpdateCustomerPositions()
        {
            for (var index = 0; index < _customers.Count; index++)
            {
                var customer = _customers[index];
                var pos = GetWorldPosition(index);
                LeanTween.move(customer.gameObject, pos, MoveDuration);
            }
        }
        
        public void RemoveFirstCustomer()
        {
            Destroy(_customers[0].gameObject);
            _customers.RemoveAt(0);
            
            if (!_customers.Any())
            {
                // TODO: Emit event to notify end of customers
                Debug.Log("Customers ended.");
                return;
            }
            
            OnOrderChange?.Invoke(_customers[0].Order);
            
            StartCoroutine(DoAfter(.5f, UpdateCustomerPositions));
        }

        [Button]
        private void LogOrderList()
        {
            foreach (var order in OrderList)
            {
                Debug.Log(order);
            }
        }
        
        private IEnumerator DoAfter(float waitTime, Action callback)
        {
            yield return new WaitForSeconds(waitTime);
            
            callback?.Invoke();
        }
    }
}
