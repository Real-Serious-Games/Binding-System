using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RSG
{
    /// <summary>
    /// A list that exposes the type of its items as a property.
    /// </summary>
    public interface ITypedList : IEnumerable //fio: Get rid of this.
    {
        /// <summary>
        /// Specifies the type of items in the list.
        /// </summary>
        Type ItemType { get; }
    }
}
