using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using System.Collections.Generic;

namespace Components
{
    public abstract partial class AsyncBaseActionsHolderComponent : BaseComponent
    {
        public AsyncActionBPToIdentifier[] ActionBPToIdentifiers;

        public override void Init()
        {
            foreach (var actionBP in ActionBPToIdentifiers)
            {
                var actions = new List<IAsyncAction>(4);
                foreach (var a in actionBP.ActionBluePrints)
                {
                    actions.Add(a.GetAction());
                }

                Actions.Add(new AsyncActionsToIdentifier { ID = actionBP.ActionIdentifier.Id, Actions = actions });
            }
        }
    }

    [Serializable]
    public struct AsyncActionBPToIdentifier
    {
        public ActionIdentifier ActionIdentifier;
        public AsyncActionBluePrint[] ActionBluePrints;
    }
}
