using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;
using HECSFramework.Unity;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

#endif

namespace HECSFramework.Unity
{
    [Serializable, HideLabel]
    public class ComponentsSystemsHolder
    {
#if DeveloperMode
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        [ListDrawerSettings(HideAddButton = true, CustomRemoveElementFunction = nameof(RemoveComponent))]
#elif ModifyMode
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ListDrawerSettings(HideAddButton = true, CustomRemoveElementFunction = nameof(RemoveComponent))]
#else
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false)]
#endif
        [Searchable, HideIf("@this.components.Count == 0")]
        public List<ComponentBluePrint> components = new List<ComponentBluePrint>();

#if DeveloperMode
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        [ListDrawerSettings(HideAddButton = true, CustomRemoveElementFunction = nameof(RemoveSystem))]
#elif ModifyMode
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ListDrawerSettings(HideAddButton = true, CustomRemoveElementFunction = nameof(RemoveSystem), Expanded = false)]
#else
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, Expanded = false)]
#endif
        [Searchable, HideIf("@this.systems.Count == 0")]
        public List<SystemBaseBluePrint> systems = new List<SystemBaseBluePrint>();

#if UNITY_EDITOR
        [HideInInspector]
#endif
        public EntityContainer Parent;
        
        private void RemoveComponent(ComponentBluePrint element)
        {
            components.Remove(element);
#if UNITY_EDITOR
            ClearDeletedBluePrints();
#endif
        }
        
        private void RemoveSystem(SystemBaseBluePrint element)
        {
            systems.Remove(element);
#if UNITY_EDITOR
            ClearDeletedBluePrints();
#endif
        }
        
#if UNITY_EDITOR
#if DeveloperMode
        [Button]
#endif
        public void ClearDeletedBluePrints()
        {
            if (Parent == null) return;
            
            var path = AssetDatabase.GetAssetPath(Parent);
            var allSo = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            systems.RemoveAll(a => a == null || a.GetSystem == null);
            components.RemoveAll(a => a == null || a.GetHECSComponent == null);

            for (int i = 0; i < allSo.Length; i++)
            {
                Object go = allSo[i];

                if (go is ComponentBluePrint component && !components.Contains(component))
                {
                    AssetDatabase.RemoveObjectFromAsset(go);
                    Object.DestroyImmediate(go);
                }

                if (go is SystemBaseBluePrint system && !systems.Contains(system))
                {
                    AssetDatabase.RemoveObjectFromAsset(go);
                    Object.DestroyImmediate(go);
                }
            }
        }
        
#if DeveloperMode
        [Button]
#endif
        private void CheckRequirements()
            => RequirementChecker.CheckRequirements(Parent);

#if DeveloperMode
        [Button]
#endif
        private void FixNullContainers()
        {
            var path = AssetDatabase.GetAssetPath(Parent);
            var allSo = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            foreach (Object go in allSo)
            {
                if (go is ComponentBluePrint component && !components.Contains(component)) 
                    components.Add(component);

                if (go is SystemBaseBluePrint system && !systems.Contains(system)) 
                    systems.Add(system);
            }
        }

        public void OnValidate(EntityContainer parent)
        {
            Parent = parent;
            FixNullContainers();
            ClearDeletedBluePrints();
            //CheckRequirements();
        }
#endif
    }
    
    public enum HecsEditorMode
    {
        Basic = 0,
        Modify = 1,
        Developer = 2
    }
}