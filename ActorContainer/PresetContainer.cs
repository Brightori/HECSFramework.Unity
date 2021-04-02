using System.Collections.Generic;
using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "ActorPresetContainer")]
    public class PresetContainer : EntityContainer
    {
        public List<ComponentBluePrint> ComponentsBluePrints => holder.components;
        public List<SystemBaseBluePrint> SystemsBluePrints => holder.systems;
    }
}