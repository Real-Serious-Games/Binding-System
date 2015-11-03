using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using Xunit;

namespace RSG.Tests
{
    public class ListItemBindingTests
    {
        public class TestClass
        {
            public int X { get; set; }
        }

        Mock<IValueBinding> mockValueBinding;

        private ObservableList<TestClass> testList;

        private ListItemBinding testObject;

        private void Init()
        {
            var list = new ObservableList<TestClass>()
            {
                new TestClass()
                {
                    X = 0
                }
            };

            InitMocks();
            InitTestObject(0, list);
        }

        private void InitMocks()
        {
            mockValueBinding = new Mock<IValueBinding>();
        }

        private void InitTestObject(int index, ObservableList<TestClass> list)
        {
            testList = list;

            testObject = new ListItemBinding(index, list, mockValueBinding.Object);
        }

        [Fact]
        public void get_value_index_0()
        {
            Init();

            Assert.Equal(testList[0], testObject.GetValue());
        }

        [Fact]
        public void get_value_index_1()
        {
            InitMocks();

            var list = new ObservableList<TestClass>()
            {
                new TestClass()
                {
                    X = 13
                },
                new TestClass()
                {
                    X = 23
                }
            };

            InitTestObject(1, list);

            Assert.Equal(list[1], testObject.GetValue());
        }

        [Fact]
        public void set_value_index_0()
        {
            Init();

            Assert.Equal(0, testList[0].X);

            testObject.SetValue(new TestClass() { X = 32 });

            Assert.Equal(32, testList[0].X);
        }

        [Fact]
        public void set_value_index_1()
        {
            InitMocks();

            var list = new ObservableList<TestClass>()
            {
                new TestClass()
                {
                    X = 0
                },
                new TestClass()
                {
                    X = 0
                }
            };

            InitTestObject(1, list);

            Assert.Equal(0, testList[1].X);

            testObject.SetValue(new TestClass() { X = 32 });

            Assert.Equal(32, testList[1].X);
        }

        [Fact]
        public void disconnect_calls_disconnect_on_value_binding()
        {
            Init();

            mockValueBinding.Verify(m => m.Connect(testList[0]), Times.Never());

            testObject.Disconnect();

            mockValueBinding.Verify(m => m.Disconnect(), Times.Once());
        }

        [Fact]
        public void reconnect_calls_connect_on_value_binding()
        {
            Init();

            var otherList = new ObservableList<TestClass>()
            {
                new TestClass()
                {
                    X = 10
                }
            };

            testObject.Connect(otherList);

            mockValueBinding.Verify(m => m.Disconnect(), Times.Once());
            mockValueBinding.Verify(m => m.Connect(otherList[0]), Times.Once());
        }

        [Fact]
        public void explicit_disconnect_and_reconnect_calls_connect_on_value_binding()
        {
            Init();

            testObject.Disconnect();

            var otherList = new ObservableList<TestClass>()
            {
                new TestClass()
                {
                    X = 10
                }
            };

            testObject.Connect(otherList);

            mockValueBinding.Verify(m => m.Disconnect(), Times.Once());
            mockValueBinding.Verify(m => m.Connect(otherList[0]), Times.Once());
        }

        [Fact]
        public void binding_type()
        {
            Init();

            Assert.Equal(typeof(TestClass), testObject.BindingType);
        }

        [Fact]
        public void FindNestedBinding_with_null_string()
        {
            Init();

            Assert.Throws<ArgumentException>(() =>
                testObject.FindNestedBinding(null)
            );
        }

        [Fact]
        public void test_FindNestedBinding_with_empty_string()
        {
            Init();

            Assert.Throws<ArgumentException>(() =>
                testObject.FindNestedBinding(string.Empty)
            );
        }

        [Fact]
        public void test_find_nested_binding()
        {
            Init();
            
            var mockNestedBinding = new Mock<IBinding>();

            mockValueBinding
                .Setup(m => m.FindNestedBinding("Other"))
                .Returns(mockNestedBinding.Object);

            var nestedObject = testObject.FindNestedBinding("Other");

            Assert.Equal(mockNestedBinding.Object, nestedObject);
        }

        [Fact]
        public void BoundPropertyChanging_event_passed_on()
        {
            Init();

            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();

            mockValueBinding
                .Setup(m => m.PropertyChangingEventStream)
                .Returns(childEventStream);

            var events = new List<BoundPropertyChangingEventArgs>();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            var mockBinding = new Mock<IBinding>();

            childEventStream.OnNext(new BoundPropertyChangingEventArgs("Nested", mockBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("0.Nested", events[0].PropertyName);
            Assert.Equal(mockBinding.Object, events[0].Binding);
        }

        [Fact]
        public void BoundPropertyChanged_event_passed_on()
        {
            Init();

            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            mockValueBinding
                .Setup(m => m.PropertyChangedEventStream)
                .Returns(childEventStream);

            var events = new List<BoundPropertyChangedEventArgs>();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            var mockBinding = new Mock<IBinding>();

            childEventStream.OnNext(new BoundPropertyChangedEventArgs("Nested", mockBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("0.Nested", events[0].PropertyName);
            Assert.Equal(mockBinding.Object, events[0].Binding);
        }

        [Fact]
        public void BoundCollectionChanged_event_passed_on()
        {
            Init();

            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();

            mockValueBinding
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(childEventStream);

            var events = new List<BoundCollectionChangedEventArgs>();

            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            childEventStream.OnNext(new BoundCollectionChangedEventArgs("Nested", fakeCollectionChangedEventArgs, null, null));

            Assert.Equal(1, events.Count);
            Assert.Equal("0.Nested", events[0].PropertyName);
            Assert.Equal(fakeCollectionChangedEventArgs, events[0].SourceEventArgs);

        }
    }
}
