using RSG.Internal;
using RSG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace RSG
{
    public class ArrayBinding : IValueBinding
    {
        /// <summary>
        /// The object that bindings are managed for.
        /// </summary>
        private Array array;

        /// <summary>
        /// Factory used to create child bindings.
        /// </summary>
        private IBindingsFactory bindingsFactory;

        /// <summary>
        /// Bindings for items in the list.
        /// </summary>
        private IArrayItemBinding[] bindings;

        /// <summary>
        /// Stream of property changing events.
        /// </summary>
        private BindableStream<BoundPropertyChangingEventArgs> propertyChangingEventStream = new BindableStream<BoundPropertyChangingEventArgs>();

        /// <summary>
        /// Stream of property changed events.
        /// </summary>
        private BindableStream<BoundPropertyChangedEventArgs> propertyChangedEventStream = new BindableStream<BoundPropertyChangedEventArgs>();

        /// <summary>
        /// Stream of collection changed events.
        /// </summary>
        private BindableStream<BoundCollectionChangedEventArgs> collectionChangedEventStream = new BindableStream<BoundCollectionChangedEventArgs>();

        /// <summary>
        /// Stream of property changing events.
        /// </summary>
        public IObservable<BoundPropertyChangingEventArgs> PropertyChangingEventStream
        {
            get
            {
                return propertyChangingEventStream;
            }
        }

        /// <summary>
        /// Stream of property changed events.
        /// </summary>
        public IObservable<BoundPropertyChangedEventArgs> PropertyChangedEventStream
        {
            get
            {
                return propertyChangedEventStream;
            }
        }

        /// <summary>
        /// Stream of collection changed events.
        /// </summary>
        public IObservable<BoundCollectionChangedEventArgs> CollectionChangedEventStream
        {
            get
            {
                return collectionChangedEventStream;
            }
        }

        public ArrayBinding(Array array, IBindingsFactory bindingsFactory)
        {
            Argument.NotNull(() => array);
            Argument.NotNull(() => bindingsFactory);

            this.bindingsFactory = bindingsFactory;

            Connect(array);
        }

        /// <summary>
        /// Connect the binding to an array.
        /// </summary>
        public void Connect(object obj)
        {
            Argument.NotNull(() => obj);

            Disconnect();

            array = (Array)obj;
            bindings = bindingsFactory.CreateArrayBindings(array);

            propertyChangingEventStream.Bind(Observable
                .Merge(this.bindings
                    .Select(b => b.PropertyChangingEventStream)
                ));

            propertyChangedEventStream.Bind(Observable
                .Merge(this.bindings
                    .Select(b => b.PropertyChangedEventStream)
                ));

            collectionChangedEventStream.Bind(Observable
                .Merge(this.bindings
                    .Select(b => b.CollectionChangedEventStream)
                ));
        }

        /// <summary>
        /// Disconnect the binding from an array.
        /// </summary>
        public void Disconnect()
        {
            if (this.array == null)
            {
                return;
            }

            collectionChangedEventStream.Unbind();
            propertyChangedEventStream.Unbind();
            propertyChangingEventStream.Unbind();

            foreach (var binding in this.bindings)
            {
                binding.Disconnect();
            }

            bindings = null;
            this.array = null;
        }

        /// <summary>
        /// Find a binding nested in the tree of property bidnings.
        /// </summary>
        public IBinding FindNestedBinding(string bindingName)
        {
            Argument.StringNotNullOrEmpty(() => bindingName);

            string listIndexStr;
            int listIndexEnd = bindingName.IndexOf('.');
            if (listIndexEnd != -1)
            {
                listIndexStr = bindingName.Substring(0, listIndexEnd);
            }
            else
            {
                listIndexStr = bindingName;
            }

            int itemIndex;
            if (!Int32.TryParse(listIndexStr, out itemIndex))
            {
                throw new ApplicationException("Invalid list index.");
            }

            var binding = this.bindings.SingleOrDefault(o => o.ItemIndex == itemIndex);
            if (binding == null)
            {
                throw new ApplicationException("List element with index '" + itemIndex + "' has no binding.");
            }

            if (listIndexEnd == -1)
            {
                return binding;
            }
            else
            {
                var remainingBindingName = bindingName.Substring(listIndexEnd + 1);
                return binding.FindNestedBinding(remainingBindingName);
            }
        }
    }
}
