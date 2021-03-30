using System.Collections.Generic;
using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "ActorPresetContainer")]
    public class PresetContainer : ActorContainer
    {
        public List<ComponentBluePrint> ComponentsBluePrints => holder.components;
        public List<SystemBaseBluePrint> SystemsBluePrints => holder.systems;
    }
}