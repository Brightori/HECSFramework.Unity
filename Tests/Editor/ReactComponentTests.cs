using Components;
using HECSFramework.Core;
using NUnit.Framework;
using Systems;

public class ReactComponentTests
{
    [Test]
    public void ReactGenericComponentGlobal()
    {
        EntityManager.RecreateInstance();
        var check = Entity.Get("Test");
        check.AddHecsSystem(new StressTestReactsSystem());
        check.Init();
        check.AddComponent(new TestReactComponent());

        EntityManager.Default.GlobalUpdateSystem.Update();
        EntityManager.Default.GlobalUpdateSystem.FinishUpdate?.Invoke();

        check.RemoveComponent<TestReactComponent>();
        var system = check.GetSystem<StressTestReactsSystem>();

        Assert.IsTrue(system.GenericGlobalAdd && system.GenericGlobalRemove);
    }

    [Test]
    public void ReactGenericComponentLocal()
    {
        EntityManager.RecreateInstance();
        var check = Entity.Get("Test");
        check.AddHecsSystem(new StressTestReactsSystem());
        check.Init();
        check.AddComponent(new TestReactComponent());

        EntityManager.Default.GlobalUpdateSystem.Update();
        EntityManager.Default.GlobalUpdateSystem.FinishUpdate?.Invoke();

        check.RemoveComponent<TestReactComponent>();
        var system = check.GetSystem<StressTestReactsSystem>();

        Assert.IsTrue(system.GenericLocalAdd && system.GenericLocalRemove);
    }

    [Test]
    public void ReactComponentGlobal()
    {
        EntityManager.RecreateInstance();
        var check = Entity.Get("Test");
        var check2 = Entity.Get("Test");

        check.AddHecsSystem(new StressTestReactsSystem());
        check.AddComponent(new TestReactComponent());
        check.Init();
        
        check2.Init();
        check2.AddHecsSystem(new StressTestReactsSystem());

        check2.AddComponent(new TestReactComponent());

        EntityManager.Default.GlobalUpdateSystem.Update();
        EntityManager.Default.GlobalUpdateSystem.FinishUpdate?.Invoke();

        check.RemoveComponent<TestReactComponent>();
        check2.RemoveComponent<TestReactComponent>();
        
        var system = check.GetSystem<StressTestReactsSystem>();
        var system2 = check2.GetSystem<StressTestReactsSystem>();

        Assert.IsTrue(system.ReactGlobalAdd && system.ReactGlobalRemove && system2.ReactGlobalAdd && system2.ReactGlobalRemove);
    }

    [Test]
    public void ReactRemoveComponentAndCheckFilter()
    {
        EntityManager.RecreateInstance();
        var check = Entity.Get("Test");
        var filter = check.World.GetFilter<TestReactComponent>();

        check.AddHecsSystem(new StressTestReactsSystem());
        check.AddComponent(new TestReactComponent());
        check.Init();


        EntityManager.Default.GlobalUpdateSystem.Update();
        EntityManager.Default.GlobalUpdateSystem.FinishUpdate?.Invoke();

        check.RemoveComponent<TestReactComponent>();
        var system = check.GetSystem<StressTestReactsSystem>();

        filter.ForceUpdateFilter();

        Assert.IsTrue(system.ReactGlobalAdd && system.ReactGlobalRemove && filter.Count == 0);
    }

    [Test]
    public void ReactComponentGlobalWithRemovingListener()
    {
        EntityManager.RecreateInstance();
        var check = Entity.Get("Test");
        var sys = new StressTestReactsSystem();
        check.AddHecsSystem(sys);
        check.Init();

        check.AddComponent(new TestReactComponent());

        EntityManager.Default.GlobalUpdateSystem.Update();
        EntityManager.Default.GlobalUpdateSystem.FinishUpdate?.Invoke();

        check.RemoveHecsSystem(sys);
        check.RemoveComponent<TestReactComponent>();

        var checkDeletedSys = check.GetSystem<StressTestReactsSystem>();

        Assert.IsTrue(checkDeletedSys == null && sys.ReactGlobalAdd && !sys.ReactGlobalRemove);
    }


    [Test]
    public void ReactComponentLocal()
    {
        EntityManager.RecreateInstance();
        var check = Entity.Get("Test");
        check.AddHecsSystem(new StressTestReactsSystem());
        check.Init();
        check.AddComponent(new TestReactComponent());

        EntityManager.Default.GlobalUpdateSystem.Update();
        EntityManager.Default.GlobalUpdateSystem.FinishUpdate?.Invoke();

        check.RemoveComponent<TestReactComponent>();
        var system = check.GetSystem<StressTestReactsSystem>();

        Assert.IsTrue(system.ReactComponentLocalAdd && system.ReactComponentLocalRemove);
    }

    [Test]
    public void AddComponentBeforeInitEntity()
    {
        EntityManager.RecreateInstance();
        var entity = Entity.Get("Test");
        entity.AddComponent(new TestComponent());
        entity.AddComponent(new TestReactComponent());
        entity.AddComponent(new TestWorldSingleComponent());
        entity.Init();

        EntityManager.Default.TryGetSingleComponent(out TestWorldSingleComponent testReactComponent);
        var boolCheckLocal = entity.TryGetComponent(out TestComponent testComponent);

        Assert.IsTrue(testReactComponent != null &&  boolCheckLocal && testComponent.InitCount == 1);
    }

    [Test]
    public void AddComponentAfterInitEntity()
    {
        EntityManager.RecreateInstance();
        var entity = Entity.Get("Test");
        entity.Init();
        entity.AddComponent(new TestComponent());
        entity.AddComponent(new TestReactComponent());
        entity.AddComponent(new TestWorldSingleComponent());

        EntityManager.Default.TryGetSingleComponent(out TestWorldSingleComponent testReactComponent);
        var boolCheckLocal = entity.TryGetComponent(out TestComponent testComponent);

        Assert.IsTrue(testReactComponent != null && boolCheckLocal && testComponent.InitCount == 1);
    }


    [Test]
    public void AfterInitTest()
    {
        EntityManager.RecreateInstance();
        var entity = Entity.Get("Test");
        entity.AddHecsSystem(new StressTestReactsSystem());
        entity.Init();
        
        if (entity.TryGetComponent(out TestInitComponent testInitComponent))
        {
            if (testInitComponent.Init)
            {
                Assert.Pass();
                return;
            }
        }

        Assert.Fail();
    }
}