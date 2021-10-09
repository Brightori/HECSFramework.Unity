using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

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
            if (Application.isPlaying)
                return;

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