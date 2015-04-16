using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
    /// <summary>
    /// Maintains bindings for a value.
    /// </summary>
    public interface IValueBinding
    {
        /// <summary>
        /// Stream of property changing events.
        /// </summary>
        IObservable<BoundPropertyChangingEventArgs> PropertyChangingEventStream { get; }

        /// <summary>
        /// Stream of property changed events.
        /// </summary>
        IObservable<BoundPropertyChangedEventArgs> PropertyChangedEventStream { get; }

        /// <summary>
        /// Stream of collection changed events.
        /// </summary>
        IObservable<BoundCollectionChangedEventArgs> CollectionChangedEventStream { get; }

        /// <summary>
        /// Connect to a new bound object.
        /// </summary>
        void Connect(object obj);

        /// <summary>
        /// Disconnect from bound objects.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Find a binding nested in the tree of property bidnings.
        /// </summary>
        IBinding FindNestedBinding(string bindingName);
    }
}
