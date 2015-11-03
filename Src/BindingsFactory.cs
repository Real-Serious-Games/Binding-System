using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RSG;
using RSG.Utils;

namespace RSG.Internal
{
    /// <summary>
    /// Builds a hierarchy of bindings.
    /// </summary>
    public interface IBindingsFactory
    {
        /// <summary>
        /// Build property bindings for the particular object.
        /// </summary>
        IObjectPropertyBinding[] CreateObjectBindings(object obj);

        /// <summary>
        /// Build list item bindings for the particular collection.
        /// </summary>
        IListItemBinding[] CreateListBindings(IList list);

        /// <summary>
        /// Create binding for an item in a list.
        /// </summary>
        IListItemBinding CreateListBinding(object item, int itemIndex, IList list);

        /// <summary>
        /// Build array item bindings for the particular array.
        /// </summary>
        IArrayItemBinding[] CreateArrayBindings(Array array);

        /// <summary>
        /// Create binding for an item in an array.
        /// </summary>
        IArrayItemBinding CreateArrayBinding(object item, int itemIndex, Array array);

        /// <summary>
        /// Create a value binding.
        /// </summary>
        IValueBinding CreateValueBinding(object parentObject, object value, Type type);
    }

    /// <summary>
    /// Builds a hierarchy of bindings.
    /// </summary>
    public class BindingsFactory : IBindingsFactory
    {
        /// <summary>
        /// Build property bindings for the particular object.
        /// </summary>
        public IObjectPropertyBinding[] CreateObjectBindings(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            var childPropertyBindings = new List<IObjectPropertyBinding>();
            var objType = obj.GetType();

            //
            // Determine properties that are bound to the UI.
            //
            foreach (var property in objType.GetProperties())
            {

                var bindingAttribute = ReflectionUtils.GetPropertyAttribute<BindingAttribute>(property);
                if (bindingAttribute == null)
                {
                    // There is no binding for this particular property.
                    continue;
                }

                var propertyValue = ReflectionUtils.GetPropertyValue(obj, property.Name);
                childPropertyBindings.Add(new ObjectPropertyBinding(property.Name, property.PropertyType, obj, CreateValueBinding(obj, propertyValue, property.PropertyType)));
            }

            return childPropertyBindings.ToArray();
        }

        /// <summary>
        /// Build list item bindings for the particular collection.
        /// </summary>
        public IListItemBinding[] CreateListBindings(IList list)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            var childPropertyBindings = new List<IListItemBinding>();
            var objType = list.GetType();

            int i = 0;

            foreach (var item in list)
            {
                childPropertyBindings.Add(CreateListBinding(item, i, list));

                ++i;
            }

            return childPropertyBindings.ToArray();
        }

        /// <summary>
        /// Create binding for an item in a list.
        /// </summary>
        public IListItemBinding CreateListBinding(object item, int itemIndex, IList list)
        {
            return new ListItemBinding(itemIndex, list, CreateValueBinding(list, item, item.GetType()));
        }

        /// <summary>
        /// Build array item bindings for the particular array.
        /// </summary>
        public IArrayItemBinding[] CreateArrayBindings(Array array)
        {
            Argument.NotNull(() => array);

            var childPropertyBindings = new List<IArrayItemBinding>();
            var objType = array.GetType();

            int i = 0;

            foreach (var item in array)
            {
                childPropertyBindings.Add(CreateArrayBinding(item, i, array));

                ++i;
            }

            return childPropertyBindings.ToArray();
        }

        /// <summary>
        /// Create binding for an item in an array.
        /// </summary>
        public IArrayItemBinding CreateArrayBinding(object item, int itemIndex, Array array)
        {
            Argument.NotNull(() => array);

            return new ArrayItemBinding(itemIndex, array, CreateValueBinding(array, item, array.GetType().GetElementType()));
        }

        /// <summary>
        /// Returns true to recurse into a type's properties to generate bindings.
        /// </summary>
        private bool RecurseTypeForBindings(Type type)
        {
            if (type.IsArray)
            {
                return true;
            }

            if (!type.IsClass && !type.IsInterface)
            {
                return false;
            }

            if (type == typeof(string) ||
                type == typeof(string[]))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create a value binding.
        /// </summary>
        public IValueBinding CreateValueBinding(object parentObject, object value, Type type)
        {
            IValueBinding valueBinding;
            //
            // Recurse if neccessary and setup bindings for subobjects.
            //
            if (RecurseTypeForBindings(type))
            {
                // Get the sub-object from the property.
                if (type.IsArray)
                {
                    valueBinding = new ArrayBinding((Array)value, this);
                }
                else if (value is IList)
                {
                    valueBinding = new ListBinding((IList)value, this);
                }
                else
                {
                    valueBinding = new ObjectBinding(value, this);
                }
            }
            else
            {
                valueBinding = new PrimitiveBinding(value);
            }
            return valueBinding;
        }
    }
}
