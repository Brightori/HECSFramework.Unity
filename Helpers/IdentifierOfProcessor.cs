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
    public class IdentifierOfProcessor<T> : OdinAttributeProcessor<T>
    {
        
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            base.ProcessChildMemberAttributes(parentProperty, member, attributes);
            var custom = member.GetCustomAttributes<IdentifierDropDownAttribute>();
            foreach (var attribute in custom)
            {
                attributes.Add(new ValueDropdownAttribute($"@{typeof(IdentifiersProvider).FullName}.{nameof(IdentifiersProvider.Get)}(\"{attribute.identifierType}\")"));
            }
        }
    }

    public static class IdentifiersProvider
    {
        private static SOProvider<IdentifierContainer> containersProvider = new();
        
        public static IEnumerable Get(string typeName)
        {
            var type = Type.GetType(typeName);
            var list = new ValueDropdownList<int>();
            foreach (var container in containersProvider.GetCollection())
            {
                if(container.GetType() == type)
                    list.Add(container.name, container.Id);
            }
            //if return empty list, check that your identifier does not have namespace 
            return list;
        }       
    }
}
#endif