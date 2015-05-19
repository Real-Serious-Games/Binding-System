using RSG.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace RSG
{
    /// <summary>
    /// Event args for the BoundCollectionChanged event.
    /// </summary>
    public class BoundCollectionChangedEventArgs : EventArgs //todo: should break this up into item added and item removed events.
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The binding to the item added or removed.
        /// </summary>
        public IBinding Binding { get; private set; }

        /// <summary>
        /// Items that were in the collection prior to the operation, only for the reset action.
        /// </summary>
        // Todo: Check that this property is used.
        public object[] PreviousCollectionContent { get; private set; }

        /// <summary>
        /// Reference to the arguments of the source event.
        /// </summary>
        public NotifyCollectionChangedEventArgs SourceEventArgs { get; private set; }

        public BoundCollectionChangedEventArgs(string propertyName, NotifyCollectionChangedEventArgs sourceEventArgs, IBinding binding, object[] PreviousCollectionContent)
        {
            Argument.NotNull(() => sourceEventArgs);

            this.PropertyName = propertyName;
            this.SourceEventArgs = sourceEventArgs;
            this.Binding = binding;
            this.PreviousCollectionContent = PreviousCollectionContent;
        }
    }
}
