using Moq;
using RSG.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using Xunit;

namespace RSG.Tests
{
    public class BindableStreamTests
    {
        BindableStream<int> testObject;

        void Init()
        {
            testObject = new BindableStream<int>();
        }

        [Fact]
        public void doesnt_receive_before_binding()
        {
            Init();

            int testInt = 0;

            testObject.Subscribe(i => testInt += i);

            Assert.Equal(0, testInt);
        }

        [Fact]
        public void receives_on_binding_before_subscribing()
        {
            Init();

            int testInt = 0;

            var eventStream = new Subject<int>();

            testObject.Bind(eventStream);

            testObject.Subscribe(i => testInt += 1);

            eventStream.OnNext(1);

            Assert.Equal(1, testInt);
        }

        [Fact]
        public void receives_on_binding_after_subscribing()
        {
            Init();

            int testInt = 0;

            var eventStream = new Subject<int>();

            testObject.Subscribe(i => testInt += 1);

            testObject.Bind(eventStream);

            eventStream.OnNext(1);

            Assert.Equal(1, testInt);
        }

        [Fact]
        public void stops_receiving_after_unbinding()
        {
            Init();

            int testInt = 0;

            var eventStream = new Subject<int>();

            testObject.Bind(eventStream);

            testObject.Subscribe(i => testInt += 1);

            testObject.Unbind();

            eventStream.OnNext(1);

            Assert.Equal(0, testInt);
        }

        [Fact]
        public void stops_receiving_after_unsubscribe()
        {
            Init();

            int testInt = 0;

            var eventStream = new Subject<int>();

            testObject.Bind(eventStream);

            using (testObject.Subscribe(i => testInt += 1))
            {
                // Disposed after using statement which unsubscribes.
            }

            eventStream.OnNext(1);

            Assert.Equal(0, testInt);
        }
    }
}
