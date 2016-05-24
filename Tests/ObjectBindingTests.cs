using Moq;
using RSG;
using RSG.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using Xunit;

namespace RSG.Tests
{
    public class ObjectBindingTests
    {
        /// <summary>
        /// Stub class used for testing.
        /// </summary>
        public class TestClass : INotifyPropertyChanged, INotifyPropertyChanging
        {
            private int x;

            public int X
            {
                get { return x; }
                set
                {
                    if (this.PropertyChanging != null)
                    {
                        this.PropertyChanging(this, new PropertyChangingEventArgs("X"));
                    }

                    x = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("X"));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public event PropertyChangingEventHandler PropertyChanging;
        }

        // Mock objects.
        private Mock<IBindingsFactory> mockBindingsFactory;
        private Mock<IObjectPropertyBinding> mockObjectPropertyBinding;

        // Test object.
        private ObjectBinding testObject;

        /// <summary>
        /// Initializes the mock objects.
        /// </summary>
        private void InitMocks()
        {
            mockBindingsFactory = new Mock<IBindingsFactory>();
            mockObjectPropertyBinding = new Mock<IObjectPropertyBinding>();
        }

        /// <summary>
        /// Initializes the test object with the property changing event stream.
        /// </summary>
        private void InitTestObjectWithPropertyChangingStream(Subject<BoundPropertyChangingEventArgs> childEventStream, TestClass obj)
        {
            mockObjectPropertyBinding
                .Setup(m => m.PropertyChangingEventStream)
                .Returns(childEventStream);

            mockBindingsFactory
                .Setup(m => m.CreateObjectBindings(obj))
                .Returns(new IObjectPropertyBinding[] { mockObjectPropertyBinding.Object });

            InitTestObject(obj);
        }

        /// <summary>
        /// Initializes the test object with the property changed event stream.
        /// </summary>
        private void InitTestObjectWithPropertyChangedStream(Subject<BoundPropertyChangedEventArgs> childEventStream, TestClass obj)
        {
            mockObjectPropertyBinding
                .Setup(m => m.PropertyChangedEventStream)
                .Returns(childEventStream);

            mockBindingsFactory
                .Setup(m => m.CreateObjectBindings(obj))
                .Returns(new IObjectPropertyBinding[] { mockObjectPropertyBinding.Object });
            
            InitTestObject(obj);
        }

        /// <summary>
        /// Initializes the test object with the collection changed event stream.
        /// </summary>
        private void InitTestObjectWithCollectionChangedStream(Subject<BoundCollectionChangedEventArgs> childEventStream, TestClass obj)
        {
            mockObjectPropertyBinding
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(childEventStream);

            mockBindingsFactory
                .Setup(m => m.CreateObjectBindings(obj))
                .Returns(new IObjectPropertyBinding[] { mockObjectPropertyBinding.Object });

            InitTestObject(obj);
        }

        /// <summary>
        /// Initializes the test object.
        /// </summary>
        private void InitTestObject(TestClass obj)
        {
            testObject = new ObjectBinding(obj, mockBindingsFactory.Object);
        }

        [Fact]
        public void test_find_property_binding()
        {
            var obj = new TestClass()
            {
                X = 15
            };

            var mockObjectPropertyBinding = new Mock<IObjectPropertyBinding>();
            var mockBindingsFactory = new Mock<IBindingsFactory>();

            mockObjectPropertyBinding
                .Setup(m => m.PropertyName)
                .Returns("X");

            mockObjectPropertyBinding
                .Setup(m => m.FindNestedBinding(string.Empty))
                .Returns(mockObjectPropertyBinding.Object);

            mockBindingsFactory
                .Setup(m => m.CreateObjectBindings(obj))
                .Returns(new IObjectPropertyBinding[] { mockObjectPropertyBinding.Object });

            var testObject = new ObjectBinding(obj, mockBindingsFactory.Object);

            Assert.Equal(mockObjectPropertyBinding.Object, testObject.FindNestedBinding("X"));
        }

