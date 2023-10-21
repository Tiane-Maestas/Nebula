using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Nebula
{
    public class RandomOrderList<T> : IEnumerable
    {
        private List<T> _list;

        public T this[int i]
        {
            get { return _list[i]; }
            set { _list[i] = value; }
        }

        public int Count
        {
            get { return _list.Count; }
            private set { Count = value; }
        }

        public RandomOrderList()
        {
            _list = new List<T>();
        }

        public void Add(T element)
        {
            _list.Add(element);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)_list.GetEnumerator();
        }

        public void Shuffle()
        {
            for (int i = 0; i < _list.Count; i++)
            {
                int randomIndex = Random.Range(0, _list.Count);
                T temp = _list[randomIndex];
                _list[randomIndex] = _list[i];
                _list[i] = temp;
            }
        }

        public override string ToString()
        {
            string result = "[";
            for (int i = 0; i < _list.Count; i++)
            {
                if (i == _list.Count - 1)
                {
                    result += _list[i].ToString() + "]";
                }
                else
                {
                    result += _list[i].ToString() + ", ";
                }
            }
            return result;
        }
    }
}