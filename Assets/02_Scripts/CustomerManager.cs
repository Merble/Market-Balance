using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MarketBalance
{
    public class CustomerManager : MonoBehaviour
    {
        [SerializeField] private Customer[] _CustomerPrefabs = new Customer[1];
        
        private List<Customer> _customers = new List<Customer>();
        //private Customer[] _customers = new Customer[6];
        
        // Linq
        private List<OrderType> OrderList => _customers.Select(customer => customer.Order).ToList();
        
        // public delegate void OrderEvent();
        // public event OrderEvent DidOrderMatch;
        
        private static float _MoveDuration = 0.4f;
        
        [SerializeField] private int _CustomerListNumber;
        
        [SerializeField] private int _AllCustomerNumber;

        private void Awake()
        {
            CreateCustomerLine();
        }
        
        private void CreateCustomerLine()
        {
            for (var y = 0; y < _CustomerListNumber; y++)
            {
                CreateRandomCustomerAtPos(y);
            }
        }
        private void CreateRandomCustomerAtPos(int y)
        {
            var newCustomer = CreateRandomCustomer();
            _customers.Add(newCustomer);

            newCustomer.transform.position = GetWorldPosFromArrayPos(y);
        }
        
        private Vector3 GetWorldPosFromArrayPos(int y)
        {
            var startPosition = transform.position;

            return startPosition + new Vector3(0, 0,  -y);
        }
        
        private Customer CreateRandomCustomer()
        {
            var randomCustomer = _CustomerPrefabs[Random.Range(0, _CustomerPrefabs.Length)];
            var newObject = Instantiate(randomCustomer, transform, true);

            return newObject;
        }
        
        [Button]
        private void SetCustomerLine()
        {
            MoveCustomersInLine();
            // for (var y = 0; y < _CustomerListNumber; y++)
            // {
            //     if (_customers[y] != null) continue;
            //     
            //     MoveCustomersInLine(y);
            //     break;
            // }
        }
        [Button]
        private void AddCustomers()
        {
            for (var y = _customers.Count; y < _CustomerListNumber; y++)
            {
                CreateRandomCustomerAtPos(y);
            }
            
            /*for (var y = 0; y < _CustomerListNumber; y++)
            {
                var customer = _customers[y];
                    
                if (!customer)
                {
                    CreateRandomCustomerAtPos(y);
                }
            }*/
        }
        private void MoveCustomersInLine()
        {
            foreach (var customer in _customers)
            {
                LeanTween.move(customer.gameObject, customer.transform.position + new Vector3(0, 0, 1), _MoveDuration);
            }
            
            /*var nullCount = 0;
            
            for (var y = yPos; y < _CustomerListNumber; y++)
            {
                var customer = _customers[y];
                
                if (customer == null)
                {
                    nullCount++;
                }
                else
                {
                    var newYPos = y - nullCount;
                    _customers[newYPos] = customer;
                    _customers[y] = null;

                    //_customerList[newYPos].ArrayPos = newYPos;
                    
                    LeanTween.move(_customers[newYPos].gameObject, GetWorldPosFromArrayPos(newYPos), _MoveDuration);
                }
            }*/
        }
        
        [Button]
        private void RemoveCustomerAtPos()
        {
            Destroy(_customers[0].gameObject);
            _customers.RemoveAt(0);
        }
        
        [Button]
        private void GetOrderList()
        {
            for (var y = 0; y < _CustomerListNumber; y++)
            {
                Debug.Log(OrderList[y]);
            }
        }
        
        // private void SetOrderList()
        // {
        //     for (var y = 0; y < _CustomerListNumber; y++)
        //     {
        //         
        //     }
        // }
    }
}
