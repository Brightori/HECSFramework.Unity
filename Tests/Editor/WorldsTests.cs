using System.Linq;
using Components;
using HECSFramework.Core;
using NUnit.Framework;
using Systems;

public class WorldsTests
{
    [Test]
    public void DisposeWorld()
    {
        EntityManager.RecreateInstance();
        var world1 = EntityManager.AddWorld();
        var world2 = EntityManager.AddWorld();

        for (int i = 0; i < 10; i++)
        {
            GetEntity(world1);
            GetEntity(world2);
        }

        EntityManager.RemoveWorld(world2);
        var components = ComponentProvider<TestComponent>.ComponentsToWorld.Data[2].Components;
        //todo очищать ентити и компоненты, сейчас остаютс€ экземпл€ры в массивах
        Assert.IsTrue((world2 == null || !world2.IsAlive) && components.All(x=> x== null));
    }

    private Entity GetEntity(World world)
    {
        var entity = world.GetEntityFromPool();
        entity.AddComponent(new TestComponent());
        entity.AddHecsSystem(new StressTestReactsSystem());
        entity.Init();

        return entity;
    }
}