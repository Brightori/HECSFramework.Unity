using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Action, "default actions with predicates holder, for others holders u should inherit BaseActionsWithPredicateHolderComponent")]
    public sealed partial class ActionsWithPredicateHolderComponent : BaseActionsWithPredicateHolderComponent
    {

    }

    [Serializable]
    [Documentation(Doc.HECS, Doc.Action, "this is base component for actions with predicates")]
    public abstract partial class BaseActionsWithPredicateHolderComponent : BaseComponent, IActionsHolderComponent
    {
        [SerializeField]
        public ActionAndPredicateBPToIdentifier[] ActionAndPredicateBPToIdentifiers;

        public void ExecuteAction(int Index, Entity entity = null)
        {
            if (entity == null)
                entity = Owner;

            foreach (var a in ActionAndPredicateBPToIdentifiers)
            {
                if (a.ActionIdentifier.Id == Index)
                {
                    foreach (var actionContainer in a.ActionBluePrints)
                    {
                        foreach (var b in actionContainer.Predicates)
                        {
                            if (!b.GetPredicate.IsReady(entity))
                            {
                                goto nextContainer;
                            }
                        }

                        actionContainer.ActionBluePrint.GetAction().Action(entity);

                    nextContainer:
                        continue;
                    }
                }
            }
        }
    }

    [Serializable]
    public struct ActionAndPredicateBPToIdentifier
    {
        public ActionIdentifier ActionIdentifier;
        public ActionWithPredicateContainer[] ActionBluePrints;
    }

    [Serializable]
    public class ActionWithPredicateContainer
    {
        public List<PredicateBluePrint> Predicates = new List<PredicateBluePrint>();
        public ActionBluePrint ActionBluePrint;
    }
}