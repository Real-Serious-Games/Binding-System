using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG.Internal
{
    public class ListBinding : IValueBinding
    {
        private ITypedList typedList;
        private BindingsFactory bindingsFactory;

        public ListBinding(ITypedList typedList, BindingsFactory bindingsFactory)
        {
            // TODO: Complete member initialization
            this.typedList = typedList;
            this.bindingsFactory = bindingsFactory;
        }

        public IObservable<BoundPropertyChangingEventArgs> PropertyChangingEventStream
        {
            get { throw new NotImplementedException(); }
        }

        public IObservable<BoundPropertyChangedEventArgs> PropertyChangedEventStream
        {
            get { throw new NotImplementedException(); }
        }

        public IObservable<BoundCollectionChangedEventArgs> CollectionChangedEventStream
        {
            get { throw new NotImplementedException(); }
        }

        public void Connect(object obj)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public IBinding FindNestedBinding(string bindingName)
        {
            throw new NotImplementedException();
        }
    }
}
