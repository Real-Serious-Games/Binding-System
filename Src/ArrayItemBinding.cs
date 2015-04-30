using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG.Internal
{
    public interface IArrayItemBinding
    {
        int ItemIndex { get; }
    }

    public class ArrayItemBinding : IArrayItemBinding
    {
        private Array array;
        private IValueBinding valueBinding;

        public ArrayItemBinding(int itemIndex, Array array, IValueBinding valueBinding)
        {
            // TODO: Complete member initialization
            this.ItemIndex = itemIndex;
            this.array = array;
            this.valueBinding = valueBinding;
        }

        public int ItemIndex
        {
            get;
            private set;
        }
    }
}
