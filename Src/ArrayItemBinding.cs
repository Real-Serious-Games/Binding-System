using RSG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace RSG
{
    /// <summary>
    /// A binding to an element in an array.
    /// </summary>
    public interface IArrayItemBinding : IBinding
    {
        /// <summary>
        /// The index of the bound array element.
        /// </summary>
        int ItemIndex { get; set; }
    }

    /// <summary>
    /// A binding to an element in an array.
    /// </summary>
    public class ArrayItemBinding : IArrayItemBinding
    {
        /// <summary>
        /// The index of the bound array element.
        /// </summary>
        public int ItemIndex { get; set; }

        /// <summary>
        /// The array that contains element that is bound.
        /// </summary>
        private Array parentArray;

        /// <summary>
        /// The binding to the array element's value.
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

        public ArrayItemBinding(int itemIndex, Array parentArray, IValueBinding valueBinding)
        {
            Argument.Invariant(() => itemIndex, () => itemIndex >= 0);
            Argument.NotNull(() => parentArray);
            Argument.NotNull(() => valueBinding);


            if (itemIndex < 0)
            {
                throw new ArgumentException("Item index can't be less than 0.", "itemIndex");
            }

            this.ItemIndex = itemIndex;
            this.ValueBinding = valueBinding;

            // Don't need to connect, the value binding will already be connected.
            this.parentArray = parentArray;
        }

        /// <summary>
        /// Retreive the value.
        /// </summary>
        public object GetValue()
        {
            return this.parentArray.GetValue(this.ItemIndex);
        }

        /// <summary>
        /// Set the value.
        /// </summary>
        public void SetValue(object newValue)
        {
            this.parentArray.SetValue(newValue, this.ItemIndex);
        }

        /// <summary>
        /// The type of the binding.
        /// </summary>
        public Type BindingType
        {
            get
            {
                return this.parentArray.GetType().GetElementType();
            }
        }

        /// <summary>
        /// Reattach the property to the property in the specified object.
        /// </summary>
        public void Connect(object parentObj)
        {
            if (parentObj == null)
            {
                throw new ArgumentNullException("parentObj");
            }

            Disconnect();

            this.parentArray = (Array)parentObj;

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
            if (this.parentArray == null)
            {
                return;
            }

            this.ValueBinding.Disconnect();

            this.parentArray = null;
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
