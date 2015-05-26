using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
    public enum NotifyCollectionChangedAction
    {
        Reset,
    }

    /// <summary>
    /// Arguments for collection changed event.
    /// </summary>
    public class NotifyCollectionChangedEventArgs : EventArgs
    {
        private NotifyCollectionChangedAction action;

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            this.action = action;
        }
    }
}
