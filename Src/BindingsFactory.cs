using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
    public class BindingsFactory : IBindingsFactory
    {
        public IObjectPropertyBinding[] CreateObjectBindings(object obj)
        {
            throw new NotImplementedException();
        }

        public IListItemBinding[] CreateListBindings(ITypedList list)
        {
            throw new NotImplementedException();
        }

        public IListItemBinding CreateListBinding(object item, int itemIndex, ITypedList list)
        {
            throw new NotImplementedException();
        }
    }
}
