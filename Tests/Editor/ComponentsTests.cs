using HECSFramework.Core;
using NUnit.Framework;

internal class ComponentsTests
{
    [Test]
    public void InitComponent()
    {
        EntityManager.RecreateInstance();

        var entity = Entity.Get("Check");
        var test = entity.AddComponent(new Components.TestComponent());
        entity.Init();


        Assert.IsTrue(test.InitCount == 2);
    }
}
