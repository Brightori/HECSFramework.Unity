using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;
using HECSFramework.Core;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HECSFramework.Unity
{
    [Serializable, HideLabel]
    public class ComponentsSystemsHolder
    {
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        [ListDrawerSettings(HideAddButton = true, DraggableItems = false, ShowFoldout = false, ElementColor = nameof(GetColor), CustomRemoveElementFunction = nameof(RemoveComponent), NumberOfItemsPerPage = 99)]
        [Searchable, HideIf("@this.components.Count == 0")]
        public List<ComponentBluePrint> components = new List<ComponentBluePrint>();

        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        [ListDrawerSettings(HideAddButton = true, DraggableItems = false, ShowFoldout = false, CustomRemoveElementFunction = nameof(RemoveSystem), NumberOfItemsPerPage = 99)]
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

        public void RemoveComponentBluePrint<T>(T component) where T : IComponent
        {
            var typeID = TypesMap.GetComponentInfo<T>().ComponentsMask.TypeHashCode;

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
        [Button]
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

        [Button]
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

        [Button]
        private void CheckRequirements()
            => RequirementChecker.CheckRequirements(Parent);

        [Button]
        private void FixNullContainers()
        {
            components.Clear();
            systems.Clear();

            var path = AssetDatabase.GetAssetPath(Parent);
            var allSo = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            foreach (Object go in allSo)
            {
                if (go is ComponentBluePrint component && !components.Contains(component))
                    components.Add(component);

                if (go is SystemBaseBluePrint system && !systems.Contains(system))
                    systems.Add(system);
            }

            var names = GetSubAssetNamesFromYaml(path);

            var bluePrintProvider = new BluePrintsProvider();

            foreach (var name in names)
            {
                foreach (var cb in bluePrintProvider.Components)
                {
                    if (cb.Value.Name.Contains(name))
                    {
                        if (components.Any(x => x!= null && x.GetType().Name.Contains(name)))
                            continue;

                        Parent.AddComponent(cb.Value);
                        continue;
                    }
                }

                foreach (var cb in bluePrintProvider.Systems)
                {
                    if (cb.Value.Name.Contains(name))
                    {
                        if (systems.Any(x => x != null && x.GetType().Name.Contains(name)))
                            continue;

                        Parent.AddSystem(cb.Value);
                        continue;
                    }
                }


                Debug.LogWarning("we dont have blueprint for type " + name);
            }
        }

        private string[] GetSubAssetNamesFromYaml(string assetPath)
        {
            string fullPath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
            if (!File.Exists(fullPath))
            {
                Debug.LogError("Файл не найден: " + fullPath);
                return new string[0];
            }

            List<string> subAssetNames = new List<string>();
            string[] lines = File.ReadAllLines(fullPath);
            Regex nameRegex = new Regex(@"^\s*m_Name:\s*(.+)$");

            foreach (string line in lines)
            {
                Match match = nameRegex.Match(line);
                if (match.Success)
                {
                    string name = match.Groups[1].Value.Trim();
                    subAssetNames.Add(name);
                }
            }

            return subAssetNames.ToArray();
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