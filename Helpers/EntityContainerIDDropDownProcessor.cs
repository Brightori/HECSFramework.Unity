#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Helpers
{
    public class EntityContainerIDDropDownProcessor<T> : OdinAttributeProcessor<T>
    {

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            base.ProcessChildMemberAttributes(parentProperty, member, attributes);
            var custom = member.GetCustomAttributes<EntityContainerIDDropDownAttribute>();
            foreach (var attribute in custom)
            {
                attributes.Add(new ValueDropdownAttribute($"@{typeof(EntityContainerIDDropDownProvider).FullName}.{nameof(EntityContainerDropDownProvider.Get)}(\"{attribute.tagComponentName}\")"));
            }
        }
    }

    public static class EntityContainerIDDropDownProvider
    {
        private static SOProvider<EntityContainer> containersProvider = new();

        public static IEnumerable Get(string typeName)
        {
            var list = new ValueDropdownList<int>();
            foreach (var container in containersProvider.GetCollection())
            {
                foreach (var c in container.Components)
                {
                    if (c.GetHECSComponent.GetType().Name == typeName)
                        list.Add(container.name, container.ContainerIndex);
                }
            }
            //if return empty list, check that your identifier does not have namespace 
            return list;
        }
    }
}
#endif