        [Fact]
        public void test_find_nested_property_binding()
        {
            var obj = new
            {
                X = new ObservableList<int>()
            };

            var mockObjectPropertyBinding = new Mock<IObjectPropertyBinding>();
            var mockBindingsFactory = new Mock<IBindingsFactory>();

            mockObjectPropertyBinding
                .Setup(m => m.PropertyName)
                .Returns("X");

            mockObjectPropertyBinding
                .Setup(m => m.FindNestedBinding("0"))
                .Returns(mockObjectPropertyBinding.Object);

            mockBindingsFactory
                .Setup(m => m.CreateObjectBindings(obj))
                .Returns(new IObjectPropertyBinding[] { mockObjectPropertyBinding.Object });

            var testObject = new ObjectBinding(obj, mockBindingsFactory.Object);

            Assert.Equal(mockObjectPropertyBinding.Object, testObject.FindNestedBinding("X.0"));
        }

        public class TestClassWithMethods
        {
            [Binding]
            public void TestMethod1() { }

            public void TestMethod2(int x) { }

            [Binding]
            public void TestMethod3(int x, int y) { }
        }

        [Fact]
        public void connecting_calls_connect_on_all_child_bindings()
        {
            var obj = new TestClass();

            var mockObjectPropertyBinding1 = new Mock<IObjectPropertyBinding>();
            var mockObjectPropertyBinding2 = new Mock<IObjectPropertyBinding>();

            InitMocks();

            mockBindingsFactory
                .Setup(m => m.CreateObjectBindings(obj))
                .Returns(new IObjectPropertyBinding[] { mockObjectPropertyBinding1.Object, mockObjectPropertyBinding2.Object });

            InitTestObject(obj);
            
            mockObjectPropertyBinding1.Verify(m => m.Connect(obj), Times.Once());
            mockObjectPropertyBinding2.Verify(m => m.Connect(obj), Times.Once());
        }

        [Fact]
        public void disconnecting_calls_disconnect_on_all_child_bindings()
        {
            var obj = new TestClass();

            var mockObjectPropertyBinding1 = new Mock<IObjectPropertyBinding>();
            var mockObjectPropertyBinding2 = new Mock<IObjectPropertyBinding>();

            InitMocks();

            mockBindingsFactory
                .Setup(m => m.CreateObjectBindings(obj))
                .Returns(new IObjectPropertyBinding[] { mockObjectPropertyBinding1.Object, mockObjectPropertyBinding2.Object });

            InitTestObject(obj);

            testObject.Disconnect();

            mockObjectPropertyBinding1.Verify(m => m.Disconnect(), Times.Once());
            mockObjectPropertyBinding2.Verify(m => m.Disconnect(), Times.Once());
        }

        // --------------------------------------
        // Property changed event stream.
        // --------------------------------------

        [Fact]
        public void property_changed_is_propagated()
        {
            var propertyName = "X";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            InitMocks();

            mockObjectPropertyBinding
                .Setup(m => m.PropertyName)
                .Returns(propertyName);

            InitTestObjectWithPropertyChangedStream(childEventStream, obj);

            var events = new List<BoundPropertyChangedEventArgs>();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            obj.X = 5;

            Assert.Equal(1, events.Count);
            Assert.Equal(propertyName, events[0].PropertyName);
            Assert.Equal(mockObjectPropertyBinding.Object, events[0].Binding);
        }

        [Fact]
        public void property_changed_is_not_propagated_after_disconnection()
        {
            var propertyName = "X";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            InitMocks();
            InitTestObjectWithPropertyChangedStream(childEventStream, obj);

            mockObjectPropertyBinding
                .Setup(m => m.PropertyName)
                .Returns(propertyName);

            var events = new List<BoundPropertyChangedEventArgs>();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            testObject.Disconnect();
            obj.X = 5;

            Assert.Empty(events);
        }

