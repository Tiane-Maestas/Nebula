using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Nebula
{
    public class RandomOrderList<T> : IEnumerable
    {
        public List<T> List; // I made this public so I didn't have to wrap "OrderBy" method.

        public T this[int i]
        {
            get { return List[i]; }
            set { List[i] = value; }
        }

        public int Count
        {
            get { return List.Count; }
            private set { Count = value; }
        }

        public RandomOrderList()
        {
            List = new List<T>();
        }

        public void Add(T element)
        {
            List.Add(element);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)List.GetEnumerator();
        }

        public void Shuffle()
        {
            for (int i = 0; i < List.Count; i++)
            {
                int randomIndex = Random.Range(0, List.Count);
                T temp = List[randomIndex];
                List[randomIndex] = List[i];
                List[i] = temp;
            }
        }

        public override string ToString()
        {
            string result = "[";
            for (int i = 0; i < List.Count; i++)
            {
                if (i == List.Count - 1)
                {
                    result += List[i].ToString() + "]";
                }
                else
                {
                    result += List[i].ToString() + ", ";
                }
            }
            return result;
        }
    }
}