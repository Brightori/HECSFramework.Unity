using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;

namespace Systems
{
	[Serializable][Documentation(Doc.Quests, Doc.HECS, "QuestsSystem operates progress of quests and ")]
    public sealed class QuestsSystem : BaseSystem 
    {
        public override void InitSystem()
        {
        }
    }
}