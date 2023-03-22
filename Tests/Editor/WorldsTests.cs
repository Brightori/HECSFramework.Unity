using System.Collections.Generic;
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

        world1.Init();
        world2.Init();

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

    [Test]
    public void AddRemove10WorldsTest()
    {
        EntityManager.RecreateInstance();
        List<World> worlds = new List<World>();

        for (int i = 0; i < 30; i++)
        {
            var world = EntityManager.AddWorld();
            var entity  = world.GetEntityFromPool("ttt");
            world.Init();
            entity.AddComponent(new TestComponent());
            entity.AddComponent(new CheckTwoComponent());
            entity.Init();
            worlds.Add(world);
        }

        foreach (var w in worlds)
            EntityManager.RemoveWorld(w);
        var check = worlds[3];

        Assert.IsTrue(worlds.All(x => !x.IsAlive) && EntityManager.Worlds[0] != null && EntityManager.Worlds[check.Index] == null && check.Entities.All(z=> z == null));
    }

    [Test]
    public void DisposeSystemTest()
    {
        EntityManager.RecreateInstance();
        var world = EntityManager.AddWorld();
        world.Init();
        var testEntity = world.GetEntityFromPool("test");
        var testDisposeSystem = new StressTestReactsSystem();
        testEntity.AddHecsSystem(testDisposeSystem);
        testEntity.Init();

        EntityManager.RemoveWorld(world);
        Assert.IsTrue(testDisposeSystem.IsDisposed);
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