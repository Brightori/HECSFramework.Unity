#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using ReadonlyField = Helpers.FindUsagesUtilities.ReadonlyField;

namespace Helpers
{
    public class FindUsagesWindow : OdinEditorWindow
    {
        [ShowInInspector, ProgressBar(0, 100), PropertyOrder(0)]
        private float Loading => (assetsCount.total - assetsCount.remaining) * 100f / assetsCount.total;

        [Space, ReadOnly, ShowInInspector, PropertyOrder(1)]
        private Object asset;

        [Space, ListDrawerSettings(IsReadOnly = true), ShowInInspector, PropertyOrder(2)]
        private List<ReadonlyField> usages = new List<ReadonlyField>();

        private readonly ConcurrentQueue<string> asyncAssetsQueue = new ConcurrentQueue<string>();
        private (int remaining, int total) assetsCount = (0, 1);

        public void Init(string[] paths)
        {
            usages.Clear();
            asset = Selection.objects[0];
            if (asset == null)
            {
                Debug.LogError("Нужно выбрать объект.");
                return;
            }

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)).ToString();
            assetsCount = (paths.Length, paths.Length);

            Task.Run(() => Parallel.ForEach(paths, a =>
            {
                var text = File.ReadAllText(a);
                if (text.Contains(guid))
                    asyncAssetsQueue.Enqueue(a);

                assetsCount.remaining--;
            }));
        }

        private void Update()
        {
            if (asyncAssetsQueue.Count == 0 || !asyncAssetsQueue.TryDequeue(out var queued)) return;

            usages.Add(new ReadonlyField { Value = AssetDatabase.LoadAssetAtPath<Object>(queued) });
        }

        private void OnInspectorUpdate()
            => Repaint();
    }

    public static class FindUsagesUtilities
    {
        private static readonly string[] ForbiddenExtensions = { ".fbx" };
        private static readonly string AssetsFilter = "t:ScriptableObject t:GameObject t:Scene";

        [MenuItem("Tools/Find Usages", priority = 0)]
        public static void FindUsages()
        {
            var assets = AssetDatabase.FindAssets(AssetsFilter)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(a => !IsForbiddenExtension(a))
                .ToArray();
            EditorWindow.GetWindow<FindUsagesWindow>().Init(assets);
        }

        private static bool IsForbiddenExtension(string subject)
        {
            var lower = subject.ToLower();
            foreach (var extension in ForbiddenExtensions)
                if (lower.EndsWith(extension))
                    return true;

            return false;
        }

        [Serializable, HideLabel, InlineProperty]
        public struct ReadonlyField
        {
            [HideLabel, ReadOnly, UsedImplicitly] public Object Value;
        }
    }
}
#endif