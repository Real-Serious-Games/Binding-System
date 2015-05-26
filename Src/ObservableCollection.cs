using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
    public class ObservableCollection<T> : IList<T>, INotifyCollectionChanged
    {
        /// <summary>
        /// Inner (non-obsevable) list.
        /// </summary>
        private List<T> innerList = new List<T>();

        /// <summary>
        /// Event raised when the collection has been changed.
        /// </summary>
        public event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;

        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            innerList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            innerList.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return innerList[index];
            }
            set
            {
                innerList[index] = value;
            }
        }

        public void Add(T item)
        {
            innerList.Add(item);
        }

        public void Clear()
        {
            innerList.Clear();
        }

        public bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get 
            {
                return innerList.Count; 
            }
        }

        public bool IsReadOnly
        {
            get 
            { 
                return false;
            }
        }

        public bool Remove(T item)
        {
            return innerList.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
    }
}
