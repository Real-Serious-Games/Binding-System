using Moq;
using RSG.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Xunit;

namespace RSG.Tests
{
    public class ArrayBindingTests
    {
        public class TestClass
        {
            public int X { get; set; }
        }

        private Mock<IBindingsFactory> mockBindingsFactory;
        private Mock<IArrayItemBinding>[] mockArrayItemBindings;

        private ArrayBinding testObject;

        private TestClass[] testArray;

        private void Init()
        {
            InitMocks(1);

            InitTestObject(testArray);
        }

        private void InitMocks(int numOfTestClasses)
        {
            mockBindingsFactory = new Mock<IBindingsFactory>();

            testArray = Enumerable.Range(0, numOfTestClasses)
                .Select(i => new TestClass())
                .ToArray();

            mockArrayItemBindings = Enumerable.Range(0, numOfTestClasses)
                .Select(i => 
                {
                    var mockBinding = new Mock<IArrayItemBinding>();

                    mockBinding
                        .Setup(m => m.ItemIndex)
                        .Returns(i);

                    return mockBinding;
                })
                .ToArray();

            mockBindingsFactory
               .Setup(m => m.CreateArrayBindings(testArray))
               .Returns(mockArrayItemBindings.Select(m => m.Object).ToArray());
        }

        private void InitTestObject(TestClass[] array)
        {
            testObject = new ArrayBinding(array, mockBindingsFactory.Object);
        }

        [Fact]
        public void find_item_binding()
        {
            Init();

            Assert.Equal(mockArrayItemBindings[0].Object, testObject.FindNestedBinding("0"));
        }


        [Fact]
        public void find_nested_item_binding()
        {
            InitMocks(1);

            var mockNestedBinding = new Mock<IBinding>();

            mockArrayItemBindings[0]
                .Setup(m => m.FindNestedBinding("Foo"))
                .Returns(mockNestedBinding.Object);

            InitTestObject(testArray);

            Assert.Equal(mockNestedBinding.Object, testObject.FindNestedBinding("0.Foo"));
        }

        [Fact]
        public void BoundPropertyChanging_event_from_child_passed_on()
        {
            InitMocks(1);

            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();

            mockArrayItemBindings[0]
                .Setup(m => m.PropertyChangingEventStream)
                .Returns(childEventStream);

            InitTestObject(testArray);
            
            var events = new List<BoundPropertyChangingEventArgs>();
            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            childEventStream.OnNext(new BoundPropertyChangingEventArgs("Nested", mockArrayItemBindings[0].Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("Nested", events[0].PropertyName);
            Assert.Equal(mockArrayItemBindings[0].Object, events[0].Binding);
        }

        [Fact]
        public void BoundPropertyChanged_event_from_child_passed_on()
        {
            InitMocks(1);

            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();

            mockArrayItemBindings[0]
                .Setup(m => m.PropertyChangedEventStream)
                .Returns(childEventStream);

            InitTestObject(testArray);

            var events = new List<BoundPropertyChangedEventArgs>();
            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            childEventStream.OnNext(new BoundPropertyChangedEventArgs("Nested", mockArrayItemBindings[0].Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("Nested", events[0].PropertyName);
            Assert.Equal(mockArrayItemBindings[0].Object, events[0].Binding);
        }

        [Fact]
        public void BoundCollectionChanged_event_from_child_passed_on()
        {
            InitMocks(1);

            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();

            mockArrayItemBindings[0]
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(childEventStream);

            InitTestObject(testArray);

            var events = new List<BoundCollectionChangedEventArgs>();
            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            childEventStream.OnNext(new BoundCollectionChangedEventArgs("Nested", fakeCollectionChangedEventArgs, null, null));

            Assert.Equal(1, events.Count);
            Assert.Equal("Nested", events[0].PropertyName);
            Assert.Equal(fakeCollectionChangedEventArgs, events[0].SourceEventArgs);

        }
    }
}
