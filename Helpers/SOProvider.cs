using System.Collections.Generic;
using System.Linq;
using HECSFramework.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#pragma warning disable
namespace HECSFramework.Unity.Helpers
{
    [Documentation(Doc.HECS, Doc.Helpers, "this helper gather all SO of needed type and return IEnumerable, its useful for drop down menus")]
    public class SOProvider<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// this is editor only code,  dont run it runtime
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetCollection()
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
Debug.LogError("dont use soProvider runtime");
#endif

#if UNITY_EDITOR
            var containers = AssetDatabase.FindAssets($"t: {typeof(T).Name}")
            .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
            .Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<T>(x)).ToList();

            return containers;
#endif

            return default;
        }
    }
}