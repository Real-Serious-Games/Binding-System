using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG.Internal
{
    public class PrimitiveBinding : IValueBinding
    {
        private object value;

        public PrimitiveBinding(object value)
        {
            // TODO: Complete member initialization
            this.value = value;
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
