using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RSG
{   
    public class IntergrationTests
    {
        private class TestDataStructure
        {
            [Binding]
            public ObservableList<int> TestList
            {
                get
                {
                    return testList;
                }
                set
                {
                    testList = value;
                }
            }
            private ObservableList<int> testList = new ObservableList<int>() { 1, 2, 3, 4 };
        }

        [Fact]
        public void collection_changed_event_is_recieved_when_list_element_is_changed()
        {

            var testDataStructure = new TestDataStructure();
            var bindingManager = new BindingManager(testDataStructure);
            var events = new List<BoundPropertyChangedEventArgs>();
            bindingManager.PropertyChangedEventStream.Subscribe(ev =>
            {
                events.Add(ev);
            });

            bindingManager.CollectionChangedEventStream.Subscribe(ev => 
            {
                var thisEvent = ev;
            });

            bindingManager.PropertyChangingEventStream.Subscribe(ev =>
            {
                var thisEvent = ev;
            });

            testDataStructure.TestList[0] = 5;

            Assert.Equal(1, events.Count);
            //Assert.Equal("intArray", events[0].PropertyName);
            //Assert.Equal(events[0].)
        }
    }
}
