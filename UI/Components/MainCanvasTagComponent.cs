using System;
using System.Collections.Generic;
using Commands;
using HECSFramework.Core;
using HECSFramework.Unity;
using Systems;

namespace Components
{
    [Feature("MainCanvas")]
    [Serializable, RequiredAtContainer(typeof(UnityTransformComponent), typeof(AdditionalCanvasesSystem))]
    [Documentation(Doc.UI, Doc.Tag, "This component marks main canvas for placing ui")]
    public sealed class MainCanvasTagComponent : BaseComponent, IWorldSingleComponent, IHaveActor
    {
        public Actor Actor { get; set; }

        public List<Actor> AdditionalCanvases = new List<Actor>(3);

        public void GatherAdditionalCanvases()
        {
            Owner.World.Command(new CanvasReadyCommand());
            if (Actor.TryGetComponents(out Actor[] uiActors))
            {
                foreach (var actor in uiActors)
                {
                    if (actor.IsAlive())
                        if (actor.Entity.ContainsMask<AdditionalCanvasTagComponent>())
                            AdditionalCanvases.Add(actor);
                }
            }
        }

        public Actor GetCanvas(int canvasID) 
        {
            foreach (var actor in AdditionalCanvases)
            {
                if (actor.TryGetHECSComponent(out AdditionalCanvasTagComponent additionalCanvasTagComponent))
                {
                    if (additionalCanvasTagComponent.AdditionalCanvasIdentifier == canvasID)
                        return actor;

                }
            }

            return null;
        }
    }
}