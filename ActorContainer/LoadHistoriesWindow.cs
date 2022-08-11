#if UNITY_EDITOR
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static HECSFramework.Unity.EntityContainer;

public class LoadHistoriesWindow : OdinEditorWindow
{
    [ShowInInspector, ValueDropdown("ShowHistories", IsUniqueList = false)]
    public DateTime? history = null;

    private List<History> histories = new List<History>();
    public string HistoryPath => Path.Combine(Application.dataPath, "BluePrints", "History");
    private EntityContainer currentContainer;


    public void Init(EntityContainer entityContainer)
    {
        currentContainer = entityContainer;

        var files = Directory.GetFiles(HistoryPath, "*.history");

        foreach (var file in files)
        {
            var fileName =  Path.GetFileName(file);
            var split = fileName.Split('_');
            var nameNeeded = split[0];

            if (nameNeeded == entityContainer.name)
            {
                var json = File.ReadAllText(file);
                histories.Add(JsonUtility.FromJson<History>(json));
            }
        }
    }

    [Button(ButtonSizes.Large)]
    public void FullCopyDataAndMissedTypes()
    {
        if (history != null)
        {
            var needHistory = histories.FirstOrDefault(x => x.DateTimeSave == history.Value.ToBinary());
            var bpp = new BluePrintsProvider();

            foreach (var item in needHistory.componentHistories)
            {
                var componentType  = TypesMap.GetTypeByComponentHECSHash(item.Index);
                var bpType = bpp.Components[componentType];

                var neededBp = currentContainer.Components.FirstOrDefault(x => x.GetType() == bpType);

                if (neededBp != null)
                    JsonUtility.FromJsonOverwrite(item.JSON, neededBp);
                else
                {
                    var asset = ScriptableObject.CreateInstance(bpType);
                    JsonUtility.FromJsonOverwrite(item.JSON, asset);
                    AssetDatabase.AddObjectToAsset(asset, currentContainer);
                    asset.name = bpType.Name;
                    currentContainer.AddComponent(asset as ComponentBluePrint);
                }
            }

            foreach (var item in needHistory.systemHistories)
            {
                var componentType = TypesMap.GetSystemFromFactory(item.Index).GetType();
                var bpType = bpp.Systems[componentType];

                var neededBp = currentContainer.Systems.FirstOrDefault(x => x.GetType() == bpType);

                if (neededBp != null)
                    continue;
                else
                {
                    var asset = ScriptableObject.CreateInstance(bpType);
                    JsonUtility.FromJsonOverwrite(item.JSON, asset);
                    AssetDatabase.AddObjectToAsset(asset, currentContainer);
                    asset.name = bpType.Name;
                    currentContainer.AddSystem(asset as SystemBaseBluePrint);
                }
            }

            EditorUtility.SetDirty(currentContainer);
            AssetDatabase.SaveAssets();
        }
    }

    private IEnumerable<DateTime> ShowHistories()
    {
        return histories.Select(x => DateTime.FromBinary(x.DateTimeSave));
    }
}
#endif