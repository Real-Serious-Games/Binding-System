using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace RSG.Internal
{
    public class PrimitiveBinding : IValueBinding
    {
        private object value;

        public PrimitiveBinding(object value)
        {
        }

        public IObservable<BoundPropertyChangingEventArgs> PropertyChangingEventStream
        {
            get
            {
                return Observable.Empty<BoundPropertyChangingEventArgs>();
            }
        }

        public IObservable<BoundPropertyChangedEventArgs> PropertyChangedEventStream
        {
            get
            {
                return Observable.Empty<BoundPropertyChangedEventArgs>();
            }
        }

        public IObservable<BoundCollectionChangedEventArgs> CollectionChangedEventStream
        {
            get
            {
                return Observable.Empty<BoundCollectionChangedEventArgs>();
            }
        }

        public void Connect(object obj)
        {
        }

        public void Disconnect()
        {
        }

        public IBinding FindNestedBinding(string bindingName)
        {
            throw new ApplicationException("Shouldn't get here! Attempting to resolve nested binding: " + bindingName);
        }
    }
}
