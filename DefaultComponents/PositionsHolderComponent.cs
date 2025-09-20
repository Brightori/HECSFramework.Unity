using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Holder, Doc.HECS, "PositionsHolderComponent contains PositionMonoComponent for associate positions from gameobject")]
    public sealed partial class PositionsHolderComponent : BaseComponent, IHaveActor, IInitAfterView
    {
        public PositionMonoComponent[] PositionsMonoComponents;

        public Actor Actor { get; set; }

        public override void Init()
        {
            if (Owner.ContainsMask<ViewReferenceGameObjectComponent>())
                return;

            UpdatePostions();
        }

        public IHavePosition GetPosition(int id)
        {
            for (int i = 0; i < PositionsMonoComponents.Length; i++)
            {
                if (PositionsMonoComponents[i].PositionIdentifier == id)
                    return PositionsMonoComponents[i];
            }

            return null;
        }

        public void InitAfterView()
        {
            UpdatePostions();
        }

        public void Reset()
        {
            UpdatePostions();
        }

        private void UpdatePostions()
        {
            Actor.TryGetComponents(out PositionsMonoComponents);
        }
    }
}