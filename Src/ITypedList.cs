using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
    public interface ITypedList : IList
    {
        /// <summary>
        /// Specifies the type of items in the list.
        /// </summary>
        Type ItemType { get; }
    }
}
