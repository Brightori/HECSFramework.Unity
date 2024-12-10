using Commands;
using HECSFramework.Core;
using NUnit.Framework;
using Systems;

public class RequestsTest
{
    [Test]
    public void RequestWithData()
    {
        EntityManager.RecreateInstance();

        var entity = Entity.Get("Check");
        var sys = new StressTestReactsSystem();
        entity.AddHecsSystem(sys);
        entity.Init();

        var request = EntityManager.Default.Request<StressTestReactsSystem, StressTestGlobalCommand>(new StressTestGlobalCommand());

        Assert.IsTrue(request != null);
    }

    [Test]
    public void RequestSimple()
    {
        EntityManager.RecreateInstance();

        var entity = Entity.Get("Check");
        var sys = new StressTestReactsSystem();
        entity.AddHecsSystem(sys);
        entity.Init();

        var request = EntityManager.Default.Request<StressTestGlobalCommand>();

        Assert.IsTrue(request.Param == true);
    }


    [Test]
    public void RemoveRequestWithData()
    {
        EntityManager.RecreateInstance();

        var entity = Entity.Get("Check");
        var sys = new StressTestReactsSystem();
        entity.AddHecsSystem(sys);
        entity.Init();

        var remove = EntityManager.Default.RemoveRequestProvider<StressTestReactsSystem, StressTestGlobalCommand>(sys);

        Assert.IsTrue(remove == true);
    }
}
