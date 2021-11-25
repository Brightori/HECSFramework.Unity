using HECSFramework.Core;
using HECSFramework.Documentation;
using HECSFramework.Unity;
using System;
using System.Linq;
using UnityEngine;

namespace Components
{
    [Serializable, Documentation(Doc.UI, "Компонент тег, который мы вешаем на UI и таким образом обозначаем принадлежность как какой группе, может содержать несколько тегов")]
    public class UIGroupTagComponent : BaseComponent
    {
        [SerializeField] private UIGroupIdentifier[] Groups = new UIGroupIdentifier[0];
        public bool IsHaveGroupIndex (int index) => Groups.Any(x => x.Id == index);
    }
}
