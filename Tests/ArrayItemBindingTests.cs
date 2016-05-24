using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using Xunit;

namespace RSG.Tests
{
    public class ArrayItemBindingTests
    {
        public class TestClass
        {
            public int X 
            { 
                get
                {
                    return x;
                }
                set
                {
                    x = value;
                }
            }
            private int x = 0;
        }

        private Mock<IValueBinding> mockValueBinding;

        private ArrayItemBinding testObject;

        private TestClass[] testArray;

        private void Init()
        {
            InitMocks();

            InitTestObject(1, 0);
        }

        private void InitMocks()
        {
            mockValueBinding = new Mock<IValueBinding>();
        }

        private void InitTestObject(int lengthOfArray, int index)
        {
            testArray = Enumerable.Range(0, lengthOfArray)
                .Select(i => new TestClass())
                .ToArray();

            testObject = new ArrayItemBinding(index, testArray, mockValueBinding.Object);
        }


        [Fact]
        public void can_disconnect()
        {
            Init();
            
            mockValueBinding.Verify(m => m.Connect(testArray[0]), Times.Never());

            testObject.Disconnect();

            mockValueBinding.Verify(m => m.Disconnect(), Times.Once());
        }

        [Fact]
        public void can_reconnect()
        {
            Init();

            var otherList = new TestClass[]
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
        public void get_value_index_0()
        {
            Init();

            Assert.Equal(testArray[0], testObject.GetValue());
        }

        [Fact]
        public void get_value_index_1()
        {
            InitMocks();

            InitTestObject(2, 1);

            Assert.Equal(testArray[1], testObject.GetValue());
        }

        [Fact]
        public void set_value_index_0()
        {
            Init();

            Assert.Equal(0, testArray[0].X);

            testObject.SetValue(new TestClass() { X = 32 });

            Assert.Equal(32, testArray[0].X);
        }

        [Fact]
        public void set_value_index_1()
        {
            InitMocks();

            InitTestObject(2, 1);

            Assert.Equal(0, testArray[1].X);

            testObject.SetValue(new TestClass() { X = 32 });

            Assert.Equal(32, testArray[1].X);
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
        public void FindNestedBinding_with_empty_string()
        {
            Init();

            Assert.Throws<ArgumentException>(() =>
                testObject.FindNestedBinding(string.Empty)
            );
        }

        [Fact]
        public void find_nested_binding()
        {
            InitMocks();
            
            var mockNestedBinding = new Mock<IBinding>();

            mockValueBinding
                .Setup(m => m.FindNestedBinding("Other"))
                .Returns(mockNestedBinding.Object);

            InitTestObject(1, 0);

            Assert.Equal(mockNestedBinding.Object, testObject.FindNestedBinding("Other"));

            mockValueBinding.Verify(m => m.FindNestedBinding("Other"));
        }

        [Fact]
        public void BoundPropertyChanging_event_from_child_passed_on()
        {
            InitMocks();

            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();
            var mockChildBinding = new Mock<IBinding>();

            mockValueBinding
                .Setup(m => m.PropertyChangingEventStream)
                .Returns(childEventStream);

            InitTestObject(1, 0);

            var events = new List<BoundPropertyChangingEventArgs>();
            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            childEventStream.OnNext(new BoundPropertyChangingEventArgs("Nested", mockChildBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("0.Nested", events[0].PropertyName);
            Assert.Equal(mockChildBinding.Object, events[0].Binding);
        }

        [Fact]
        public void BoundPropertyChanged_event_from_child_passed_on()
        {
            InitMocks();

            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();
            var mockChildBinding = new Mock<IObjectPropertyBinding>();

            mockValueBinding
                .Setup(m => m.PropertyChangedEventStream)
                .Returns(childEventStream);

            InitTestObject(1, 0);

            var events = new List<BoundPropertyChangedEventArgs>();
            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add((BoundPropertyChangedEventArgs)ev));

            childEventStream.OnNext(new BoundPropertyChangedEventArgs("Nested", mockChildBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("0.Nested", events[0].PropertyName);
            Assert.Equal(mockChildBinding.Object, events[0].Binding);
        }

        [Fact]
        public void BoundCollectionChanged_event_from_child_passed_on()
        {
            InitMocks();

            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();

            mockValueBinding
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(childEventStream);

            InitTestObject(1, 0);

            var events = new List<BoundCollectionChangedEventArgs>();
            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs();
            childEventStream.OnNext(new BoundCollectionChangedEventArgs("Nested", fakeCollectionChangedEventArgs, null, null));

            Assert.Equal(1, events.Count);
            Assert.Equal("0.Nested", events[0].PropertyName);
            Assert.Equal(fakeCollectionChangedEventArgs, events[0].SourceEventArgs);
        }
    }
}
