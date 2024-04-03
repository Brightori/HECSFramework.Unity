#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public class RemoveMissedComponents : EditorWindow
{
    [MenuItem("GameObject/Missing Scripts/Remove", true, 0)]
    private static bool RemoveMissingScripts_OnPrefabs_Validate()
    {
        return Selection.objects != null && Selection.objects.All(x => x.GetType() == typeof(GameObject));
    }

    [MenuItem("GameObject/Missing Scripts/Remove", false, 0)]
    private static void RemoveMissingScripts_OnPrefabs()
    {
        foreach (var obj in Selection.gameObjects)
        {
            RemoveMissingScripts_OnPrefabs_Recursive(obj);
        }
    }

    private static void RemoveMissingScripts_OnPrefabs_Recursive(GameObject obj)
    {
        // list missing on this object
        if (GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj) != 0)
        {
            Debug.Log($"REMOVED: Missing Scripts on object '{obj.name}'");
        }

        // scan childeren
        foreach (Transform transform in obj.transform)
        {
            RemoveMissingScripts_OnPrefabs_Recursive(transform.gameObject);
        }
    }

    [MenuItem("GameObject/Missing Scripts/List", true, 0)]
    private static bool ListMissingScripts_OnPrefabs_Validate()
    {
        return Selection.objects != null && Selection.objects.All(x => x.GetType() == typeof(GameObject));
    }

    [MenuItem("GameObject/Missing Scripts/List", false, 0)]
    private static void ListMissingScripts_OnPrefabs()
    {
        foreach (var obj in Selection.gameObjects)
        {
            ListMissingScripts_OnPrefabs_Recursive(obj);
        }
    }

    private static void ListMissingScripts_OnPrefabs_Recursive(GameObject obj)
    {
        // list missing on this object
        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj) != 0)
        {
            Debug.Log($"Missing Script on object '{obj.name}'");
        }

        // scan childeren
        foreach (Transform transform in obj.transform)
        {
            ListMissingScripts_OnPrefabs_Recursive(transform.gameObject);
        }
    }
}
#endif