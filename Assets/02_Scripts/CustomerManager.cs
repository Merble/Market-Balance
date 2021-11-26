using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MarketBalance
{
    public class CustomerManager : MonoBehaviour
    {
        public event Action CustomerDidEnd;
        
        private const float MoveDuration = 0.4f;

        [SerializeField] private Customer[] _CustomerPrefabs = new Customer[1];
        [SerializeField] private int _CustomerList;
        [SerializeField] private int _CustomersToAdd;
        
        private readonly List<Customer> _customers = new List<Customer>();
        [Space]
        [SerializeField] private float _CustomerMinScale = .15f;
        [SerializeField] private float _CustomerMaxScale = .8f;
        [SerializeField] private float _CustomerCreationDuration = .2f;
        
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
            
            newCustomer.transform.localPosition = GetLocalPosition(y);
            
            newCustomer.transform.localScale = Vector3.one * _CustomerMinScale;
            LeanTween.scale(newCustomer.gameObject, Vector3.one * _CustomerMaxScale, MoveDuration);
        }
        
        private Vector3 GetLocalPosition(int index)
        {
            return  new Vector3(0, 0,  -index);
        }
        
        private Customer CreateRandomCustomer()
        {
            var randomCustomer = _CustomerPrefabs[Random.Range(0, _CustomerPrefabs.Length)];
            var newObject = Instantiate(randomCustomer, transform, false);

            return newObject;
        }
        
        public void AddCustomers()
        {
            for (var y = _customers.Count; y < _CustomerList; y++)
            {
                if (_CustomersToAdd <= 0)
                {
                    break;
                }
                _CustomersToAdd--;
                CreateRandomCustomerAtPos(y);
            }
        }
        private void UpdateCustomerPositions()
        {
            for (var index = 0; index < _customers.Count; index++)
            {
                var customer = _customers[index];
                var localPos = GetLocalPosition(index);
                
                LeanTween.moveLocal(customer.gameObject, localPos, MoveDuration);
            }
        }
        
        public void RemoveFirstCustomer()
        {
            LeanTween.scale(_customers[0].gameObject, Vector3.one *_CustomerMinScale, _CustomerCreationDuration).setOnComplete(DestroyCustomer);
            void DestroyCustomer()
            {
                Destroy(_customers[0].gameObject);
                _customers.RemoveAt(0);
            }
            
            if (!_customers.Any())
            {
                CustomerDidEnd?.Invoke();
                return;
            }
            
            StartCoroutine(DoAfter(.5f, UpdateCustomerPositions));
        }

        private IEnumerator DoAfter(float waitTime, Action callback)
        {
            yield return new WaitForSeconds(waitTime);
            
            callback?.Invoke();
        }
    }
}
