using System;
using System.Collections.Generic;
using System.Linq;
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
}
