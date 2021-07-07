#if UNITY_EDITOR

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity.Editor
{
    public class ShowDependenciesWindow : OdinEditorWindow
    {
        [ShowInInspector]
        public List<Object> Dependencies = new List<Object>();

        protected override void OnEnable()
        {
            base.OnEnable();
            Dependencies.Clear();
        }

        public void Init(EntityContainer entityContainer)
        {
            Dependencies.Clear();

            var container = AssetDatabase.GetAssetPath(entityContainer);
            var path = Application.dataPath;
            var prefabs = AssetDatabase.FindAssets($"t:Prefab");
            var containers = AssetDatabase.FindAssets($"t: {typeof(EntityContainer).Name}");

            foreach (var go in prefabs)
            {
                var pathGo = AssetDatabase.GUIDToAssetPath(go);
                var dependencies = AssetDatabase.GetDependencies(pathGo, false);

                if (dependencies != null && dependencies.Length > 0)
                {
                    if (dependencies.Contains(container))
                    {
                        var needed = AssetDatabase.LoadAssetAtPath<Object>(pathGo);

                        if (needed != null)
                            Dependencies.Add(needed);
                    }
                }
            }

            foreach (var go in containers)
            {
                var pathGo = AssetDatabase.GUIDToAssetPath(go);
                var dependencies = AssetDatabase.GetDependencies(pathGo, true);

                if (dependencies != null && dependencies.Length > 0)
                {
                    if (dependencies.Contains(container))
                    {
                        var needed = AssetDatabase.LoadAssetAtPath<Object>(pathGo);

                        if (needed != null)
                        {
                            if (pathGo != container)
                                Dependencies.Add(needed);
                        }
                    }
                }
            }
        }
    }
}
#endif