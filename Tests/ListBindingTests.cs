using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Xunit;

namespace RSG.Tests
{
    public class ListBindingTests
    {
        private class TestClass
        {
            private int X { get; set; }
        }

        private List<Mock<IListItemBinding>> mockListItemBindings;
        private Mock<IBindingsFactory> mockBindingsFactory;

        private ListBinding testObject;
        private ObservableList<TestClass> testList;

        private void Init()
        {
            InitMocksWithNoBindings();
            InitTestObject();
        }

        private void InitTestObject()
        {
            testObject = new ListBinding(testList, mockBindingsFactory.Object);
        }

        private void InitMocksWithNoBindings()
        {
            InitMocksWithBindings(new List<TestClass>() { });
        }

        private void InitMocksWithOneBinding(TestClass testClass)
        {
            InitMocksWithBindings(new List<TestClass>() { testClass });
        }

        private void InitMocksWithBindings(List<TestClass> testClasses)
        {
            testList = new ObservableList<TestClass>();

            mockListItemBindings = new List<Mock<IListItemBinding>>();
            mockBindingsFactory = new Mock<IBindingsFactory>();

            for (var i = 0; i < testClasses.Count; i++)
            {
                var newMockBinding = new Mock<IListItemBinding>();

                newMockBinding
                    .Setup(m => m.ItemIndex)
                    .Returns(i);

                mockListItemBindings.Add(newMockBinding);
                testList.Add(testClasses[i]);
            }

            var mockBindingObjects = new List<IListItemBinding>();
            foreach (var mockBinding in mockListItemBindings)
            {
                mockBindingObjects.Add(mockBinding.Object);
            }

            mockBindingsFactory
                .Setup(m => m.CreateListBindings(testList))
                .Returns(mockBindingObjects.ToArray());
        }

        [Fact]
        public void remove_item_updates_itemindex_for_other_bindings()
        {
            InitMocksWithBindings(new List<TestClass>() 
            {
                new TestClass(),
                new TestClass(),
                new TestClass()
            });

            InitTestObject();

            mockBindingsFactory.Verify(m => m.CreateListBindings(testList), Times.Once());

            testList.RemoveAt(1);

            mockListItemBindings[0].VerifySet(m => m.ItemIndex = It.IsAny<int>(), Times.Never());
            mockListItemBindings[2].VerifySet(m => m.ItemIndex = 1, Times.Once());
        }

        [Fact]
        public void insert_item_updates_itemindex_for_other_bindings()
        {
            InitMocksWithBindings(new List<TestClass>()
            {
                new TestClass(),
                new TestClass()
            });

            var newItem = new TestClass();

            var mockNewListItemBinding = new Mock<IListItemBinding>();

            mockNewListItemBinding
                .Setup(m => m.ItemIndex)
                .Returns(1);

            mockBindingsFactory
                .Setup(m => m.CreateListBinding(newItem, 1, testList))
                .Returns(mockNewListItemBinding.Object);

            InitTestObject();

            mockBindingsFactory.Verify(m => m.CreateListBindings(testList), Times.Once());

            testList.Insert(1, newItem);

            mockListItemBindings[0].VerifySet(m => m.ItemIndex = It.IsAny<int>(), Times.Never());
            mockListItemBindings[1].VerifySet(m => m.ItemIndex = 2, Times.Once());
        }

        [Fact]
        public void find_item_binding()
        {
            InitMocksWithOneBinding(new TestClass());

            InitTestObject();

            Assert.Equal(mockListItemBindings[0].Object, testObject.FindNestedBinding("0"));
        }

        [Fact]
        public void find_nested_item_binding()
        {
            InitMocksWithOneBinding(new TestClass());

            var mockNestedBinding = new Mock<IBinding>();

            mockListItemBindings[0]
                .Setup(m => m.FindNestedBinding("Foo"))
                .Returns(mockNestedBinding.Object);

            InitTestObject();

            Assert.Equal(mockNestedBinding.Object, testObject.FindNestedBinding("0.Foo"));
        }

        // --------------------------------------
        // Property changing event stream.
        // --------------------------------------

