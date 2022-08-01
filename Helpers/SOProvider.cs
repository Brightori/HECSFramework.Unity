using System.Collections.Generic;
using System.Linq;
using HECSFramework.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HECSFramework.Unity.Helpers
{
    [Documentation(Doc.HECS, Doc.Helpers, "this helper gather all SO of needed type and return IEnumerable, its useful for drop down menus")]
    public class SOProvider<T> where T: ScriptableObject
    {
        public IEnumerable<T> GetNeeded()
        {
#if UNITY_EDITOR
            var containers = AssetDatabase.FindAssets($"t: {typeof(T).Name}")
            .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
            .Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<T>(x)).ToList();

            return containers;
#endif
        }
    }
}