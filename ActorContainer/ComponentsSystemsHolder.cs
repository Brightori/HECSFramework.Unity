using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;
using HECSFramework.Core;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HECSFramework.Unity
{
    [Serializable, HideLabel]
    public class ComponentsSystemsHolder
    {
#if DeveloperMode
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        [ListDrawerSettings(HideAddButton = true, ElementColor = nameof(GetColor), CustomRemoveElementFunction = nameof(RemoveComponent), NumberOfItemsPerPage = 99)]
#elif ModifyMode
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ListDrawerSettings(HideAddButton = true, ElementColor = nameof(GetColor), CustomRemoveElementFunction = nameof(RemoveComponent), NumberOfItemsPerPage = 99)]
#else
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ListDrawerSettings(HideAddButton = true, ElementColor = nameof(GetColor), HideRemoveButton = true, DraggableItems = false, NumberOfItemsPerPage = 99)]
#endif
        [Searchable, HideIf("@this.components.Count == 0")]
        public List<ComponentBluePrint> components = new List<ComponentBluePrint>();

#if DeveloperMode
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        [ListDrawerSettings(HideAddButton = true, CustomRemoveElementFunction = nameof(RemoveSystem), NumberOfItemsPerPage = 99)]
#elif ModifyMode
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ListDrawerSettings(HideAddButton = true, CustomRemoveElementFunction = nameof(RemoveSystem), ShowFoldout = false, NumberOfItemsPerPage = 99)]
#else
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, ShowFoldout = false, NumberOfItemsPerPage = 99)]
#endif
        [Searchable, HideIf("@this.systems.Count == 0")]
        public List<SystemBaseBluePrint> systems = new List<SystemBaseBluePrint>();

        public Color GetColor(int index, Color defaultColor, List<ComponentBluePrint> list)
        {
#if UNITY_EDITOR
            var baseColor = EditorGUIUtility.isProSkin
                ? new Color32(56, 56, 56, 255)
                : new Color32(194, 194, 194, 255);
#else
	        var baseColor =  new Color();
#endif

            if (!list[index].IsColorNeeded) return defaultColor;
            if (list[index].IsOverride) return baseColor + new Color(.0375f, .0375f, 0);
            return baseColor + new Color(0, .0375f, 0);
        }

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

        public void RemoveComponentBluePrint<T> (T component) where T: IComponent
        {
            var typeID = TypesMap.GetComponentInfo<T>().ComponentsMask.Index;

            foreach (var componentBlueprint in components)
            {
                if (componentBlueprint.GetHECSComponent.GetTypeHashCode == typeID)
                {
                    RemoveComponent(componentBlueprint);
                    break;
                }
            }
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

                if (go == null) continue;

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

            AssetDatabase.SaveAssets();
        }

#if DeveloperMode
        [Button]
#endif
        private void ClearBrokenSubAssets()
        {
            if (Parent == null) return;

            //Create a new instance of the object to delete
            ScriptableObject newInstance = ScriptableObject.CreateInstance(Parent.GetType().ToString());

            //Copy the original content to the new instance
            EditorUtility.CopySerialized(Parent, newInstance);
            newInstance.name = Parent.name;

            string toDeletePath = AssetDatabase.GetAssetPath(Parent);
            string clonePath = toDeletePath.Replace(".asset", "CLONE.asset");

            //Create the new asset on the project files
            AssetDatabase.CreateAsset(newInstance, clonePath);
            AssetDatabase.ImportAsset(clonePath);

            //Unhide sub-assets
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(toDeletePath);
            HideFlags[] flags = new HideFlags[subAssets.Length];
            for (int i = 0; i < subAssets.Length; i++)
            {
                //Ignore the "corrupt" one
                if (subAssets[i] == null) continue;

                //Store the previous hide flag
                flags[i] = subAssets[i].hideFlags;
                subAssets[i].hideFlags = HideFlags.None;
                EditorUtility.SetDirty(subAssets[i]);
            }

            EditorUtility.SetDirty(Parent);
            AssetDatabase.SaveAssets();

            //Reparent the subAssets to the new instance
            foreach (var subAsset in AssetDatabase.LoadAllAssetRepresentationsAtPath(toDeletePath))
            {
                //Ignore the "corrupt" one
                if (subAsset == null) continue;

                //We need to remove the parent before setting a new one
                AssetDatabase.RemoveObjectFromAsset(subAsset);
                AssetDatabase.AddObjectToAsset(subAsset, newInstance);
            }

            //Import both assets back to unity
            AssetDatabase.ImportAsset(toDeletePath);
            AssetDatabase.ImportAsset(clonePath);

            //Reset sub-asset flags
            for (int i = 0; i < subAssets.Length; i++)
            {
                //Ignore the "corrupt" one
                if (subAssets[i] == null) continue;

                subAssets[i].hideFlags = flags[i];
                EditorUtility.SetDirty(subAssets[i]);
            }

            EditorUtility.SetDirty(newInstance);
            AssetDatabase.SaveAssets();

            //Here's the magic. First, we need the system path of the assets
            string globalToDeletePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), toDeletePath);
            string globalClonePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), clonePath);

            //We need to delete the original file (the one with the missing script asset)
            //Rename the clone to the original file and finally
            //Delete the meta file from the clone since it no longer exists

            System.IO.File.Delete(globalToDeletePath);
            System.IO.File.Delete(globalClonePath + ".meta");
            System.IO.File.Move(globalClonePath, globalToDeletePath);

            AssetDatabase.Refresh();
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
            if (Application.isPlaying || EntityManager.IsAlive)
                return;

            Parent = parent;
            FixNullContainers();
            ClearDeletedBluePrints();
            //CheckRequirements();
        }

        public void SortComponents()
        {
            components = components.OrderBy(x => x.name).ToList();
            systems = systems.OrderBy(x => x.name).ToList();
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