using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RSG
{
    public class ObservableList<T> : ObservableCollection<T>, ITypedList //fio:
    {
        public Type ItemType
        {
            get 
            {
                return typeof(T);
            }
        }

        public int IndexOf(object item)
        {
            return base.IndexOf((T)item);
        }

        public void Insert(int index, object item)
        {
            base.Insert(index, (T)item);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (T)value;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return true;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public int Add(object item)
        {
            base.Add((T)item);

            return this.Count - 1;
        }

        public bool Contains(object value)
        {
            return base.Contains((T)value);
        }

        public void CopyTo(Array array, int arrayIndex)
        {
            base.CopyTo((T[])array, arrayIndex);
        }

        public void Remove(object item)
        {
            base.Remove((T)item);
        }
    }
}
