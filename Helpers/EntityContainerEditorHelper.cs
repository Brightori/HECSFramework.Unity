using System.Linq;
using HECSFramework.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HECSFramework.Unity.Helpers
{
    public static class EntityContainerEditorHelper
    {
        public static void MarkDirtyContainersWithComponent<T>() where T : IComponent
        {
#if UNITY_EDITOR
            var soProvider = new SOProvider<EntityContainer>();
            var collection = soProvider.GetCollection().ToArray();

            foreach (var a in collection)
            {
                if (a.IsHaveComponent<T>())
                {
                    MarkDirtyAllInContainer(a);
                }
            }
#endif
        }

        public static void MarkDirtyAllInContainer(EntityContainer entityContainer)
        {
#if UNITY_EDITOR

            if (entityContainer == null)
                return;

            var path = AssetDatabase.GetAssetPath(entityContainer);

            var assets = AssetDatabase.LoadAllAssetsAtPath(path);

            if (assets != null)
            {
                foreach (var a in assets)
                {
                    if (a == null)
                        continue;

                    EditorUtility.SetDirty(a);
                }
            }

            EditorUtility.SetDirty(entityContainer);
#endif
        }
    }
}
