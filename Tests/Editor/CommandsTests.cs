using Commands;
using HECSFramework.Core;
using NUnit.Framework;
using Systems;

internal class CommandsTests
{
    [Test]
    public void TestLocalCommand()
    {
        EntityManager.RecreateInstance();

        var entity = Entity.Get("Check");
        var sys = new StressTestReactsSystem();
        entity.AddHecsSystem(sys);
        entity.Init();

        entity.Command(new StressTestLocalCommand { Param = true });
        entity.RemoveHecsSystem<StressTestReactsSystem>();
        entity.Command(new StressTestLocalCommand { Param = false });

        Assert.IsTrue(sys.LocalReact && sys.LocalReactRemoved);
    }


    [Test]
    public void TestGlobalCommand()
    {
        EntityManager.RecreateInstance();

        var entity = Entity.Get("Check");
        var sys = new StressTestReactsSystem();
        entity.AddHecsSystem(sys);
        entity.Init();

        EntityManager.Command(new StressTestGlobalCommand { Param = true });
        entity.RemoveHecsSystem<StressTestReactsSystem>();
        EntityManager.Command(new StressTestGlobalCommand { Param = false });

        Assert.IsTrue(sys.GlobalReact && sys.GlobalReactRemoved);
    }

    [Test]
    public void TestGlobalCommandWithoutListener()
    {
        EntityManager.RecreateInstance();

        var entity = Entity.Get("Check");
        entity.Init();

        EntityManager.Command(new StressTestGlobalCommand { Param = true });
        EntityManager.Command(new StressTestGlobalCommand { Param = true }, 8);
        Assert.Pass();
    }

    [Test]
    public void TestCommandQueue()
    {
        EntityManager.RecreateInstance();

        var entity = Entity.Get("Check");
        var sys = new StressTestReactsSystem();
        entity.AddHecsSystem(sys);
        entity.Init();

        CommandQueueManager<StressTestGlobalCommand>.AddToQueue(new StressTestGlobalCommand { Param = true });
        EntityManager.Default.GlobalUpdateSystem.Update();
        EntityManager.Default.GlobalUpdateSystem.PreFinishUpdate?.Invoke();
        Assert.IsTrue(sys.GlobalReact);
    }

    [Test]
    public void TestCommandExpliciteWorldQueue()
    {
        EntityManager.RecreateInstance();

        var entity = Entity.Get("Check");
        var sys = new StressTestReactsSystem();
        entity.AddHecsSystem(sys);
        entity.Init();

        CommandQueueManager<StressTestGlobalCommand>.AddToQueue(EntityManager.Default, new StressTestGlobalCommand { Param = true });
        EntityManager.Default.GlobalUpdateSystem.Update();
        EntityManager.Default.GlobalUpdateSystem.PreFinishUpdate?.Invoke();
        Assert.IsTrue(sys.GlobalReact);
    }
}
