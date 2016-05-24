using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RSG
{
    public class ObservableListTests
    {
        [Fact]
        public void event_raised_when_first_item_is_added()
        {
            var testObject = new ObservableList<int>();

            var events = new List<NotifyCollectionChangedEventArgs>();

            testObject.CollectionChanged += (source, args) => events.Add(args);

            var itemAdded = 1;
            testObject.Add(itemAdded);

            Assert.Equal(1, events.Count);

            var ev = events[0];
            Assert.Equal(NotifyCollectionChangedAction.Add, ev.Action);
            Assert.Equal(1, ev.NewItems.Count);
            Assert.Equal(itemAdded, ev.NewItems[0]);
            Assert.Equal(0, ev.NewStartingIndex);
        }

        [Fact]
        public void event_raised_when_second_item_is_added()
        {
            var testObject = new ObservableList<int>();

            var events = new List<NotifyCollectionChangedEventArgs>();

            testObject.Add(1);

            testObject.CollectionChanged += (source, args) => events.Add(args);

            var itemAdded = 2;
            testObject.Add(itemAdded);

            Assert.Equal(1, events.Count);

            var ev = events[0];
            Assert.Equal(NotifyCollectionChangedAction.Add, ev.Action);
            Assert.Equal(itemAdded, ev.NewItems[0]);
            Assert.Equal(1, ev.NewStartingIndex);
        }

        [Fact]
        public void event_raised_when_item_is_inserted()
        {
            var testObject = new ObservableList<int>();

            var events = new List<NotifyCollectionChangedEventArgs>();

            testObject.Add(1);
            testObject.Add(2);

            testObject.CollectionChanged += (source, args) => events.Add(args);

            var itemAdded = 3;
            testObject.Insert(1, itemAdded);

            Assert.Equal(1, events.Count);

            var ev = events[0];
            Assert.Equal(NotifyCollectionChangedAction.Add, ev.Action);
            Assert.Equal(itemAdded, ev.NewItems[0]);
            Assert.Equal(1, ev.NewStartingIndex);
        }

        [Fact]
        public void event_raised_when_item_is_removed_by_index()
        {
            var testObject = new ObservableList<int>();

            var events = new List<NotifyCollectionChangedEventArgs>();

            testObject.Add(1);
            testObject.Add(2);
            testObject.Add(3);

            var itemRemoved = 2;

            testObject.CollectionChanged += (source, args) => events.Add(args);

            testObject.RemoveAt(1);

            Assert.Equal(1, events.Count);

            var ev = events[0];
            Assert.Equal(NotifyCollectionChangedAction.Remove, ev.Action);
            Assert.Equal(1, ev.OldItems.Count);
            Assert.Equal(itemRemoved, ev.OldItems[0]);
            Assert.Equal(1, ev.OldStartingIndex);
        }

        [Fact]
        public void event_raised_when_item_is_removed_by_value()
        {
            var testObject = new ObservableList<int>();

            var events = new List<NotifyCollectionChangedEventArgs>();

            testObject.Add(1);
            testObject.Add(2);
            testObject.Add(3);

            testObject.CollectionChanged += (source, args) => events.Add(args);

            var itemRemoved = 2;
            testObject.Remove(itemRemoved);

            var ev = events[0];
            Assert.Equal(NotifyCollectionChangedAction.Remove, ev.Action);

            Assert.Equal(1, events.Count);
            Assert.Equal(1, ev.OldItems.Count);
            Assert.Equal(itemRemoved, ev.OldItems[0]);
            Assert.Equal(1, ev.OldStartingIndex);
        }

        [Fact]
        public void event_raised_when_list_is_cleared()
        {
            var testObject = new ObservableList<int>();

            var events = new List<NotifyCollectionChangedEventArgs>();

            testObject.Add(1);
            testObject.Add(2);

            testObject.CollectionChanged += (source, args) => events.Add(args);

            testObject.Clear();

            Assert.Equal(1, events.Count);

            var ev = events[0];
            Assert.Equal(NotifyCollectionChangedAction.Reset, ev.Action);
            Assert.Equal(2, ev.OldItems.Count);
            Assert.Equal(1, ev.OldItems[0]);
            Assert.Equal(2, ev.OldItems[1]);
            Assert.Equal(0, ev.OldStartingIndex);
        }
    }
}
