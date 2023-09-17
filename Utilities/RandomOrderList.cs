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
            private set { _list[i] = value; }
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
    }
}