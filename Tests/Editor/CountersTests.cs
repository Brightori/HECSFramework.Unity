using Components;
using HECSFramework.Core;
using NUnit.Framework;
using Systems;

internal class CountersTests
{
    [Test]
    public void TestAddCounter()
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
}
