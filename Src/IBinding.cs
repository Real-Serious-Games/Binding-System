using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
    /// <summary>
    /// Bind to a property of an object or an element in a list or an array.
    /// Receive notifications when the value changes, or when the value of a nested binding changes.
    /// </summary>
    public interface IBinding
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
        /// The binding to the value of the property or array/list element.
        /// </summary>
        IValueBinding ValueBinding { get; }

        /// <summary>
        /// The type of the binding.
        /// </summary>
        Type BindingType { get; }

        /// <summary>
        /// Retreive the value.
        /// </summary>
        object GetValue();

        /// <summary>
        /// Set the value.
        /// </summary>
        void SetValue(object newValue);

        /// <summary>
        /// Reattach the property to the property in the specified object.
        /// </summary>
        void Connect(object parentObject);

        /// <summary>
        /// Disconnected events from the binding.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Find a binding nested in the tree of property bidnings.
        /// </summary>
        IBinding FindNestedBinding(string bindingName);
    }
}
