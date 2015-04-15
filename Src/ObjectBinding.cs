using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using RSG;
using RSG.Utils;


namespace RSG.Internal
{
    /// <summary>
    /// Binds an object value and all of its children.
    /// </summary>
    public class ObjectBinding : IValueBinding 
    {
        /// <summary>
        /// The object that bindings are anaged form.
        /// </summary>
        private object obj;

        /// <summary>
        /// Factory used to create child property bindings.
        /// </summary>
        private IBindingsFactory bindingsFactory;

        /// <summary>
        /// Properties of the object that are bound.
        /// </summary>
        private IObjectPropertyBinding[] bindings;

        private BindableStream<BoundPropertyChangingEventArgs> propertyChangingEventStream = new BindableStream<BoundPropertyChangingEventArgs>();

        private BindableStream<BoundPropertyChangedEventArgs> propertyChangedEventStream = new BindableStream<BoundPropertyChangedEventArgs>();

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
        /// Event raised when a value in the binding tree has changed.
        /// </summary>
        public IObservable<BoundCollectionChangedEventArgs> CollectionChangedEventStream
        {
            get
            {
                return collectionChangedEventStream;
            }
        }

        public ObjectBinding(object obj, IBindingsFactory bindingsFactory)
        {
            if (bindingsFactory == null)
            {
                throw new ArgumentNullException("bindingsFactory");
            }

            this.bindingsFactory = bindingsFactory;

            if (obj != null)
            {
                Connect(obj);
            }
        }

        /// <summary>
        /// Reattach the property to the property in the specified object.
        /// </summary>
        public void Connect(object obj)
        {
            Argument.NotNull(() => obj);

            Disconnect();

            this.obj = obj;
            this.bindings = this.bindingsFactory.CreateObjectBindings(obj);

            foreach (var binding in this.bindings)
            {
                binding.Connect(this.obj);
            };

            collectionChangedEventStream.Bind(Observable
                .Merge(this.bindings.Select(b => b.CollectionChangedEventStream)));

            propertyChangingEventStream.Bind(Observable
                .Merge(this.bindings.Select(b => b.PropertyChangingEventStream))
                .Merge(GetPropertyChangingEventStream())
            );

            propertyChangedEventStream.Bind(Observable
                .Merge(this.bindings.Select(b => b.PropertyChangedEventStream))
                .Merge(GetPropertyChangedEventStream())
            );
        }

        /// <summary>
        /// Disconnected events from the bound property.
        /// </summary>
        public void Disconnect()
        {
            if (this.obj == null)
            {
                return;
            }

            propertyChangedEventStream.Unbind();
            propertyChangingEventStream.Unbind();
            collectionChangedEventStream.Unbind();

            foreach (var binding in this.bindings)
            {
                binding.Disconnect();
            }

            this.obj = null;
        }

        /// <summary>
        /// Find a binding nested in the tree of property bidnings.
        /// </summary>
        public IBinding FindNestedBinding(string bindingName)
        {
            if (string.IsNullOrEmpty(bindingName))
            {
                throw new ApplicationException("Unexpected code path.");
            }
            else
            {
                string propertyName;
                var dot = bindingName.IndexOf('.');

                if (dot == -1)
                {
                    propertyName = bindingName;
                    bindingName = string.Empty;
                }
                else
                {
                    propertyName = bindingName.Substring(0, dot);
                    bindingName = bindingName.Substring(dot + 1);
                }

                var propertyBinding = this.bindings.SingleOrDefault(o => o.PropertyName == propertyName);
                if (propertyBinding == null)
                {
                    throw new ApplicationException("Property '" + propertyName + "' has no binding.");
                }

                if (dot == -1)
                {
                    return propertyBinding;
                }

                return propertyBinding.FindNestedBinding(bindingName);
            }
        }

        private IObservable<BoundPropertyChangedEventArgs> GetPropertyChangedEventStream()
        {
            var notifyPropertyChanged = this.obj as INotifyPropertyChanged;
            if (notifyPropertyChanged == null)
            {
                return Observable.Empty<BoundPropertyChangedEventArgs>();
            }

            return notifyPropertyChanged
                .OnAnyPropertyChanges()
                .Select(e => new
                {
                    ChildBinding = this.bindings.SingleOrDefault(o => o.PropertyName == e.PropertyName),
                    Event = e
                })
                .Where(propertyEvent => propertyEvent.ChildBinding != null)
                .Select(propertyEvent => new BoundPropertyChangedEventArgs(propertyEvent.Event.PropertyName, propertyEvent.ChildBinding));
        }

        private IObservable<BoundPropertyChangingEventArgs> GetPropertyChangingEventStream()
        {
            var notifyPropertyChanging = this.obj as INotifyPropertyChanging;
            if (notifyPropertyChanging == null)
            {
                return Observable.Empty<BoundPropertyChangingEventArgs>();
            }

            return notifyPropertyChanging
                .OnAnyPropertyChanging()
                .Select(e => new
                {
                    ChildBinding = this.bindings.SingleOrDefault(b => b.PropertyName == e.PropertyName),
                    Event = e
                })
                .Where(propertyEvent => propertyEvent.ChildBinding != null)
                .Select(propertyEvent => new BoundPropertyChangingEventArgs(propertyEvent.Event.PropertyName, propertyEvent.ChildBinding));
        }
    }
}
