using System;
using Commands;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Action, Doc.HECS, Doc.UI, "this action execute show or hide ui commands")]
    public sealed class ShowHideUIAction : IAction
    {
        public UIIdentifier UIIdentifier;
        public bool IsMultyple;
        public bool Show = true;

        public void Action(Entity entity, Entity target = null)
        {
            if (Show)
                entity.World.Command(new ShowUICommand { UIViewType = UIIdentifier, MultyView = IsMultyple });
            else
                entity.World.Command(new HideUICommand { UIViewType = UIIdentifier });
        }
    }
}