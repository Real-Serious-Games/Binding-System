using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
    public interface IBindingsFactory
    {
        IObjectPropertyBinding[] CreateObjectBindings(object obj);

        IListItemBinding[] CreateListBindings(ITypedList list);

        IListItemBinding CreateListBinding(object item, int itemIndex, ITypedList list);

        IArrayItemBinding[] CreateArrayBindings(Array array);
    }
}
