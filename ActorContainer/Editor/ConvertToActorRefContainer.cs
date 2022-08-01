using HECSFramework.Unity;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class ConvertToActorRefContainer : OdinEditorWindow
{
    [MenuItem("HECS Options/Helpers/Convert ActorContainers to RefContainers")]
    public static void ConvertActorContainersToRefContainers()
    {
        foreach (var o in Selection.objects)
        {
            if (o is ActorContainer actorContainer && o is not ActorReferenceContainer)
            {
                Convert(actorContainer);
            }
        }
    }

    private static void Convert(ActorContainer actorContainer)
    {
        ActorReferenceContainer newInstance = ScriptableObject.CreateInstance<ActorReferenceContainer>();

        //Copy the original content to the new instance
        EditorUtility.CopySerialized(actorContainer, newInstance);
        newInstance.name = actorContainer.name;

        string toDeletePath = AssetDatabase.GetAssetPath(actorContainer);
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

        EditorUtility.SetDirty(actorContainer);
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

        FixNullContainers(newInstance);
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

    private static void FixNullContainers(ActorContainer container)
    {
        var path = AssetDatabase.GetAssetPath(container);
        var allSo = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

        foreach (Object go in allSo)
        {
            if (go is ComponentBluePrint component && !container.Components.Contains(component))
                container.Components.Add(component);

            if (go is SystemBaseBluePrint system && !container.Systems.Contains(system))
                container.Systems.Add(system);
        }
    }
}