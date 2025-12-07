using System;
using System.Collections;
using System.Collections.Generic;
using HECSFramework.Core.Helpers;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetupActorsInitsOnScene  : OdinEditorWindow
{
    public bool SetGuidPersistent = true;

    [MenuItem("HECS Options/Helpers/Scene/SetupActorsInitsOnScene")]
    public static void SetupActorsInitsOnSceneFunc()
    {
        GetWindow<SetupActorsInitsOnScene>();
    }

    [Button]
    public void Proccess()
    {
        var actorsInScene = new HashSet<Actor>(1024);
        HashSet<Guid> guidsInProcces = new(1024);

        // Получаем активную сцену
        Scene activeScene = SceneManager.GetActiveScene();

        // Все корневые объекты сцены
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        foreach (GameObject root in rootObjects)
        {
            // Получаем компоненты указанного типа во всех дочерних объектах
            actorsInScene.AddRange(root.GetComponentsInChildren<Actor>(true));
        }

        foreach (var a in actorsInScene)
        {
            var module = ReflectionHelpers.GetPrivateFieldValue<ActorInitModule>(a, "actorInitModule");
            ReflectionHelpers.SetPrivateFieldValue(module, "initActorMode", InitActorMode.InitOnStart);

            if (SetGuidPersistent)
            {
                ReflectionHelpers.SetPrivateFieldValue(module, "guidRule", GuidGenerationRule.PersistentUnique);

                if (!guidsInProcces.Add(module.Guid))
                {
                    module.SetGuid(Guid.NewGuid());
                    guidsInProcces.Add(module.Guid);
                }
            }
            else
            {
                ReflectionHelpers.SetPrivateFieldValue(module, "guidRule", GuidGenerationRule.Default);
            }

            EditorUtility.SetDirty(a);
        }
    }
}
