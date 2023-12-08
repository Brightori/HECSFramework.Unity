using System;
using Components;
using HECSFramework.Core;
using NUnit.Framework;
using Systems;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

internal class CountersTests
{
    [Test]
    public void TestAddComponentCounter()
    {
        EntityManager.RecreateInstance();
        var entity = Entity.Get("Test");
        entity.AddComponent(new CountersHolderComponent());
        entity.AddHecsSystem(new CountersHolderSystem());
        entity.Init();
        entity.AddComponent(new TestComponent());

        EntityManager.Default.GlobalUpdateSystem.Update();

        var counter = entity.GetComponent<CountersHolderComponent>().GetCounter<ICounter<float>>(10);

        Assert.IsTrue(counter != null && counter.Value == 11);
    }

    [Test]
    public void TestRemoveComponentCounter()
    {
        EntityManager.RecreateInstance();
        var entity = Entity.Get("Test");
        entity.AddComponent(new CountersHolderComponent());
        entity.AddHecsSystem(new CountersHolderSystem());
        entity.Init();
        entity.AddComponent<TestComponent>();
        EntityManager.Default.GlobalUpdateSystem.Update();
        entity.RemoveComponent<TestComponent>();
        var counter = entity.GetComponent<CountersHolderComponent>().GetCounter<ICounter<float>>(10);
        Assert.IsTrue(counter == null);
    }

    [Test]
    public void TestAddModifier()
    {
        var check = new ModifiersFloatContainer();

        check.SetBaseValue(2);

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Add,
            GetValue = 2,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Add,
            GetValue = 2,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        Assert.IsTrue(check.GetCalculatedValue() == 6);
    }

    [Test]
    public void TestAddIntModifier()
    {
        var check = new ModifiersIntContainer();

        check.SetBaseValue(10);

        check.AddModifier(Guid.NewGuid(), new DefaultIntModifier
        {
            GetCalculationType = ModifierCalculationType.Add,
            GetValue = 2,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultIntModifier
        {
            GetCalculationType = ModifierCalculationType.Add,
            GetValue = 2,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        Assert.IsTrue(check.GetCalculatedValue() == 14);
    }

    [Test]
    public void TestSubtractModifier()
    {
        var check = new ModifiersFloatContainer();

        check.SetBaseValue(2);

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Subtract,
            GetValue = 2,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Subtract,
            GetValue = 2,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        Assert.IsTrue(check.GetCalculatedValue() == -2);
    }

    [Test]
    public void TestMultiplyModifier()
    {
        var check = new ModifiersFloatContainer();

        check.SetBaseValue(2);

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Multiply,
            GetValue = 4,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Multiply,
            GetValue = 2,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        Assert.IsTrue(check.GetCalculatedValue() == 16);
    }

    [Test]
    public void TestDivideModifier()
    {
        var check = new ModifiersFloatContainer();

        check.SetBaseValue(16);

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Divide,
            GetValue = 2,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Divide,
            GetValue = 2,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        Assert.IsTrue(check.GetCalculatedValue() == 4);
    }

    [Test]
    public void TestAllLevelsOfModifiers()
    {
        var check = new ModifiersFloatContainer();

        check.SetBaseValue(3);

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Add,
            GetValue = 3,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Add,
            GetValue = 6,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Subtract,
            GetValue = 3,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Subtract,
            GetValue = 3,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Multiply,
            GetValue = 3,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Multiply,
            GetValue = 0.5f,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Divide,
            GetValue = 0.5f,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Divide,
            GetValue = 2f,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        Assert.IsTrue(check.GetCalculatedValue() == 9);
    }


    [Test]
    public void TestAddSubtractModifier()
    {
        var check = new ModifiersFloatContainer();

        check.SetBaseValue(2);

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Add,
            GetValue = 5,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Subtract,
            GetValue = 2,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        check.AddModifier(Guid.NewGuid(), new DefaultFloatModifier
        {
            GetCalculationType = ModifierCalculationType.Subtract,
            GetValue = 3,
            GetModifierType = ModifierValueType.Value,
            ID = 1,
            ModifierGuid = Guid.NewGuid(),
        });

        Assert.IsTrue(check.GetCalculatedValue() == 0);
    }
}
