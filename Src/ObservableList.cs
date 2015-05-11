using System;
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
    }
}
