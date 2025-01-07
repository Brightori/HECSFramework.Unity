using System.Collections.Generic;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Systems;
using UnityEditor;

namespace HECSFramework.Unity
{
    public class ShowLoadedAssetsWindow : OdinEditorWindow
    {
        [Searchable]
        public List<AssetContainer> assets = new List<AssetContainer>(256);

        [MenuItem("HECS Options/Helpers/ShowLoadedAssetsWindow")]
        public static void GetShowLoadedAssetsWindow()
        {
            GetWindow<ShowLoadedAssetsWindow>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateAssets();
        }

        [Button]
        public void UpdateAssets()
        {
            var service = EntityManager.Default.GetSingleSystem<AssetService>() ;

            if (service == null)
                service = new AssetService();

            var field = service.GetType().GetField("assetsContainers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField);

            var value = field.GetValue(service) as HashSet<AssetContainer>;

            assets.Clear();

            foreach (var asset in value)
            {
                assets.Add(asset);
            }
        }
    }
}
