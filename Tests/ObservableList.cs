using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RSG
{
    /// <summary>
    /// A list that exposes the type of its items as a property.
    /// </summary>
    public class ObservableList<T> : ObservableCollection<T>, ITypedList
    {
        /// <summary>
        /// Convert a variable length argument list of items to an ObservableList.
        /// </summary>
        public static ObservableList<T> FromItems(params T[] items)
        {
            return new ObservableList<T>(items);
        }

        public ObservableList()
        {
        }

        public ObservableList(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.Add(item);
            }
        }

        /// <summary>
        /// Specifies the type of items in the list.
        /// </summary>
        public Type ItemType
        {
            get
            {
                return typeof(T);
            }
        }
    }
}
