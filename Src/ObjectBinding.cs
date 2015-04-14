using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
    public class ObjectBinding
    {
        private object bindingObject;
        private BindingsFactory bindingsFactory;

        public ObjectBinding(object bindingObject, BindingsFactory bindingsFactory)
        {
            // TODO: Complete member initialization
            this.bindingObject = bindingObject;
            this.bindingsFactory = bindingsFactory;
        }

        public IObservable<BoundPropertyChangingEventArgs> PropertyChangingEventStream { get; set; }

        public IObservable<BoundPropertyChangedEventArgs> PropertyChangedEventStream { get; set; }

        public IObservable<BoundCollectionChangedEventArgs> CollectionChangedEventStream { get; set; }

        internal void Disconnect()
        {
            throw new NotImplementedException();
        }

        internal IBinding FindNestedBinding(string bindingName)
        {
            throw new NotImplementedException();
        }
    }
}