        [Fact]
        public void property_changed_is_propagated_after_reconnection()
        {
            var propertyName = "X";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            InitMocks();
            InitTestObjectWithPropertyChangedStream(childEventStream, obj);

            mockObjectPropertyBinding
                .Setup(m => m.PropertyName)
                .Returns(propertyName);

            var events = new List<BoundPropertyChangedEventArgs>();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            testObject.Disconnect();
            obj.X = 5;

            Assert.Empty(events);

            testObject.Connect(obj);
            obj.X = 5;

            Assert.Equal(1, events.Count);
            Assert.Equal(propertyName, events[0].PropertyName);
            Assert.Equal(mockObjectPropertyBinding.Object, events[0].Binding);
        }

        [Fact]
        public void property_changed_is_propagated_from_child_node()
        {
            var propertyName = "X";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            InitMocks();
            InitTestObjectWithPropertyChangedStream(childEventStream, obj);

            var events = new List<BoundPropertyChangedEventArgs>();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            childEventStream.OnNext(new BoundPropertyChangedEventArgs(propertyName, mockObjectPropertyBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal(propertyName, events[0].PropertyName);
            Assert.Equal(mockObjectPropertyBinding.Object, events[0].Binding);
        }

        [Fact]
        public void property_changed_is_not_propagated_from_child_node_after_disconnect()
        {
            var propertyName = "X";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            InitMocks();
            InitTestObjectWithPropertyChangedStream(childEventStream, obj);

            var events = new List<BoundPropertyChangedEventArgs>();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            testObject.Disconnect();

            childEventStream.OnNext(new BoundPropertyChangedEventArgs(propertyName, mockObjectPropertyBinding.Object));

            Assert.Empty(events);
        }

        [Fact]
        public void property_changed_is_propagated_from_child_node_after_reconnect()
        {
            var propertyName = "X";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            InitMocks();
            InitTestObjectWithPropertyChangedStream(childEventStream, obj);

            var events = new List<BoundPropertyChangedEventArgs>();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            testObject.Disconnect();

            childEventStream.OnNext(new BoundPropertyChangedEventArgs(propertyName, mockObjectPropertyBinding.Object));

            Assert.Empty(events);

            testObject.Connect(obj);

            childEventStream.OnNext(new BoundPropertyChangedEventArgs(propertyName, mockObjectPropertyBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal(propertyName, events[0].PropertyName);
            Assert.Equal(mockObjectPropertyBinding.Object, events[0].Binding);
        }

        // --------------------------------------
        // Property changing event stream.
        // --------------------------------------

        [Fact]
        public void property_changing_is_propagated()
        {
            var propertyName = "X";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();

            InitMocks();

            mockObjectPropertyBinding
                .Setup(m => m.PropertyName)
                .Returns(propertyName);

            InitTestObjectWithPropertyChangingStream(childEventStream, obj);

            var events = new List<BoundPropertyChangingEventArgs>();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            obj.X = 5;

            Assert.Equal(1, events.Count);
            Assert.Equal(propertyName, events[0].PropertyName);
            Assert.Equal(mockObjectPropertyBinding.Object, events[0].Binding);
        }

        [Fact]
        public void property_changing_is_not_propagated_after_disconnect()
        {
            var propertyName = "X";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();

            InitMocks();

            mockObjectPropertyBinding
                .Setup(m => m.PropertyName)
                .Returns(propertyName);

            InitTestObjectWithPropertyChangingStream(childEventStream, obj);

            var events = new List<BoundPropertyChangingEventArgs>();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            testObject.Disconnect();
            obj.X = 5;

            Assert.Empty(events);
        }

        [Fact]
        public void property_changing_is_propagated_after_reconnection()
        {
            var propertyName = "X";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();

            InitMocks();

            mockObjectPropertyBinding
                .Setup(m => m.PropertyName)
                .Returns(propertyName);

            InitTestObjectWithPropertyChangingStream(childEventStream, obj);

            var events = new List<BoundPropertyChangingEventArgs>();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            testObject.Disconnect();
            obj.X = 5;

            Assert.Empty(events);

            testObject.Connect(obj);
            obj.X = 10;

            Assert.Equal(1, events.Count);
            Assert.Equal(propertyName, events[0].PropertyName);
            Assert.Equal(mockObjectPropertyBinding.Object, events[0].Binding);
        }

        [Fact]
        public void property_changing_is_propagated_from_child_node()
        {
            var propertyName = "X";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();

            InitMocks();
            InitTestObjectWithPropertyChangingStream(childEventStream, obj);

            var events = new List<BoundPropertyChangingEventArgs>();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            childEventStream.OnNext(new BoundPropertyChangingEventArgs(propertyName, mockObjectPropertyBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal(propertyName, events[0].PropertyName);
            Assert.Equal(mockObjectPropertyBinding.Object, events[0].Binding);
        }

        [Fact]
        public void property_changing_is_not_propagated__from_child_node_after_disconnect()
        {
            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();

            InitMocks();
            InitTestObjectWithPropertyChangingStream(childEventStream, obj);

            var events = new List<BoundPropertyChangingEventArgs>();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            testObject.Disconnect();

            childEventStream.OnNext(new BoundPropertyChangingEventArgs("X", mockObjectPropertyBinding.Object));

            Assert.Empty(events);
        }

        [Fact]
        public void property_changing_is_propagated_from_child_node_after_reconnect()
        {
            var propertyName = "X";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();

            InitMocks();
            InitTestObjectWithPropertyChangingStream(childEventStream, obj);

            var testObject = new ObjectBinding(obj, mockBindingsFactory.Object);

            List<BoundPropertyChangingEventArgs> events = new List<BoundPropertyChangingEventArgs>();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            testObject.Disconnect();

            Assert.Empty(events);

            testObject.Connect(obj);

            childEventStream.OnNext(new BoundPropertyChangingEventArgs(propertyName, mockObjectPropertyBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal(propertyName, events[0].PropertyName);
            Assert.Equal(mockObjectPropertyBinding.Object, events[0].Binding);
        }

        // --------------------------------------
        // Collection changed event stream.
        // --------------------------------------

        [Fact]
        public void collection_changed_is_propagated_from_child_node()
        {
            var propertyName = "Nested";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();

            InitMocks();
            InitTestObjectWithCollectionChangedStream(childEventStream, obj);

            var events = new List<BoundCollectionChangedEventArgs>();

            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs();
            childEventStream.OnNext(new BoundCollectionChangedEventArgs(propertyName, fakeCollectionChangedEventArgs, null, null));

            Assert.Equal(1, events.Count);
            Assert.Equal(propertyName, events[0].PropertyName);
            Assert.Equal(fakeCollectionChangedEventArgs, events[0].SourceEventArgs);
        }

        [Fact]
        public void collection_changed_is_not_propagated_from_child_node_after_disconnect()
        {
            var propertyName = "Nested";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();

            InitMocks();
            InitTestObjectWithCollectionChangedStream(childEventStream, obj);

            var events = new List<BoundCollectionChangedEventArgs>();

            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            testObject.Disconnect();

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs();
            childEventStream.OnNext(new BoundCollectionChangedEventArgs(propertyName, fakeCollectionChangedEventArgs, null, null));

            Assert.Empty(events);
        }

        [Fact]
        public void collection_changed_is_propagated_from_child_node_after_reconnect()
        {
            var propertyName = "Nested";

            var obj = new TestClass();
            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();

            InitMocks();
            InitTestObjectWithCollectionChangedStream(childEventStream, obj);

            var events = new List<BoundCollectionChangedEventArgs>();

            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            testObject.Disconnect();

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs();
            childEventStream.OnNext(new BoundCollectionChangedEventArgs(propertyName, fakeCollectionChangedEventArgs, null, null));

            Assert.Empty(events);

            testObject.Connect(obj);

            childEventStream.OnNext(new BoundCollectionChangedEventArgs(propertyName, fakeCollectionChangedEventArgs, null, null));

            Assert.Equal(1, events.Count);
            Assert.Equal("Nested", events[0].PropertyName);
            Assert.Equal(fakeCollectionChangedEventArgs, events[0].SourceEventArgs);
        }
    }
}
