using System;
using System.Runtime.CompilerServices;
using HECSFramework.Core;
using HECSFramework.Unity;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Documentation(Doc.Actor, Doc.HECS, Doc.Visual, Doc.Layer, "this component holds all childs gameobjects of current actor")]
    public sealed class ActorChildsProviderComponent : BaseComponent, IInitAfterView, IHaveActor
    {
        [ReadOnlyCrossPlatform]
        public Transform[] GameObjects;

        public Actor Actor { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GatherChildsGameObject()
        {
            GameObjects  = Actor.GetComponentsInChildren<Transform>();
        }

        public override void Init()
        {
            GatherChildsGameObject();
        }

        public void InitAfterView()
        {
            GatherChildsGameObject();
        }

        public void Reset()
        {
            GatherChildsGameObject();
        }
    }
}