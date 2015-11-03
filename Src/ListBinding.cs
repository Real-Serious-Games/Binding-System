using RSG.Internal;
using RSG.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace RSG
{
    /// <summary>
    /// Binds a list value and all of its children.
    /// </summary>
    public class ListBinding : IValueBinding
    {
        /// <summary>
        /// The object that bindings are managed for.
        /// </summary>
        private IList list;

        /// <summary>
        /// Factory used to create child bindings.
        /// </summary>
        private IBindingsFactory bindingsFactory;

        /// <summary>
        /// Bindings for items in the list.
        /// </summary>
        private List<IListItemBinding> bindings;

        /// <summary>
        /// Subscription to the list collection changed events.
        /// </summary>
        private IDisposable listSubscription;

        /// <summary>
        /// Previous content of the collection.
        /// Must be maintained so that we know what was removed when the collection is reset.
        /// </summary>
        private object[] previousCollectionContent;

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
        /// Stream of collection changed events.
        /// </summary>
        private Subject<BoundCollectionChangedEventArgs> listCollectionChangedEvents = new Subject<BoundCollectionChangedEventArgs>();

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

        public ListBinding(IList list, IBindingsFactory bindingsFactory)
        {
            Argument.NotNull(() => list);
            Argument.NotNull(() => bindingsFactory);

            this.bindingsFactory = bindingsFactory;

            Connect(list);
        }

        /// <summary>
        /// Reattach the property to the property in the specified object.
        /// </summary>
        public void Connect(object list)
        {
            Argument.NotNull(() => list);

            if (this.list != null)
            {
                Disconnect();
            }

            this.list = (IList)list;
            this.previousCollectionContent = this.list.Cast<object>().ToArray();
            this.bindings = bindingsFactory.CreateListBindings(this.list).ToList();

            var notifyCollectionChanged = this.list as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
            {
                listSubscription = notifyCollectionChanged
                    .OnAnyCollectionChanges()
                    .Subscribe(e => 
                    {
                        switch(e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                {
                                    bindings
                                        .Where(b => b.ItemIndex >= e.NewStartingIndex + e.NewItems.Count - 1)
                                        .Each(b => b.ItemIndex = b.ItemIndex + e.NewItems.Count);

                                    e.NewItems.Cast<object>()
                                        .Select((i, index) => 
                                        {
                                            return bindingsFactory.CreateListBinding(i, e.NewStartingIndex + index, this.list);
                                        })
                                        .Each(b => bindings.Add(b));
                                }
                                break;
                            case NotifyCollectionChangedAction.Remove:
                                {
                                    var bindingsToRemove = bindings
                                        .Where(b =>
                                            b.ItemIndex >= e.OldStartingIndex &&
                                            b.ItemIndex < e.OldStartingIndex + e.OldItems.Count);

                                    bindingsToRemove.Each(b => b.Disconnect());
                                    bindings.RemoveAll(b => bindingsToRemove.Any(btr => btr == b));

                                    bindings
                                        .Where(b => b.ItemIndex >= e.OldStartingIndex + e.OldItems.Count)
                                        .Each(b => b.ItemIndex = b.ItemIndex - e.OldItems.Count);
                                }
                                break;
                            case NotifyCollectionChangedAction.Reset:
                                {
                                    bindings.Each(b => b.Disconnect());

                                    bindings.Clear();
                                }
                                break;
                            default:
                                {
                                    throw new ApplicationException("Unsupported operation for a bound list: " + 
                                        Enum.GetName(typeof(NotifyCollectionChangedAction), e.Action));
                                }
                        }

                        // This does not account for multiple items added or removed.
                        var binding = bindings.Where(b => b.ItemIndex == e.NewStartingIndex).FirstOrDefault();

                        if (e.Action != NotifyCollectionChangedAction.Add || binding != null)
                        {
                            listCollectionChangedEvents.OnNext(new BoundCollectionChangedEventArgs(null, e, binding, previousCollectionContent));
                        }

                        // Update previous collection content each time the list changes.
                        previousCollectionContent = this.list.Cast<object>().ToArray();

                        BindEventStreams();
                    });
            }

            BindEventStreams();
        }

        /// <summary>
        /// Bind event streams to the current list bindings.
        /// </summary>
        public void BindEventStreams()
        {
            propertyChangingEventStream.Bind(Observable
                .Merge(bindings.Select(b => b.PropertyChangingEventStream)));

            propertyChangedEventStream.Bind(Observable
                .Merge(bindings.Select(b => b.PropertyChangedEventStream)));

            collectionChangedEventStream.Bind(Observable
                .Merge(bindings.Select(b => b.CollectionChangedEventStream))
                .Merge(listCollectionChangedEvents));
        }

        /// <summary>
        /// Disconnected events from the bound property.
        /// </summary>
        public void Disconnect()
        {
            bindings.Each(b => b.Disconnect());
            bindings.Clear();

            collectionChangedEventStream.Unbind();
            propertyChangedEventStream.Unbind();
            propertyChangingEventStream.Unbind();

            listSubscription.Dispose();

            list = null;
            previousCollectionContent = null;
        }

        /// <summary>
        /// Find a binding nested in the tree of property bidnings.
        /// </summary>
        public IBinding FindNestedBinding(string bindingName)
        {
            if (string.IsNullOrEmpty(bindingName))
            {
                throw new ArgumentException("Bad bindingName input.", "bindingName");
            }

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
