using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive.Subjects;
using Xunit;

namespace RSG.Tests
{
    public class ObjectPropertyBiindingTests
    {
        public class TestClass
        {
            public int X { get; set; }
        }

        [Fact]
        public void disconnect_calls_disconnect_on_value_binding()
        {
            var obj = new TestClass();
            var mockValueBinding = new Mock<IValueBinding>();

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            mockValueBinding.Verify(m => m.Connect(obj.X), Times.Never());

            testObject.Disconnect();

            mockValueBinding.Verify(m => m.Disconnect(), Times.Once());
        }

        [Fact]
        public void reconnect_calls_disconnect_on_binding_and_connect_with_new_object()
        {
            var obj = new TestClass();
            var mockValueBinding = new Mock<IValueBinding>();

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            var otherObj = new TestClass()
            {
                X = 10
            };

            testObject.Connect(otherObj);

            mockValueBinding.Verify(m => m.Disconnect(), Times.Once());

            mockValueBinding.Verify(m => m.Connect(otherObj.X), Times.Once());
        }

        [Fact]
        public void connect_does_not_call_disconnect_when_not_connected()
        {
            var obj = new TestClass();
            var mockValueBinding = new Mock<IValueBinding>();

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            testObject.Disconnect();

            var otherObj = new TestClass()
            {
                X = 10
            };

            testObject.Connect(otherObj);

            mockValueBinding.Verify(m => m.Disconnect(), Times.Once());

            mockValueBinding.Verify(m => m.Connect(otherObj.X), Times.Once());
        }

        [Fact]
        public void get_value()
        {
            var obj = new TestClass()
            {
                X = 13
            };
            var mockValueBinding = new Mock<IValueBinding>();

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            Assert.Equal(13, testObject.GetValue());
        }

        [Fact]
        public void set_value()
        {
            var obj = new TestClass()
            {
                X = 0
            };
            var mockValueBinding = new Mock<IValueBinding>();

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            Assert.Equal(0, obj.X);

            testObject.SetValue(32);

            Assert.Equal(32, obj.X);
        }

        [Fact]
        public void binding_type()
        {
            var obj = new TestClass();
            var mockValueBinding = new Mock<IValueBinding>();

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            Assert.Equal(typeof(int), testObject.BindingType);
        }

        [Fact]
        public void find_nested_binding_with_empty_string()
        {
            var obj = new TestClass();
            var mockValueBinding = new Mock<IValueBinding>();

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            Assert.Throws<ArgumentException>(() => 
                testObject.FindNestedBinding(string.Empty)
            );
        }

        [Fact]
        public void find_nested_binding()
        {
            var obj = new TestClass();
            var mockNestedBinding = new Mock<IBinding>();
            var mockValueBinding = new Mock<IValueBinding>();

            mockValueBinding
                .Setup(m => m.FindNestedBinding("Other"))
                .Returns(mockNestedBinding.Object);

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            Assert.Equal(mockNestedBinding.Object, testObject.FindNestedBinding("Other"));

            mockValueBinding.Verify(m => m.FindNestedBinding("Other"));
        }

        [Fact]
        public void property_changing_is_propagated()
        {
            var obj = new TestClass()
            {
                X = 15
            };

            var childEventStream = new Subject<BoundPropertyChangingEventArgs>();
            var mockValueBinding = new Mock<IValueBinding>();
            var mockBinding = new Mock<IBinding>();

            mockValueBinding
                .Setup(m => m.PropertyChangingEventStream)
                .Returns(childEventStream);

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            var events = new List<BoundPropertyChangingEventArgs>();

            testObject.PropertyChangingEventStream.Subscribe(ev => events.Add(ev));

            childEventStream.OnNext(new BoundPropertyChangingEventArgs("Nested", mockBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("X.Nested", events[0].PropertyName);
            Assert.Equal(mockBinding.Object, events[0].Binding);
        }

        [Fact]
        public void property_changed_is_propagated()
        {
            var obj = new TestClass()
            {
                X = 15
            };

            var childEventStream = new Subject<BoundPropertyChangedEventArgs>();
            var mockValueBinding = new Mock<IValueBinding>();
            var mockBinding = new Mock<IBinding>();

            mockValueBinding
                .Setup(m => m.PropertyChangedEventStream)
                .Returns(childEventStream);

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            var events = new List<BoundPropertyChangedEventArgs>();

            testObject.PropertyChangedEventStream.Subscribe(ev => events.Add(ev));

            childEventStream.OnNext(new BoundPropertyChangedEventArgs("Nested", mockBinding.Object));

            Assert.Equal(1, events.Count);
            Assert.Equal("X.Nested", events[0].PropertyName);
            Assert.Equal(mockBinding.Object, events[0].Binding);
        }

        [Fact]
        public void collection_changed_is_propagated()
        {
            var obj = new TestClass()
            {
                X = 15
            };

            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();
            var mockValueBinding = new Mock<IValueBinding>();
            var mockBinding = new Mock<IBinding>();

            mockValueBinding
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(childEventStream);

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            var events = new List<BoundCollectionChangedEventArgs>();

            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));            

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs();
            childEventStream.OnNext(new BoundCollectionChangedEventArgs("Nested", fakeCollectionChangedEventArgs, null, null));

            Assert.Equal(1, events.Count);
            Assert.Equal("X.Nested", events[0].PropertyName);
            Assert.Equal(fakeCollectionChangedEventArgs, events[0].SourceEventArgs);
        }

        [Fact]
        public void collection_changed_is_propagated_with_null_property_name()
        {
            var obj = new TestClass()
            {
                X = 15
            };

            var childEventStream = new Subject<BoundCollectionChangedEventArgs>();
            var mockValueBinding = new Mock<IValueBinding>();
            var mockBinding = new Mock<IBinding>();

            mockValueBinding
                .Setup(m => m.CollectionChangedEventStream)
                .Returns(childEventStream);

            var testObject = new ObjectPropertyBinding("X", typeof(int), obj, mockValueBinding.Object);

            var events = new List<BoundCollectionChangedEventArgs>();

            testObject.CollectionChangedEventStream.Subscribe(ev => events.Add(ev));            

            var fakeCollectionChangedEventArgs = new NotifyCollectionChangedEventArgs();
            childEventStream.OnNext(new BoundCollectionChangedEventArgs(null, fakeCollectionChangedEventArgs, null, null));

            Assert.Equal(1, events.Count);
            Assert.Equal("X", events[0].PropertyName);
            Assert.Equal(fakeCollectionChangedEventArgs, events[0].SourceEventArgs);
        }
    }
}
