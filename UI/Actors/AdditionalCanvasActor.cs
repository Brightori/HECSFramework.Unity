using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

public class AdditionalCanvasActor : Actor
{
    public AdditionalCanvasIdentifier AdditionalCanvasIdentifier;

    protected override void Start()
    {
        InitActorWithoutEntity();
        Entity.AddComponent(new UnityTransformComponent());
        Entity.AddComponent(new Components.AdditionalCanvasTagComponent() { AdditionalCanvasIdentifier = AdditionalCanvasIdentifier});
        Entity.Init();
    }
}
