using RSG.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace RSG
{
    /// <summary>
    /// A binding to an item in a list.
    /// </summary>
    public interface IListItemBinding : IBinding
    {
        /// <summary>
        /// The index of the bound list item.
        /// </summary>
        int ItemIndex { get; set; }
    }

    /// <summary>
    /// A binding to an item in a list.
    /// </summary>
    public class ListItemBinding : IListItemBinding
    {
        /// <summary>
        /// The index of the bound list item.
        /// </summary>
        public int ItemIndex { get; set; }

        /// <summary>
        /// The list that bindings are managed for.
        /// </summary>
        private IList parentList;

        /// <summary>
        /// The value binding that is named.
        /// </summary>
        public IValueBinding ValueBinding { get; private set; }

        /// <summary>
        /// Stream of property changing events.
        /// </summary>
        public IObservable<BoundPropertyChangingEventArgs> PropertyChangingEventStream
        {
            get
            {
                return ValueBinding.PropertyChangingEventStream
                    .Select(ev => new BoundPropertyChangingEventArgs(GenerateAggregatePropertyName(ev.PropertyName), ev.Binding));
            }
        }

        /// <summary>
        /// Stream of property changed events.
        /// </summary>
        public IObservable<BoundPropertyChangedEventArgs> PropertyChangedEventStream
        {
            get
            {
                return ValueBinding.PropertyChangedEventStream
                    .Select(ev => new BoundPropertyChangedEventArgs(GenerateAggregatePropertyName(ev.PropertyName), ev.Binding));
            }
        }

        /// <summary>
        /// Stream of collection changed events.
        /// </summary>
        public IObservable<BoundCollectionChangedEventArgs> CollectionChangedEventStream
        {
            get
            {
                return ValueBinding.CollectionChangedEventStream
                    .Select(ev => new BoundCollectionChangedEventArgs(GenerateAggregatePropertyName(ev.PropertyName), ev.SourceEventArgs, ev.Binding, ev.PreviousCollectionContent));
            }
        }

        public ListItemBinding(int itemIndex, IList parentObj, IValueBinding valueBinding)
        {
            Argument.Invariant(() => itemIndex, () => itemIndex >= 0);
            Argument.NotNull(() => parentObj);
            Argument.NotNull(() => valueBinding);

            this.ItemIndex = itemIndex;
            this.ValueBinding = valueBinding;

            this.parentList = parentObj;
        }

        /// <summary>
        /// Retreive the value.
        /// </summary>
        public object GetValue()
        {
            return this.parentList[this.ItemIndex];
        }

        /// <summary>
        /// Set the value.
        /// </summary>
        public void SetValue(object newValue)
        {
            this.parentList[this.ItemIndex] = newValue;
        }

        /// <summary>
        /// The type of the binding.
        /// </summary>
        public Type BindingType
        {
            get
            {
                return this.parentList[this.ItemIndex].GetType();
            }
        }

        /// <summary>
        /// Reattach the property to the property in the specified object.
        /// </summary>
        public void Connect(object parentObj)
        {
            Argument.NotNull(() => parentObj);

            Disconnect();

            this.parentList = (IList)parentObj;

            // 
            // Rebind if non-null.
            //
            var obj = this.GetValue();
            if (obj != null)
            {
                this.ValueBinding.Connect(obj);
            }
        }

        /// <summary>
        /// Disconnected events from the bound property.
        /// </summary>
        public void Disconnect()
        {
            if (this.parentList == null)
            {
                return;
            }

            this.ValueBinding.Disconnect();

            this.parentList = null;
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

            return this.ValueBinding.FindNestedBinding(bindingName);
        }

        /// <summary>
        /// Helper to concatenate property names.
        /// </summary>
        private string GenerateAggregatePropertyName(string propertyName)
        {
            var itemIndexStr = this.ItemIndex.ToString();

            return
                !string.IsNullOrEmpty(propertyName) ?
                    itemIndexStr + "." + propertyName :
                    itemIndexStr;
        }
    }
}
