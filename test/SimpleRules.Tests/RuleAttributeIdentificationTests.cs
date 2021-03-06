﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleRules.Attributes;
using SimpleRules.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleRules.Tests
{
    [TestClass]
    public class RuleAttributeIdentificationTests
    {
        [TestMethod]
        public void CanIdentifyClassWithRuleAttributes()
        {
            var srcType = typeof(Employee);
            var result = srcType.HasRuleAttributes();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanIdentifyClassWithoutRuleAttributes()
        {
            var srcType = typeof(EmployeeWithExternalMeta);
            var result = srcType.HasRuleAttributes();
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanIdentifyClassWithRuleMetadataAttribute()
        {
            var srcType = typeof(EmployeeWithMetaAttribute);
            var result = srcType.HasRuleMetadataAttribute();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanIdentifyTheAssociatedRuleMetaType()
        {
            var srcType = typeof(EmployeeWithMetaAttribute);
            var result = srcType.FindRuleMetadataType();
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof (EmployeeMetadata), result);
        }

        [TestMethod]
        public void CanIdentifyClassWithoutRuleMetadataAttribute()
        {
            var srcType = typeof(Employee);
            var result = srcType.HasRuleMetadataAttribute();
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public void CanIdentifyRulesUsingAttributes()
        {
            var srcType = typeof(Employee);
            var rules = srcType.GetRules().ToList();
            Assert.AreEqual(7, rules.Count);
            var greaterThanAttrs = rules.Select(r => r.Item1).Where(r => r.Is<GreaterThanAttribute>());
            Assert.AreEqual(3, greaterThanAttrs.Count());
            var greaterThanEqualAttrs = rules.Select(r => r.Item1).Where(r => r.Is<GreaterThanOrEqualAttribute>());
            Assert.AreEqual(1, greaterThanEqualAttrs.Count());
            var notEqualAttrs = rules.Select(r => r.Item1).Where(r => r.Is<NotEqualToAttribute>());
            Assert.AreEqual(3, notEqualAttrs.Count());
        }

        [TestMethod]
        public void CanIdentifyRulesUsingMetadataType()
        {
            var srcType = typeof(EmployeeWithMetaAttribute);
            var metaType = typeof(EmployeeMetadata);
            var rules = srcType.GetRules(metaType).ToList();
            Assert.AreEqual(7, rules.Count);
            var greaterThanAttrs = rules.Select(r => r.Item1).Where(r => r.Is<GreaterThanAttribute>());
            Assert.AreEqual(3, greaterThanAttrs.Count());
            var greaterThanEqualAttrs = rules.Select(r => r.Item1).Where(r => r.Is<GreaterThanOrEqualAttribute>());
            Assert.AreEqual(1, greaterThanEqualAttrs.Count());
            var notEqualAttrs = rules.Select(r => r.Item1).Where(r => r.Is<NotEqualToAttribute>());
            Assert.AreEqual(3, notEqualAttrs.Count());
        }

        [TestMethod]
        public void CanIdentifyMetadataTypeIfPresentFromRegistration()
        {
            var concreteType = typeof (Employee);
            var metaType = typeof (EmployeeMetadata);
            var dictionary = new Dictionary<Type, Type>();
            dictionary.Add(concreteType, metaType);

            var requiredMetadata = dictionary.GetMetadataType(concreteType);
            Assert.IsNotNull(requiredMetadata);
            Assert.AreEqual(metaType, requiredMetadata.Item2);
        }

        [TestMethod]
        public void CanIdentifyMetadataTypeWithMetaAttribute()
        {
            var concreteType = typeof(EmployeeWithMetaAttribute);
            var metaType = typeof(EmployeeMetadata);
            var dictionary = new Dictionary<Type, Type>();

            var requiredMetadata = dictionary.GetMetadataType(concreteType);
            Assert.IsNotNull(requiredMetadata);
            Assert.AreEqual(metaType, requiredMetadata.Item2);
        }

        [TestMethod]
        public void CanIdentifyMetadataTypeInline()
        {
            var concreteType = typeof(Employee);
            var dictionary = new Dictionary<Type, Type>();

            var requiredMetadata = dictionary.GetMetadataType(concreteType);
            Assert.IsNotNull(requiredMetadata);
            Assert.AreEqual(concreteType, requiredMetadata.Item1);
            Assert.AreEqual(concreteType, requiredMetadata.Item2);
        }

        [TestMethod]
        public void CanIdentifyMetadataAsNullIfNotPresent()
        {
            var concreteType = typeof(User);
            var dictionary = new Dictionary<Type, Type>();

            var requiredMetadata = dictionary.GetMetadataType(concreteType);
            Assert.IsNull(requiredMetadata);
        }

        [TestMethod]
        public void CanIdentifyEntityKeyAttributeIfPresent()
        {
            var concreteType = typeof (User);
            var metaType = typeof (UserMetadata);
            var ruleMetaMap = new Tuple<Type, Type>(concreteType, metaType);
            var entityProp = ruleMetaMap.FindEntityKeyPropertyInfo();
            Assert.IsNotNull(entityProp);
            var expectedProp = concreteType.GetProperty("Id");
            Assert.AreEqual(expectedProp.Name, entityProp.Name);
            Assert.AreEqual(expectedProp.PropertyType, entityProp.PropertyType);
            Assert.AreEqual(expectedProp.DeclaringType, entityProp.DeclaringType);
        }

        [TestMethod]
        public void CanIdentifyEntityKeyAttributeIfPresentWithoutMeta()
        {
            var concreteType = typeof(Course);
            var ruleMetaMap = new Tuple<Type, Type>(concreteType, concreteType);
            var entityProp = ruleMetaMap.FindEntityKeyPropertyInfo();
            Assert.IsNotNull(entityProp);
            var expectedProp = concreteType.GetProperty("Id");
            Assert.AreEqual(expectedProp.Name, entityProp.Name);
            Assert.AreEqual(expectedProp.PropertyType, entityProp.PropertyType);
            Assert.AreEqual(expectedProp.DeclaringType, entityProp.DeclaringType);
        }

        [TestMethod]
        public void CanIdentifyNullEntityKeyAttribute()
        {
            var concreteType = typeof(Employee);
            var ruleMetaMap = new Tuple<Type, Type>(concreteType, concreteType);
            var entityProp = ruleMetaMap.FindEntityKeyPropertyInfo();
            Assert.IsNull(entityProp);
        }
    }

    public class Employee
    {
        [GreaterThan("", constantValue: 0)] 
        public int Id { get; set; }
        [NotEqualTo("", RuleType.Error)]
        [NotEqualTo(null, RuleType.Error)]
        public string Name { get; set; }
        [NotEqualTo("", RuleType.Error)]
        public string Ssn { get; set; }
        [GreaterThan("LastPayDate", RuleType.Warning)]
        public DateTime StartDate { get; set; }
        [GreaterThan("StartDate", RuleType.Error, canBeNull: true)]
        public DateTime? TermDate { get; set; }
        [GreaterThanOrEqual("StartDate", RuleType.Warning)]
        public DateTime LastPayDate { get; set; }
    }

    [RuleMetadata(typeof(EmployeeMetadata))]
    public class EmployeeWithMetaAttribute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ssn { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? TermDate { get; set; }
        public DateTime LastPayDate { get; set; }
    }

    public class EmployeeMetadata
    {
        [GreaterThan("", constantValue: 0)]
        public object Id { get; set; }
        [NotEqualTo("", RuleType.Error)]
        [NotEqualTo(null, RuleType.Error)]
        public string Name { get; set; }
        [NotEqualTo("", RuleType.Error)]
        public string Ssn { get; set; }
        [GreaterThan("LastPayDate", RuleType.Warning)]
        public DateTime StartDate { get; set; }
        [GreaterThan("StartDate", RuleType.Error, canBeNull: true)]
        public DateTime? TermDate { get; set; }
        [GreaterThanOrEqual("StartDate", RuleType.Warning)]
        public DateTime LastPayDate { get; set; }
    }

    public class EmployeeWithExternalMeta
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ssn { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? TermDate { get; set; }
        public DateTime LastPayDate { get; set; }
    }

    public static class TestExtensions
    {
        public static bool Is<T>(this BaseRuleAttribute attribute)
            where T: BaseRuleAttribute
        {
            return attribute != null && attribute is T;
        }
    }
}
