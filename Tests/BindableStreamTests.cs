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
        public void subscribing_to_bindablestream_doesnt_throw_exception()
        {
            Init();

            int testInt = 0;

            Assert.DoesNotThrow(() =>
            {
                testObject.Subscribe(i =>
                {
                    testInt += i;
                });
            });
        }

        [Fact]
        public void subscribing_to_bindablestream_does_not_receive_without_being_bound()
        {
            Init();

            int testInt = 0;

            testObject.Subscribe(i =>
            {
                testInt += i;
            });

            Assert.Equal(0, testInt);
        }

        [Fact]
        public void binding_to_bindablestream_after_subscribing_receives()
        {
            Init();

            int testInt = 0;

            var eventStream = new Subject<int>();

            testObject.Bind(eventStream);

            testObject.Subscribe(i =>
                {
                    testInt += 1;
                });

            eventStream.OnNext(1);

            Assert.Equal(1, testInt);
        }
    }
}
