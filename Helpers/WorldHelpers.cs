using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HECSFramework.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace HECSFramework.Unity
{
    [Documentation(Doc.HECS, "This helper load location to ")]
    public static partial class WorldHelpers
    {
        public static async Task<SceneInstance> LoadLocationToWorld(World world, AssetReference location)
        {
            var scene = await Addressables.LoadSceneAsync(location, UnityEngine.SceneManagement.LoadSceneMode.Additive).Task;
            InitGameObjects(world, scene.Scene.GetRootGameObjects());
            return scene;
        }

        public static async Task<SceneInstance> LoadLocationToWorld(World world, string location)
        {
            var scene = await Addressables.LoadSceneAsync(location, UnityEngine.SceneManagement.LoadSceneMode.Additive).Task;
            InitGameObjects(world, scene.Scene.GetRootGameObjects());
            return scene;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InitGameObjects(World world, GameObject[] gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                var actors = gameObject.GetComponentsInChildren<Actor>();

                foreach (var a in actors)
                {
                    if (a.IsInited)
                    {
                        HECSDebug.LogError("Actors on scene when we load it to world, should not be inited, remove init on start from them");
                        continue;
                    }

                    a.Init(world.Index, true);
                }
            }
        }
    }
}