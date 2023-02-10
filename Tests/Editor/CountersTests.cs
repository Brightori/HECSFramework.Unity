using Components;
using HECSFramework.Core;
using NUnit.Framework;
using Systems;

internal class CountersTests
{
    [Test]
    public void TestAddComponentCounter()
    {
        EntityManager.RecreateInstance();
        var entity = new Entity();
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
        var entity = new Entity();
        entity.AddComponent(new CountersHolderComponent());
        entity.AddHecsSystem(new CountersHolderSystem());
        entity.Init();
        entity.AddComponent<TestComponent>();
        EntityManager.Default.GlobalUpdateSystem.Update();
        entity.RemoveComponent<TestComponent>();
        var counter = entity.GetComponent<CountersHolderComponent>().GetCounter<ICounter<float>>(10);
        Assert.IsTrue(counter == null);
    }
}