        [Fact]
        public void BoundPropertyChanging_event_from_child_passed_on()
        {
            InitMocksWithOneBinding(new TestClass());

            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();

            mockListItemBindings[0]
                .Setup(m => m.ItemIndex)
                .Returns(0);

            mockListItemBindings[0]
                .Setup(m => m.PropertyChangingEventStream)
                .Returns(childEventStream);

            var events = new List<BoundPropertyChangingEventArgs>();

            InitTestObject();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            childEventStream.OnNext(new BoundPropertyChangingEventArgs("Nested", mockListItemBindings[0].Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("Nested", events[0].PropertyName);
            Assert.Equal(mockListItemBindings[0].Object, events[0].Binding);
        }

        [Fact]
        public void BoundPropertyChanging_event_from_removed_child_not_passed_on()
        {
            InitMocksWithOneBinding(new TestClass());

            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();

            var newItem = new TestClass();

            mockListItemBindings[0]
                .Setup(m => m.ItemIndex)
                .Returns(0);

            mockListItemBindings[0]
                .Setup(m => m.PropertyChangingEventStream)
                .Returns(childEventStream);

            var events = new List<BoundPropertyChangingEventArgs>();

            InitTestObject();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            testList.RemoveAt(0);

            childEventStream.OnNext(new BoundPropertyChangingEventArgs("Nested", mockListItemBindings[0].Object));

            Assert.Empty(events);
        }

        [Fact]
        public void BoundPropertyChanging_event_from_added_child_passed_on()
        {
            InitMocksWithNoBindings();

            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();
            var mockListItemBinding = new Mock<IListItemBinding>();

            mockListItemBinding
                .Setup(m => m.PropertyChangingEventStream)
                .Returns(childEventStream);

            mockBindingsFactory
                .Setup(m => m.CreateListBindings(testList))
                .Returns(new IListItemBinding[0]);

            var newItem = new TestClass();

            mockBindingsFactory
                .Setup(m => m.CreateListBinding(newItem, 0, testList))
                .Returns(mockListItemBinding.Object);

            var events = new List<BoundPropertyChangingEventArgs>();

            InitTestObject();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            testList.Add(newItem);

            childEventStream.OnNext(new BoundPropertyChangingEventArgs("Nested", mockListItemBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("Nested", events[0].PropertyName);
            Assert.Equal(mockListItemBinding.Object, events[0].Binding);
        }

        [Fact]
        public void BoundPropertyChanging_event_from_child_not_passed_on_after_cleared_list()
        {
            InitMocksWithOneBinding(new TestClass());

            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();

            mockListItemBindings[0]
                .Setup(m => m.PropertyChangingEventStream)
                .Returns(childEventStream);

            var events = new List<BoundPropertyChangingEventArgs>();

            InitTestObject();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            testList.Clear();

            childEventStream.OnNext(new BoundPropertyChangingEventArgs("Nested", mockListItemBindings[0].Object));

            Assert.Empty(events);
        }

        // --------------------------------------
        // Property changed event stream.
        // --------------------------------------

        [Fact]
        public void BoundPropertyChanged_event_from_child_passed_on()
        {
            InitMocksWithOneBinding(new TestClass());

            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            mockListItemBindings[0]
                .Setup(m => m.PropertyChangedEventStream)
                .Returns(childEventStream);

            var events = new List<BoundPropertyChangedEventArgs>();

            InitTestObject();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            childEventStream.OnNext(new BoundPropertyChangedEventArgs("Nested", mockListItemBindings[0].Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("Nested", events[0].PropertyName);
            Assert.Equal(mockListItemBindings[0].Object, events[0].Binding);
        }

        [Fact]
        public void BoundPropertyChanged_event_from_removed_child_not_passed_on()
        {
            InitMocksWithOneBinding(new TestClass());

            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            mockListItemBindings[0]
                .Setup(m => m.PropertyChangedEventStream)
                .Returns(childEventStream);

            InitTestObject();

            var events = new List<BoundPropertyChangedEventArgs>();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            testList.RemoveAt(0);

            childEventStream.OnNext(new BoundPropertyChangedEventArgs("Nested", mockListItemBindings[0].Object));

            Assert.Equal(0, events.Count);
        }

        [Fact]
        public void BoundPropertyChanged_event_from_added_child_passed_on()
        {
            InitMocksWithNoBindings();

            var mockNewItemBinding = new Mock<IListItemBinding>();
            var newItem = new TestClass();

            mockBindingsFactory
                .Setup(m => m.CreateListBinding(newItem, 0, testList))
                .Returns(mockNewItemBinding.Object);

            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            mockNewItemBinding
                .Setup(m => m.PropertyChangedEventStream)
                .Returns(childEventStream);

            var events = new List<BoundPropertyChangedEventArgs>();

            InitTestObject();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            testList.Add(newItem);

            childEventStream.OnNext(new BoundPropertyChangedEventArgs("Nested", mockNewItemBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("Nested", events[0].PropertyName);
            Assert.Equal(mockNewItemBinding.Object, events[0].Binding);
        }

        [Fact]
        public void BoundPropertyChanged_event_from_child_not_passed_on_after_cleared_list()
        {
            InitMocksWithOneBinding(new TestClass());

            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            mockListItemBindings[0]
                .Setup(m => m.PropertyChangedEventStream)
                .Returns(childEventStream);

            var events = new List<BoundPropertyChangedEventArgs>();

            InitTestObject();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            testList.Clear();

            childEventStream.OnNext(new BoundPropertyChangedEventArgs("Nested", mockListItemBindings[0].Object));

            Assert.Equal(0, events.Count);
        }

        // --------------------------------------
        // Collection changed event stream.
        // --------------------------------------

        [Fact]
        public void BoundCollectionChanged_event_from_child_passed_on()
        {
            InitMocksWithOneBinding(new TestClass());

            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();

            mockListItemBindings[0]
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(childEventStream);

            InitTestObject();

            var events = new List<BoundCollectionChangedEventArgs>();

            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            childEventStream.OnNext(new BoundCollectionChangedEventArgs("Nested", fakeCollectionChangedEventArgs, null, null));

            Assert.Equal(1, events.Count);
            Assert.Equal("Nested", events[0].PropertyName);
            Assert.Equal(fakeCollectionChangedEventArgs, events[0].SourceEventArgs);
        }

        [Fact]
        public void BoundCollectionChanged_event_from_removed_child_not_passed_on()
        {
            InitMocksWithOneBinding(new TestClass());

            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();

            mockListItemBindings[0]
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(childEventStream);

            InitTestObject();

            testList.RemoveAt(0);

            var events = new List<BoundCollectionChangedEventArgs>();
            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            childEventStream.OnNext(new BoundCollectionChangedEventArgs("Nested", fakeCollectionChangedEventArgs, null, null));

            Assert.Empty(events);
        }

        [Fact]
        public void BoundCollectionChanged_event_from_added_child_passed_on()
        {
            InitMocksWithNoBindings();

            var newMockListItemBinding = new Mock<IListItemBinding>();
            newMockListItemBinding
                .Setup(m => m.ItemIndex)
                .Returns(0);

            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();
            newMockListItemBinding
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(childEventStream);

            var newItem = new TestClass();
            mockBindingsFactory
                .Setup(m => m.CreateListBinding(newItem, 0, testList))
                .Returns(newMockListItemBinding.Object);

            InitTestObject();

            testList.Add(newItem);

            var events = new List<BoundCollectionChangedEventArgs>();
            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            childEventStream.OnNext(new BoundCollectionChangedEventArgs("Nested", fakeCollectionChangedEventArgs, null, null));

            Assert.Equal(1, events.Count);
            Assert.Equal("Nested", events[0].PropertyName);
            Assert.Equal(fakeCollectionChangedEventArgs, events[0].SourceEventArgs);
        }

        [Fact]
        public void BoundCollectionChanged_event_from_child_not_passed_on_after_cleared_list()
        {
            InitMocksWithOneBinding(new TestClass());

            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();

            mockListItemBindings[0]
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(childEventStream);

            InitTestObject();

            testList.Clear();

            var events = new List<BoundCollectionChangedEventArgs>();
            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            childEventStream.OnNext(new BoundCollectionChangedEventArgs("Nested", fakeCollectionChangedEventArgs, null, null));

            Assert.Empty(events);
        }

        [Fact]
        public void BoundCollectionChanged_add_event_passed_on()
        {
            InitMocksWithOneBinding(new TestClass());

            mockListItemBindings[0]
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(Observable.Empty<BoundCollectionChangedEventArgs>());

            var newItem = new TestClass();
            var mockNewListItemBinding = new Mock<IListItemBinding>();

            mockBindingsFactory
                .Setup(m => m.CreateListBinding(newItem, 1, testList))
                .Returns(mockNewListItemBinding.Object);

            mockNewListItemBinding
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(Observable.Empty<BoundCollectionChangedEventArgs>());

            mockNewListItemBinding
                .Setup(m => m.ItemIndex)
                .Returns(1);

            InitTestObject();

            mockBindingsFactory.Verify(m => m.CreateListBindings(testList), Times.Once());

            var events = new List<BoundCollectionChangedEventArgs>();
            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            testList.Add(newItem);

            mockBindingsFactory.Verify(m => m.CreateListBinding(newItem, 1, testList), Times.Once());

            Assert.Equal(1, events.Count);
            Assert.Equal(null, events[0].PropertyName);
            Assert.Equal(NotifyCollectionChangedAction.Add, events[0].SourceEventArgs.Action);
            Assert.Equal(1, events[0].SourceEventArgs.NewStartingIndex);
            Assert.Equal(1, events[0].SourceEventArgs.NewItems.Count);
            Assert.Equal(newItem, events[0].SourceEventArgs.NewItems[0]);
        }

        [Fact]
        public void BoundCollectionChanged_reset_event_passed_on()
        {
            var testClass = new TestClass();
            InitMocksWithOneBinding(testClass);

            mockListItemBindings[0]
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(new Subject<BoundCollectionChangedEventArgs>());

            InitTestObject();

            var events = new List<BoundCollectionChangedEventArgs>();
            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            testList.Clear();

            Assert.Equal(1, events.Count);
            Assert.Equal(NotifyCollectionChangedAction.Reset, events[0].SourceEventArgs.Action);
            Assert.Equal(1, events[0].PreviousCollectionContent.Length);
            Assert.Equal(testClass, events[0].PreviousCollectionContent[0]);
        }

        [Fact]
        public void BoundCollectionChanged_event_not_passed_on_after_disconnect()
        {
            InitMocksWithOneBinding(new TestClass());

            var newItem = new TestClass();

            mockListItemBindings[0]
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(Observable.Empty<BoundCollectionChangedEventArgs>());

            InitTestObject();

            mockBindingsFactory.Verify(m => m.CreateListBindings(testList), Times.Once());

            var events = new List<BoundCollectionChangedEventArgs>();
            testObject.CollectionChangedEventStream.Subscribe(e => events.Add(e)); 

            testObject.Disconnect();

            mockListItemBindings[0].Verify(m => m.Disconnect());

            testList.Add(newItem);

            Assert.Empty(events);
        }

        [Fact]
        public void BoundCollectionChanged_event_not_passed_on_after_reconnect()
        {
            InitMocksWithOneBinding(new TestClass());

            var mockListItemBinding = new Mock<IListItemBinding>();
            var mockNewListItemBinding = new Mock<IListItemBinding>();

            var newList = new ObservableList<TestClass>()
            {
                new TestClass()
            };

            mockBindingsFactory
                .Setup(m => m.CreateListBindings(newList))
                .Returns(new IListItemBinding[] { mockListItemBinding.Object });

            var newItem = new TestClass();

            mockListItemBindings[0]
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(Observable.Empty<BoundCollectionChangedEventArgs>());

            mockListItemBinding
                .Setup(m => m.ItemIndex)
                .Returns(0);

            mockListItemBinding
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(Observable.Empty<BoundCollectionChangedEventArgs>());

            mockNewListItemBinding
                .Setup(m => m.ItemIndex)
                .Returns(1);

            mockNewListItemBinding
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(Observable.Empty<BoundCollectionChangedEventArgs>());

            mockBindingsFactory
                .Setup(m => m.CreateListBinding(newItem, 1, newList))
                .Returns(mockNewListItemBinding.Object);

            InitTestObject();

            var events = new List<BoundCollectionChangedEventArgs>();
            testObject.CollectionChangedEventStream.Subscribe(e => events.Add(e));

            testObject.Connect(newList);

            mockListItemBindings[0].Verify(m => m.Disconnect(), Times.Once());

            mockBindingsFactory.Verify(m => m.CreateListBindings(newList), Times.Once());

            newList.Add(newItem);

            mockBindingsFactory.Verify(m => m.CreateListBinding(newItem, 1, newList), Times.Once());

            Assert.Equal(1, events.Count);
            Assert.Equal(null, events[0].PropertyName);
            Assert.Equal(NotifyCollectionChangedAction.Add, events[0].SourceEventArgs.Action);
            Assert.Equal(1, events[0].SourceEventArgs.NewStartingIndex);
            Assert.Equal(1, events[0].SourceEventArgs.NewItems.Count);
            Assert.Equal(newItem, events[0].SourceEventArgs.NewItems[0]);
            Assert.Equal(mockNewListItemBinding.Object, events[0].Binding);
        }
    }
}
