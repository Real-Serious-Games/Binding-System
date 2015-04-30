using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RSG.Internal
{
    public class ObservableList<T> : ObservableCollection<T>, ITypedList
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
