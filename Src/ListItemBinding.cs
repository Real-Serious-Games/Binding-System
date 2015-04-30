using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG.Internal
{
    public interface IListItemBinding
    {
        int ItemIndex { get; }
    }

    public class ListItemBinding : IListItemBinding
    {
        private ITypedList list;
        private IValueBinding valueBinding;

        public ListItemBinding(int itemIndex, ITypedList list, IValueBinding valueBinding)
        {
            // TODO: Complete member initialization
            this.ItemIndex = itemIndex;
            this.list = list;
            this.valueBinding = valueBinding;
        }

        public int ItemIndex
        {
            get;
            private set;
        }
    }
}
