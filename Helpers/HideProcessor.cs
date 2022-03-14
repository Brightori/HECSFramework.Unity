#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HECSFramework.Core;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Helpers
{
    public class HideProcessor<T> : OdinAttributeProcessor<T> where T : IComponent
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            base.ProcessChildMemberAttributes(parentProperty, member, attributes);
            var custom = member.GetCustomAttributes<HideInInspectorCrossPlatform>();

            if (custom != null && custom.Count() > 0)
            {
                attributes.Add(new HideInInspector());
            }
        }
    }
}
#endif