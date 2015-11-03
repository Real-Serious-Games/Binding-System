using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
    public class ObservableCollection<T> : IList<T>, IList, INotifyCollectionChanged
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

        public bool IsFixedSize
        {
            get
            {
                return true;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return innerList[index];
            }
            set
            {
                innerList[index] = (T)value;
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

        public int Add(object item)
        {
            innerList.Add((T)item);
            return innerList.Count - 1;
        }

        public bool Contains(object item)
        {
            return innerList.Contains((T)item);
        }

        public int IndexOf(object item)
        {
            return innerList.IndexOf((T)item);
        }

        public void Insert(int index, object item)
        {
            innerList.Insert(index, (T)item);
        }

        public void Remove(object item)
        {
            innerList.Remove((T)item);
        }

        public void CopyTo(Array array, int index)
        {
            innerList.CopyTo((T[])array, index);
        }
    }
}
