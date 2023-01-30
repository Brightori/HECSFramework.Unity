using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Components;
using HECSFramework.Core;
using NUnit.Framework;
using Systems;
using UnityEngine.TestTools;

public class TestsHecs
{
    // A Test behaves as an ordinary method
    [Test]
    public void CreateEntity()
    {
        EntityManager.RecreateInstance();

        var entity = new Entity();
        entity.AddComponent(new ActorContainerID { ID = "test" });
        entity.Init();

        Assert.IsTrue(entity.GetComponent<ActorContainerID>().ID == "test" && entity.IsAlive && entity.IsInited && entity.IsDirty);
    }

    [Test]
    public void TestFilters()
    {
        EntityManager.RecreateInstance();
        var filter = EntityManager.Default.GetFilter(Filter.Get<ActorContainerID>());

        for (int i = 0; i < 512; i++)
        {
            var entity = EntityManager.Default.GetEntityFromPool();
            entity.AddComponent<ActorContainerID>();
            entity.Init();
        }

        filter.ForceUpdateFilter();

        for (int i = 0; i < 256; i++)
        {
            EntityManager.Default.Entities[filter.Entities[i]].AddComponent<ActorProviderComponent>();
        }

        for (int i = 0; i < 128; i++)
        {
            EntityManager.Default.Entities[filter.Entities[i]].AddComponent<ViewReadyTagComponent>();
        }

        var poolFilter = EntityManager.Default.GetFilter(Filter.Get<ActorContainerID>());
        var filter2 = EntityManager.Default.GetFilter(Filter.Get<ActorContainerID, ActorProviderComponent>());
        var filter3 = EntityManager.Default.GetFilter(Filter.Get<ActorContainerID, ActorProviderComponent>(), Filter.Get<ViewReadyTagComponent>());

        filter2.ForceUpdateFilter();
        filter3.ForceUpdateFilter();

        Assert.IsTrue(filter.Count == 512 && poolFilter.Count == 512 && filter2.Count == 256 && filter3.Count == 128);
    }

    [Test]
    public void RemoveEntityTest()
    {
        EntityManager.RecreateInstance();
        var list = new List<Entity>(128);

        for (int i = 0; i < 128; i++)
        {
            var newEntity = EntityManager.Default.GetEntityFromPool("progress");
            list.Add(newEntity);
            newEntity.AddComponent(new ActorContainerID());
            newEntity.AddHecsSystem(new Systems.CountersHolderSystem());
            newEntity.Init();
        }

        foreach (var e in list)
            e.HecsDestroy();

        Assert.IsTrue(list.All(x => x.IsAlive == false && x.Components.Count == 0 && x.Systems.Count == 0));
    }

    [Test]
    public void TestReactiveComponents()
    {
        EntityManager.RecreateInstance();
        var check = new Entity("Test");
        check.AddHecsSystem(new StressTestReactsSystem());
        check.Init();
        check.AddComponent(new TestReactComponent());

        EntityManager.Default.GlobalUpdateSystem.Update();
        EntityManager.Default.GlobalUpdateSystem.FinishUpdate?.Invoke();

        var system = check.GetSystem<StressTestReactsSystem>();

        Assert.IsTrue(
               system.GenericGlobalAdd
            && system.GenericGlobalRemove
            && system.GenericLocalAdd
            && system.GenericLocalRemove
            && system.ReactGlobalAdd
            && system.ReactGlobalRemove
            && system.ReactComponentLocalAdd
            && system.ReactComponentLocalRemove);
    }
}