using System;
using System.Reactive.Linq;
using RSG.Utils;

namespace RSG
{
    /// <summary>
    /// Interface for the Object Property 
    /// </summary>
    public interface IObjectPropertyBinding : IBinding
    {
        /// <summary>
        /// The name of the property that is bound.
        /// </summary>
        string PropertyName { get; }
    }

    /// <summary>
    /// Binds to an object property.
    /// </summary>
    public class ObjectPropertyBinding : IObjectPropertyBinding
    {
        /// <summary>
        /// The object that bindings are anaged form.
        /// </summary>
        private object parentObject;

        /// <summary>
        /// /The type of the property that is bound.
        /// </summary>
        private Type propertyType;

        /// <summary>
        /// The name of the property that is bound..
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The binding for the property's values.
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
                    .Select(ev => 
                        new BoundPropertyChangingEventArgs(
                            GenerateAggregatePropertyName(ev.PropertyName), 
                            ev.Binding
                        )
                    );
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
                    .Select(ev =>
                        new BoundPropertyChangedEventArgs(
                            GenerateAggregatePropertyName(ev.PropertyName),
                            ev.Binding
                        )
                    );
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
                    .Select(ev =>
                        new BoundCollectionChangedEventArgs(
                            GenerateAggregatePropertyName(ev.PropertyName),
                            ev.SourceEventArgs,
                            ev.Binding,
                            ev.PreviousCollectionContent
                        )
                    );
            }
        }

        public ObjectPropertyBinding(string propertyName, Type propertyType, object parentObj, IValueBinding valueBinding)
        {
            Argument.StringNotNullOrEmpty(() => propertyName);
            Argument.NotNull(() => propertyType);
            Argument.NotNull(() => parentObj);
            Argument.NotNull(() => valueBinding);

            this.PropertyName = propertyName;
            this.propertyType = propertyType;
            this.ValueBinding = valueBinding;

            // Don't need to connect, the value binding will already be connected.
            this.parentObject = parentObj;
        }

        /// <summary>
        /// The type of the binding.
        /// </summary>
        public Type BindingType
        {
            get
            {
                return this.propertyType;
            }
        }

        /// <summary>
        /// Retreive the value.
        /// </summary>
        public object GetValue()
        {
            return ReflectionUtils.GetPropertyValue(this.parentObject, this.PropertyName);
        }

        /// <summary>
        /// Set the value.
        /// </summary>
        public void SetValue(object newValue)
        {
            ReflectionUtils.SetPropertyValue(this.parentObject, this.PropertyName, newValue);
        }

        /// <summary>
        /// Reattach the property to the property in the specified object.
        /// </summary>
        public void Connect(object parentObj)
        {
            Argument.NotNull(() => parentObj);

            Disconnect();

            this.parentObject = parentObj;

            // 
            // Rebind if non-null.
            //
            var obj = GetValue();
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
            if (this.parentObject == null)
            {
                return;
            }

            ValueBinding.Disconnect();

            parentObject = null;
        }

        /// <summary>
        /// Find a binding nested in the tree of property bidnings.
        /// </summary>
        public IBinding FindNestedBinding(string bindingName)
        {
            Argument.StringNotNullOrEmpty(() => bindingName);

            return ValueBinding.FindNestedBinding(bindingName);
        }

        /// <summary>
        /// Given a child property name, generate an aggregate property name.
        /// </summary>
        private string GenerateAggregatePropertyName(string childPropertyName)
        {
            var propertyName = PropertyName;
            if (!string.IsNullOrEmpty(childPropertyName))
            {
                if (!childPropertyName.StartsWith("["))
                {
                    propertyName += ".";
                }

                propertyName += childPropertyName;
            }

            return propertyName;
        }
    }
}
