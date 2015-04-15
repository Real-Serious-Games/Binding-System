using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
    public interface IObjectPropertyBinding : IBinding
    {
        string PropertyName { get; set; }

        IBinding FindNestedBinding(string bindingName);

        IObservable<BoundCollectionChangedEventArgs> CollectionChangedEventStream { get; set; }

        IObservable<BoundPropertyChangingEventArgs> PropertyChangingEventStream { get; set; }

        IObservable<BoundPropertyChangedEventArgs> PropertyChangedEventStream { get; set; }

        void Disconnect();

        void Connect(object obj);
    }
}
