using Moq;
using RSG.Internal;
using RSG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RSG.Tests
{
    public class BindingsFactoryTests
    {
        public class TestClass
        {
            public int NotBound { get; set; }

            [Binding]
            public int X { get; set; }

            [Binding]
            public float Y { get; set; }

            [Binding]
            public string S { get; set; }

            [Binding]
            public NestedTestClass Nested { get; set; }
        };

        [Binding]
        public class NestedTestClass
        {
            public int X { get; set; }
        }

        [Fact]
        public void test_CreateObjectBindings()
        {
            var obj = new TestClass()
            {
                X = 22,
                Y = 3.3f,
                S = "Hi",
                Nested = new NestedTestClass()
                {
                    X = 15
                }
            };

            var testObject = new BindingsFactory();

            var bindings = testObject.CreateObjectBindings(obj);

            Assert.Equal(4, bindings.Length);
            Assert.Equal("X", bindings[0].PropertyName);
            Assert.Equal("Y", bindings[1].PropertyName);
            Assert.Equal("S", bindings[2].PropertyName);
            Assert.Equal("Nested", bindings[3].PropertyName);
        }

        [Fact]
        public void test_CreateListBinding()
        {
            var list = new ObservableList<int>()
            {
                10
            };

            var testObject = new BindingsFactory();
            

            var binding = testObject.CreateListBinding(10, 0, list);

            Assert.Equal(0, binding.ItemIndex);
        }

        [Fact]
        public void test_CreateListBindings()
        {
            var list = new ObservableList<int>()
            {
                1,
                2,
                3
            };


            var testObject = new BindingsFactory();
            

            var bindings = testObject.CreateListBindings(list);

            Assert.Equal(3, bindings.Length);
            Assert.Equal(0, bindings[0].ItemIndex);
            Assert.Equal(1, bindings[1].ItemIndex);
            Assert.Equal(2, bindings[2].ItemIndex);
        }

        [Fact]
        public void test_CreateArrayBinding()
        {
            var array = new int[]
            {
                10
            };

            var testObject = new BindingsFactory();
            

            var itemIndex = 0;
            var binding = testObject.CreateArrayBinding(array[0], itemIndex, array);

            Assert.Equal(itemIndex, binding.ItemIndex);
        }

        [Fact]
        public void test_CreateArrayBindings()
        {
            var array = new int[]
            {
                1,
                2,
                3
            };

            var testObject = new BindingsFactory();
            

            var bindings = testObject.CreateArrayBindings(array);

            Assert.Equal(3, bindings.Length);
            Assert.Equal(0, bindings[0].ItemIndex);
            Assert.Equal(1, bindings[1].ItemIndex);
            Assert.Equal(2, bindings[2].ItemIndex);
        }

        public class TestClassWithUnboundProperty
        {
            public string X { get; set; }
        }

        [Fact]
        public void test_unbound_property_result_in_no_bindings()
        {
            var testObject = new BindingsFactory();
            

            var bindings = testObject.CreateObjectBindings(new TestClassWithUnboundProperty());

            Assert.Equal(0, bindings.Length);
        }

        public class TestClassWithNullProperty
        {
            [Binding]
            public string X { get; set; }
        }

        [Fact]
        public void test_can_bind_to_null_property()
        {
            var testObject = new BindingsFactory();
            

            var bindings = testObject.CreateObjectBindings(new TestClassWithNullProperty());

            Assert.Equal(1, bindings.Length);
            Assert.NotNull(bindings[0]);
        }

        [Fact]
        public void can_create_array_binding()
        {
            var testObject = new BindingsFactory();
            

            var parent = new object();
            var array = new int[0];

            var binding = testObject.CreateValueBinding(parent, array, array.GetType());

            Assert.NotNull(binding);
            Assert.IsType(typeof(ArrayBinding), binding);
        }

        [Fact]
        public void can_create_list_binding()
        {
            var testObject = new BindingsFactory();
            

            var parent = new object();
            var list = new ObservableList<int>();

            var binding = testObject.CreateValueBinding(parent, list, list.GetType());

            Assert.NotNull(binding);
            Assert.IsType(typeof(ListBinding), binding);
        }

        [Fact]
        public void can_create_object_binding()
        {
            var testObject = new BindingsFactory();
            

            var parent = new object();
            var obj = new object();

            var binding = testObject.CreateValueBinding(parent, obj, obj.GetType());

            Assert.NotNull(binding);
            Assert.IsType(typeof(ObjectBinding), binding);
        }

        [Fact]
        public void can_create_primitive_binding()
        {
            var testObject = new BindingsFactory();
            

            var parent = new object();
            var primValue = 1;

            var binding = testObject.CreateValueBinding(parent, primValue, primValue.GetType());

            Assert.NotNull(binding);
            Assert.IsType(typeof(PrimitiveBinding), binding);
        }
    }
}
