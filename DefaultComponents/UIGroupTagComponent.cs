using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using System.Linq;
using UnityEngine;

namespace Components
{
    [Serializable, Documentation(Doc.UI, "The tag component, which we hang on the UI and thus designate belonging to which group, can contain several tags")]
    public class UIGroupTagComponent : BaseComponent
    {
        [SerializeField] private UIGroupIdentifier[] Groups = new UIGroupIdentifier[0];
        public bool IsHaveGroupIndex (int index) => Groups.Any(x => x.Id == index);
    }
}
