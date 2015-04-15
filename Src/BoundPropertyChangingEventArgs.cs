using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
    /// <summary>
    /// Event args for BoundPropertyChanging event.
    /// </summary>
    public class BoundPropertyChangingEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the property whose value is changing.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The binding.
        /// </summary>
        public IBinding Binding { get; private set; }

        public BoundPropertyChangingEventArgs(string propertyName, IBinding binding)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Invalid property name", "propertyName");
            }

            if (binding == null)
            {
                throw new ArgumentNullException("binding");
            }

            this.PropertyName = propertyName;
            this.Binding = binding;
        }

        public override string ToString()
        {
            return string.Format("BoundPropertyChangedEventArgs - PropertyName: {0}", PropertyName);
        }
    }
}
