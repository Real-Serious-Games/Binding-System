using System;
using System.Collections;

namespace RSG
{
    public enum NotifyCollectionChangedAction
    {
        Add,
        Remove,
        Reset,
    }

    /// <summary>
    /// Arguments for collection changed event.
    /// </summary>
    public class NotifyCollectionChangedEventArgs : EventArgs
    {
        // Summary:
        //     Gets the action that caused the event.
        //
        // Returns:
        //     A System.Collections.Specialized.NotifyCollectionChangedAction value that
        //     describes the action that caused the event.
        public NotifyCollectionChangedAction Action { get; private set; }

        // Summary:
        //     Gets the list of new items involved in the change.
        //
        // Returns:
        //     The list of new items involved in the change.
        public IList NewItems { get; private set; }

        // Summary:
        //     Gets the index at which the change occurred.
        //
        // Returns:
        //     The zero-based index at which the change occurred.
        public int NewStartingIndex { get; private set; }

        // Summary:
        //     Gets the list of items affected by a System.Collections.Specialized.NotifyCollectionChangedAction.Replace,
        //     Remove, or Move action.
        //
        // Returns:
        //     The list of items affected by a System.Collections.Specialized.NotifyCollectionChangedAction.Replace,
        //     Remove, or Move action.
        public IList OldItems { get; private set; }

        // Summary:
        //     Gets the index at which a System.Collections.Specialized.NotifyCollectionChangedAction.Move,
        //     Remove, or Replace action occurred.
        //
        // Returns:
        //     The zero-based index at which a System.Collections.Specialized.NotifyCollectionChangedAction.Move,
        //     Remove, or Replace action occurred.
        public int OldStartingIndex { get; private set; }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            this.Action = action;
        }

    }
}
