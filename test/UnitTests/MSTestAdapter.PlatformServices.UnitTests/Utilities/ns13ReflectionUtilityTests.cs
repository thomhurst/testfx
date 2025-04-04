﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.TestPlatform.MSTestAdapter.PlatformServices.Utilities;

using TestFramework.ForTestingMSTest;

namespace MSTestAdapter.PlatformServices.Tests.Utilities;

#pragma warning disable SA1649 // File name must match first type name
public class ReflectionUtilityTests : TestContainer
#pragma warning restore SA1649 // File name must match first type name
{
    public void GetCustomAttributesShouldReturnAllAttributes()
    {
        MethodInfo methodInfo = typeof(DummyBaseTestClass).GetMethod("DummyVTestMethod1")!;

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(methodInfo, false);

        Verify(attributes is not null);
        Verify(attributes.Count == 2);

        string[] expectedAttributes = ["DummyA : base", "DummySingleA : base"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    public void GetCustomAttributesShouldReturnAllAttributesIgnoringBaseInheritance()
    {
        MethodInfo methodInfo = typeof(DummyTestClass).GetMethod("DummyVTestMethod1")!;

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(methodInfo, false);

        Verify(attributes is not null);
        Verify(attributes.Count == 2);

        string[] expectedAttributes = ["DummyA : derived", "DummySingleA : derived"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    public void GetCustomAttributesShouldReturnAllAttributesWithBaseInheritance()
    {
        MethodInfo methodInfo = typeof(DummyTestClass).GetMethod("DummyVTestMethod1")!;

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(methodInfo, true);

        Verify(attributes is not null);
        Verify(attributes.Count == 3);

        // Notice that the DummySingleA on the base method does not show up since it can only be defined once.
        string[] expectedAttributes = ["DummyA : derived", "DummySingleA : derived", "DummyA : base"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    public void GetCustomAttributesOnTypeShouldReturnAllAttributes()
    {
        Type type = typeof(DummyBaseTestClass);

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(type, false);

        Verify(attributes is not null);
        Verify(attributes.Count == 1);

        string[] expectedAttributes = ["DummyA : ba"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    public void GetCustomAttributesOnTypeShouldReturnAllAttributesIgnoringBaseInheritance()
    {
        Type type = typeof(DummyTestClass);

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(type, false);

        Verify(attributes is not null);
        Verify(attributes.Count == 1);

        string[] expectedAttributes = ["DummyA : a"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    public void GetCustomAttributesOnTypeShouldReturnAllAttributesWithBaseInheritance()
    {
        Type method = typeof(DummyTestClass);

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(method, true);

        Verify(attributes is not null);
        Verify(attributes.Count == 2);

        string[] expectedAttributes = ["DummyA : a", "DummyA : ba"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    public void GetSpecificCustomAttributesShouldReturnAllAttributes()
    {
        MethodInfo methodInfo = typeof(DummyBaseTestClass).GetMethod("DummyVTestMethod1")!;

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(methodInfo, typeof(DummyAAttribute), false);

        Verify(attributes is not null);
        Verify(attributes.Count == 1);

        string[] expectedAttributes = ["DummyA : base"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    public void GetSpecificCustomAttributesShouldReturnAllAttributesIgnoringBaseInheritance()
    {
        MethodInfo methodInfo = typeof(DummyTestClass).GetMethod("DummyVTestMethod1")!;

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(methodInfo, typeof(DummyAAttribute), false);

        Verify(attributes is not null);
        Verify(attributes.Count == 1);

        string[] expectedAttributes = ["DummyA : derived"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    public void GetSpecificCustomAttributesShouldReturnAllAttributesWithBaseInheritance()
    {
        MethodInfo methodInfo = typeof(DummyTestClass).GetMethod("DummyVTestMethod1")!;

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(methodInfo, typeof(DummyAAttribute), true);

        Verify(attributes is not null);
        Verify(attributes.Count == 2);

        string[] expectedAttributes = ["DummyA : derived", "DummyA : base"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    public void GetSpecificCustomAttributesOnTypeShouldReturnAllAttributes()
    {
        Type type = typeof(DummyBaseTestClass);

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(type, typeof(DummyAAttribute), false);

        Verify(attributes is not null);
        Verify(attributes.Count == 1);

        string[] expectedAttributes = ["DummyA : ba"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    public void GetSpecificCustomAttributesOnTypeShouldReturnAllAttributesIgnoringBaseInheritance()
    {
        Type type = typeof(DummyTestClass);

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(type, typeof(DummyAAttribute), false);

        Verify(attributes is not null);
        Verify(attributes.Count == 1);

        string[] expectedAttributes = ["DummyA : a"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    public void GetSpecificCustomAttributesOnTypeShouldReturnAllAttributesWithBaseInheritance()
    {
        Type method = typeof(DummyTestClass);

        IReadOnlyList<object> attributes = ReflectionUtility.GetCustomAttributes(method, typeof(DummyAAttribute), true);

        Verify(attributes is not null);
        Verify(attributes.Count == 2);

        string[] expectedAttributes = ["DummyA : a", "DummyA : ba"];
        Verify(expectedAttributes.SequenceEqual(GetAttributeValuePairs(attributes)));
    }

    internal static List<string> GetAttributeValuePairs(IEnumerable attributes)
    {
        var attribValuePairs = new List<string>();
        foreach (object? attrib in attributes)
        {
            if (attrib is DummySingleAAttribute dummySingleAAttribute)
            {
                attribValuePairs.Add("DummySingleA : " + dummySingleAAttribute.Value);
            }
            else if (attrib is DummyAAttribute dummyAAttribute)
            {
                attribValuePairs.Add("DummyA : " + dummyAAttribute.Value);
            }
        }

        return attribValuePairs;
    }

    [DummyA("ba")]
    public class DummyBaseTestClass
    {
        [DummyA("base")]
        [DummySingleA("base")]
        public virtual void DummyVTestMethod1()
        {
        }

        public void DummyBTestMethod2()
        {
        }
    }

    [DummyA("a")]
    public class DummyTestClass : DummyBaseTestClass
    {
        [DummyA("derived")]
        [DummySingleA("derived")]
        public override void DummyVTestMethod1()
        {
        }

        [DummySingleA("derived")]
        public void DummyTestMethod2()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class DummyAAttribute : Attribute
    {
        public DummyAAttribute(string foo) => Value = foo;

        public string Value { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class DummySingleAAttribute : Attribute
    {
        public DummySingleAAttribute(string foo) => Value = foo;

        public string Value { get; set; }
    }
}
