using System;
using System.Collections.Generic;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    public sealed partial class ActionsHolderComponent : BaseActionsHolderComponent, IInitable
    {
    }

    public abstract partial class BaseActionsHolderComponent : BaseComponent, IInitable
    {
        public ActionBPToIdentifier[] ActionBPToIdentifiers;

        public void Init()
        {
            foreach (var actionBP in ActionBPToIdentifiers)
            {
                var actions = new List<IAction>(4);
                foreach (var a in actionBP.ActionBluePrints)
                {
                    actions.Add(a.GetAction());
                }

                Actions.Add(new ActionsToIdentifier { ID = actionBP.ActionIdentifier.Id, Actions = actions });
            }
        }
    }

    [Serializable]
    public struct ActionBPToIdentifier
    {
        public ActionIdentifier ActionIdentifier;
        public ActionBluePrint[] ActionBluePrints;
    }
}