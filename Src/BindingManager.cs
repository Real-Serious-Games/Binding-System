using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using RSG.Utils;

namespace RSG
{
    /// <summary>
    /// Application of the Facade pattern. IBindingManager provides an insulated entry point to the binding system.
    /// The binding system is used to monitor an object tree for changes.
    /// </summary>
    public interface IBindingManager
    {
        /// <summary>
        /// Disconnect the binding
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Find a binding nested in the tree of property bidnings.
        /// </summary>
        IBinding FindNestedBinding(string bindingName);

        /// <summary>
        /// Stream of property changing events.
        /// </summary>
        IObservable<BoundPropertyChangingEventArgs> PropertyChangingEventStream { get; }

        /// <summary>
        /// Stream of property changed events.
        /// </summary>
        IObservable<BoundPropertyChangedEventArgs> PropertyChangedEventStream { get; }

        /// <summary>
        /// Event raised when a value in the binding tree has changed.
        /// </summary>
        IObservable<BoundCollectionChangedEventArgs> CollectionChangedEventStream { get; }
    }

    /// <summary>
    /// Application of the Facade pattern. IBindingManager provides an insulated entry point to the binding system.
    /// The binding system is used to monitor an object tree for changes.
    /// </summary>
    public class BindingManager : IBindingManager
    {
        /// <summary>
        /// Binds to the root object in the object graph.
        /// </summary>
        private ObjectBinding binding;

        /// <summary>
        /// Stream of property changing events.
        /// </summary>
        public IObservable<BoundPropertyChangingEventArgs> PropertyChangingEventStream
        {
            get
            {
                return binding.PropertyChangingEventStream;
            }
        }

        /// <summary>
        /// Stream of property changed events.
        /// </summary>
        public IObservable<BoundPropertyChangedEventArgs> PropertyChangedEventStream
        {
            get
            {
                return binding.PropertyChangedEventStream;
            }
        }

        /// <summary>
        /// Event raised when a value in the binding tree has changed.
        /// </summary>
        public IObservable<BoundCollectionChangedEventArgs> CollectionChangedEventStream
        {
            get
            {
                return binding.CollectionChangedEventStream;
            }
        }


        public BindingManager(object bindingObject)
        {
            Argument.NotNull(() => bindingObject);
            
            this.binding = new ObjectBinding(bindingObject, new BindingsFactory());
        }

        /// <summary>
        /// Stop monitoring object graph.
        /// </summary>
        public void Disconnect()
        {
            if (binding == null)
            {
                return;
            }

            binding.Disconnect();

            binding = null;
        }

        /// <summary>
        /// Find a binding nested in the tree of property binfings.
        /// </summary>
        public IBinding FindNestedBinding(string bindingName)
        {
            Argument.StringNotNullOrEmpty(() => bindingName);

            return binding.FindNestedBinding(bindingName);
        }
    }
}
