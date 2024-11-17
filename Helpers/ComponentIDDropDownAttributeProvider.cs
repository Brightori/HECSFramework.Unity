#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HECSFramework.Core;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Helpers
{
    public class ComponentIDDropDownProcessor<T> : OdinAttributeProcessor<T>
    {

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            base.ProcessChildMemberAttributes(parentProperty, member, attributes);
            var custom = member.GetCustomAttributes<ComponentIDDropDownAttribute>();
            foreach (var attribute in custom)
            {
                attributes.Add(new ValueDropdownAttribute($"@{typeof(ComponentIDDropDownAttributeProvider).FullName}.{nameof(ComponentIDDropDownAttributeProvider.Get)}()"));
            }
        }
    }

    public static class ComponentIDDropDownAttributeProvider
    {
        private static BluePrintsProvider componentsProvider = new();

        public static IEnumerable Get()
        {
            var list = new ValueDropdownList<int>();
            foreach (var container in componentsProvider.Components)
            {
                list.Add(container.Key.Name, IndexGenerator.GetIndexForType(container.Key));
            }
            return list;
        }
    }
}
#endif