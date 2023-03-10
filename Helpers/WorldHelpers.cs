using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HECSFramework.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace HECSFramework.Unity
{
    [Documentation(Doc.HECS, "This helper load location to world")]
    public static partial class WorldHelpers
    {
        public static async Task<SceneInstance> LoadLocationToWorld(this World world, AssetReference location)
        {
            var scene = await Addressables.LoadSceneAsync(location, UnityEngine.SceneManagement.LoadSceneMode.Additive).Task;
            InitGameObjects(world, scene.Scene.GetRootGameObjects());
            return scene;
        }

        public static async Task<SceneInstance> LoadLocationToWorld(this World world, string location)
        {
            var scene = await Addressables.LoadSceneAsync(location, UnityEngine.SceneManagement.LoadSceneMode.Additive).Task;
            InitGameObjects(world, scene.Scene.GetRootGameObjects());
            return scene;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InitGameObjects(World world, GameObject[] gameObjects)
        {
            var actorsList = new List<Actor>(16);

            foreach (var gameObject in gameObjects)
            {
                var actors = gameObject.GetComponentsInChildren<Actor>();

                foreach (var a in actors)
                {
                    if (a.IsAlive())
                    {
                        HECSDebug.LogError("Actors on scene when we load it to world, should not be inited, remove init on start from them");
                        continue;
                    }

                    a.Init(world, true, true);
                    //a.Entity.Init();
                    actorsList.Add(a);
                }

                foreach (var a in actors)
                {
                    foreach (var s in a.Entity.Systems)
                    {
                        if (s is IStartOnScene startOnScene)
                        {
                            startOnScene.StartOnScene();
                        }
                    }
                }
            }
        }
    }
}