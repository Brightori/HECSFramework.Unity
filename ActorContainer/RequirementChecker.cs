using HECSFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HECSFramework.Unity
{
   

    /// <summary>
    /// ��� ��� ����������� ������, ��� �� ����������� ��� � ����� ���������� ��� �������� � ���� ������, 
    /// ����� ��� ����������
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredOnOwnerAttribute : Attribute { }


    

    [AttributeUsage(AttributeTargets.Class)]
    public class RequiredPredicates : Attribute
    {
        public readonly Type[] neededTypes;

        public RequiredPredicates(params Type[] neededTypes)
        {
            this.neededTypes = neededTypes;
        }
    }

    public static class RequirementChecker
    {
        public static void CheckRequirements(EntityContainer actorContainer)
        {
#if UNITY_EDITOR
            var componentType = typeof(IComponent);
            var systemType = typeof(ISystem);
            string bluePrintName = string.Empty;
            string type = string.Empty;

            var requiredAtContainer = RequiredAtContainerBPs(actorContainer);
            ProcessBPList(requiredAtContainer, actorContainer);

            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        /// <summary>
        /// ��� ��� �������� ������� ����������� ������ � ��������� ��� ����� �� ������
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="assetID"></param>
        public static void CheckRequiredOnOwnerComponent(List<IAbility> ability, string assetID)
        {
#if UNITY_EDITOR
            var list = new List<Type>(8);
            var newNeededTypes = new List<Type>(8);
            var MembersInfo = new List<MemberInfo>(128);
            var find = UnityEditor.AssetDatabase.FindAssets(assetID);
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(find.FirstOrDefault());

            var actorContainer = UnityEditor.AssetDatabase.LoadAssetAtPath<ActorContainer>(path);
            ability = ability.Distinct().ToList();

            foreach (var a in ability)
            {
                var abilityType = a.GetType();

                var members = abilityType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                MembersInfo.AddRange(members);

                if (ability.GetType().BaseType != null)
                {
                    var members2 = abilityType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    MembersInfo.AddRange(members2);
                }

                if (abilityType.BaseType.BaseType != null)
                {
                    var members3 = abilityType.BaseType.BaseType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    MembersInfo.AddRange(members3);
                }
            }

            foreach (var m in MembersInfo)
            {
                var attr = m.GetCustomAttribute<RequiredOnOwnerAttribute>();

                if (attr != null)
                {
                    var neededType = GetMemberUnderlyingType(m);
                    list.Add(neededType);
                }
            }

            list = list.Distinct().ToList();

            foreach (var ownerReqType in list)
            {
                var dirty = false;

                if (ownerReqType.GetInterfaces().Contains(typeof(IComponent)))
                {
                    foreach (var c in actorContainer.Components)
                    {
                        if (ownerReqType.IsAssignableFrom(c.GetHECSComponent.GetType()))
                        {
                            dirty = true;
                        }
                    }

                    if (dirty)
                        continue;

                    newNeededTypes.Add(ownerReqType);
                }
            }

            foreach (var ownerReqType in list)
            {
                var dirty = false;

                if (ownerReqType.GetInterfaces().Contains(typeof(ISystem)))
                {
                    foreach (var c in actorContainer.Systems)
                    {
                        if (ownerReqType.IsAssignableFrom(c.GetSystem.GetType()))
                        {
                            dirty = true;
                        }
                    }

                    if (dirty)
                        continue;

                    newNeededTypes.Add(ownerReqType);
                }
            }

            if (newNeededTypes.Count > 0)
            {
                FindAndAddBluePrints(newNeededTypes, actorContainer);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
        }

        private static void ProcessBPList(List<Type> bps, EntityContainer actorContainer)
        {
            foreach (var bp in bps)
            {
                if (bp.BaseType.GenericTypeArguments.Any(x => x.GetInterface(typeof(IComponent).Name) != null))
                {
                    if (actorContainer.Components.Any(x => x != null && x.GetType() == bp))
                        continue;

                    AddComponent(actorContainer, bp);
                    continue;
                }

                if (bp.BaseType.GenericTypeArguments.Any(x => x.GetInterface(typeof(ISystem).Name) != null))
                {
                    if (actorContainer.Systems.Any(x => x.GetType() == bp))
                        continue;

                    AddSystem(actorContainer, bp);
                    continue;
                }

                //if (actorContainer is IHavePredicateContainers havePredicateContainers)
                //{
                //    if (bp.BaseType.GenericTypeArguments.Any(x => x.GetInterface(typeof(IPredicate).Name) != null))
                //    {
                //        if (havePredicateContainers.Predicates.Any(x => x.GetType() == bp))
                //            continue;

                //        AddPredicate(actorContainer, bp, havePredicateContainers);
                //        continue;
                //    }
                //}
            }
        }

        private static void FindAndAddBluePrints(List<Type> types, ActorContainer actorContainer)
        {
            var asses = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes());
            var bp = asses.Where(x => x.BaseType != null && x.BaseType.Name.Contains("ComponentBluePrintContainer")).ToList();
            var bp2 = asses.Where(x => x.BaseType != null && x.BaseType.Name.Contains("SystemBluePrint")).ToList();

            foreach (var neededBP in types)
            {
                var neededClass = asses.FirstOrDefault(x => x.GetInterface(neededBP.Name) != null);

                foreach (var componentBP in bp)
                {
                    if (componentBP.BaseType.GenericTypeArguments.Any(x => x == neededClass))
                    {
                        AddComponent(actorContainer, componentBP);
                    }
                }

                foreach (var systemBP in bp2)
                {
                    if (systemBP.BaseType.GenericTypeArguments.Any(x => x == neededClass))
                    {
                        AddSystem(actorContainer, systemBP);
                    }
                }
            }
        }


        private static List<Type> RequiredAtContainerBPs(EntityContainer actorContainer)
        {
            var list = new List<Type>(8);
            var tempTypes = new List<Type>(128);
            var sysTypes = new List<Type>(8);

            if (actorContainer.Systems == null)
                return default;

            foreach (var s in actorContainer.Systems.ToArray())
            {
                if (s == null)
                    continue;

                sysTypes.Add(s.GetSystem.GetType());
            }     
            
            foreach (var c in actorContainer.Components.ToArray())
            {
                if (c == null)
                    continue;

                sysTypes.Add(c.GetHECSComponent.GetType());
            }

            foreach (var s in sysTypes)
            {
                var systemAttrs = s.GetCustomAttributes<RequiredAtContainerAttribute>();

                foreach (var attr in systemAttrs)
                {
                    if (attr != null)
                    {
                        foreach (var neededType in attr.neededTypes)
                        {
                            tempTypes.Add(neededType);
                        }
                    }
                }

                var members = s.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                foreach (var m in members)
                {
                    var required = m.GetCustomAttribute<RequiredAttribute>();

                    if (required != null)
                        tempTypes.Add(m.FieldType);
                }
            }

            var bpProvider = new BluePrintsProvider();

            foreach (var m in tempTypes)
            {
                if (bpProvider.Components.TryGetValue(m, out var bpType))
                {
                    list.Add(bpType);
                    continue;
                }

                if (bpProvider.Systems.TryGetValue(m, out var sysbpType))
                {
                    list.Add(sysbpType);
                    continue;
                }

                foreach (var bpC in bpProvider.Components)
                {
                    if (m.IsAssignableFrom(bpC.Key))
                    {
                        list.Add(bpC.Value);
                        break;
                    }
                }

                foreach (var bpS in bpProvider.Systems)
                {
                    if (m.IsAssignableFrom(bpS.Key))
                    {
                        list.Add(bpS.Value);
                        break;
                    }
                }
            }

            return list;
        }

        private static void AddComponent(EntityContainer container, Type neededType)
        {
#if UNITY_EDITOR
            var asset = ScriptableObject.CreateInstance(neededType);
            var parent = container;
            UnityEditor.AssetDatabase.AddObjectToAsset(asset, parent);

            asset.name = neededType.Name;
            container.AddComponent(asset as ComponentBluePrint);

            Debug.LogWarning($"We added new requirement component {container.name} " + (asset as ComponentBluePrint).GetHECSComponent.GetType());
#endif
        }

        private static void AddSystem(EntityContainer container, Type neededType)
        {
#if UNITY_EDITOR
            var asset = ScriptableObject.CreateInstance(neededType);
            var parent = container;
            UnityEditor.AssetDatabase.AddObjectToAsset(asset, parent);

            asset.name = neededType.Name;
            container.AddSystem(asset as SystemBaseBluePrint);

            Debug.LogWarning($"We added new required system  {container.name}" + (asset as SystemBaseBluePrint).GetSystem.GetType());
#endif
        }

//        private static void AddPredicate(EntityContainer container, Type neededType, IHavePredicateContainers havePredicateContainers)
//        {
//#if UNITY_EDITOR
//            var asset = ScriptableObject.CreateInstance(neededType);
//            var parent = container;
//            UnityEditor.AssetDatabase.AddObjectToAsset(asset, parent);

//            asset.name = neededType.Name;
//            havePredicateContainers.Predicates.Add(asset as PredicateBluePrint);

//            Debug.LogWarning($"�������� ��������� � ��������� {container.name}" + (asset as PredicateBluePrint).GetPredicate.GetType());
//#endif
//        }

        public static Type GetMemberUnderlyingType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }

        public static bool IsAlrdyHaveComponentBluePrint(IEnumerable<Type> asses, Type t)
        {
            var bp = asses.Where(x => x.BaseType != null && x.BaseType.Name.Contains("ComponentBluePrintContainer"));
            return bp.Any(x => x.BaseType.GetGenericArguments().Any(z => z == t));
        }

        public static bool IsAlrdyHaveSystemBluePrint(IEnumerable<Type> asses, Type t)
        {
            var bp2 = asses.Where(x => x.BaseType != null && x.BaseType.Name.Contains("SystemBluePrint"));
            return bp2.Any(x => x.BaseType.GetGenericArguments().Any(z => z == t));
        }

        public static bool IsAlrdyHaveAbilityuBluePrint(IEnumerable<Type> asses, Type t)
        {
            var bp2 = asses.Where(x => x.BaseType != null && x.BaseType.Name.Contains("AbilityContainerBluePrint"));
            return bp2.Any(x => x.BaseType.GetGenericArguments().Any(z => z == t));
        }
    }
}