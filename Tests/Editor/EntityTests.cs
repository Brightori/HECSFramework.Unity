using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Components;
using HECSFramework.Core;
using NUnit.Framework;
using Systems;
using UnityEditor;

public class EntityTests
{
    [Test]
    public void CreateEntity()
    {
        EntityManager.RecreateInstance();

        var entity = Entity.Get("Test");
        entity.AddComponent(new ActorContainerID { ID = "test" });
        entity.Init();

        Assert.IsTrue(entity.GetComponent<ActorContainerID>().ID == "test" && entity.IsAlive && entity.IsInited && entity.IsDirty);
    }

    [Test]
    public void ReactEntity()
    {
        EntityManager.RecreateInstance();
        var entity = Entity.Get("Test");
        entity.AddHecsSystem(new StressTestReactsSystem());
        entity.Init();

        var entity2 = Entity.Get("Test");
        entity2.Init();
        entity2.HecsDestroy();
        EntityManager.Default.GlobalUpdateSystem.FinishUpdate?.Invoke();

        var sys = entity.GetSystem<StressTestReactsSystem>();
        Assert.IsTrue(sys.EntityAdded && sys.EntityRemoved);
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
    public void TestFiltersDiffWorlds()
    {
        EntityManager.RecreateInstance();
        EntityManager.AddWorld();
        EntityManager.AddWorld();

        foreach (var w in EntityManager.Worlds)
        {
            if (w != null)
            {
                var check = w.GetEntityFromPool("testInput");
                check.AddComponent(new InputListenerTagComponent());
                check.Init();
            }
        }

        var count = 0;

        foreach (var w in EntityManager.Worlds)
            if (w != null)
                w.GlobalUpdateSystem.Update();

        foreach (var w in EntityManager.Worlds)
        {
            if (w != null)
            {
                var f = w.GetFilter<InputListenerTagComponent>();
                if (f.Count > 0)
                {
                    count++;
                }
            }
        }

        Assert.IsTrue(count >= 3);
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
    public void Migration()
    {
        EntityManager.RecreateInstance();
        EntityManager.AddWorld();
        var check = Entity.Get("ReactEntity");
        check.AddHecsSystem(new StressTestReactsSystem());
        check.Init();

        var check2 = Entity.Get("MigrateEntity");
        check2.AddComponent(new TestReactComponent());
        check2.Init();
        var guid = check2.GUID;

        EntityManager.Worlds[1].MigrateEntityToWorld(check2);
        Assert.IsTrue(check2.WorldId == 1 && check.GetSystem<StressTestReactsSystem>().ReactGlobalRemove == true && check2.GUID == guid);
    }
}