using System;
using System.Collections.Generic;
using System.Linq;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class SaveLoadContainerToJSONWindow : OdinEditorWindow
{
    public EntityContainer Container;

    [Sirenix.OdinInspector.FilePath(AbsolutePath = true)]
    public string PathToSave = InstallHECS.ScriptPath + InstallHECS.HECSGenerated + "/SavedContainers/";

    private BluePrintsProvider bluePrintsProvider = new BluePrintsProvider();
    public bool overWriteData = true;

    [MenuItem("HECS Options/Helpers/SaveLoadContainerToJSONWindow")]
    public static void GetSaveLoadToJSONContainerWindow()
    {
        GetWindow<SaveLoadContainerToJSONWindow>();

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InstallHECS.CheckFolder(PathToSave);
        bluePrintsProvider = new BluePrintsProvider();
    }

    [Button]
    public void LoadData()
    {
        if (SaveManager.TryLoadJson(PathToSave, out var json))
        {
            var jsonContainer = JsonUtility.FromJson<JSONSaveContainer>(json);

            if (jsonContainer.ComponentJSONSaves != null)
            {
                foreach (var c in jsonContainer.ComponentJSONSaves)
                {
                    if (TypesMap.GetComponentInfo(c.ComponentIndex, out var info))
                    {
                        var alrdy = Container.Components.FirstOrDefault(x => x.GetHECSComponent.GetTypeHashCode == c.ComponentIndex);

                        if (alrdy != null)
                        {
                            if (overWriteData)
                            {
                                JsonUtility.FromJsonOverwrite(c.Data, alrdy.GetHECSComponent);
                                EditorUtility.SetDirty(alrdy);
                            }
                        }
                        else
                        {
                            var type = TypesMap.GetTypeByComponentHECSHash(c.ComponentIndex);

                            var componentBluePrint = ScriptableObject.CreateInstance(bluePrintsProvider.Components[type]) as ComponentBluePrint;
                            componentBluePrint.name = type.Name;
                            JsonUtility.FromJsonOverwrite(c.Data, componentBluePrint.GetHECSComponent);
                            EditorUtility.SetDirty(componentBluePrint);
                            AssetDatabase.AddObjectToAsset(componentBluePrint, Container);
                            Container.AddComponent(componentBluePrint);
                        }
                    }
                    else
                    {
                        Debug.LogError("we dont have component " + c.Name);
                    }
                }
            }

            if (jsonContainer.Systems != null)
            {
                foreach (var system in jsonContainer.Systems)
                {
                    if (Container.Systems.Any(x => x.GetSystem.GetTypeHashCode == system.Index))
                        continue;

                    var systemNew = TypesMap.GetSystemFromFactory(system.Index);

                    if ( systemNew != null)
                    {
                        var sysBpType = bluePrintsProvider.Systems[systemNew.GetType()];
                        var sysBp = ScriptableObject.CreateInstance(sysBpType) as SystemBaseBluePrint;
                        sysBp.name = systemNew.GetType().Name;

                        AssetDatabase.AddObjectToAsset(sysBp, Container);
                        Container.AddSystem(sysBp);
                    }
                    else
                    {
                        Debug.LogError("we dont have system " + system.Name);
                    }
                }
            }
        }
    }

    [Button]
    public void SaveData()
    {
        var jsonContainer = new JSONSaveContainer();
        jsonContainer.ComponentJSONSaves = new List<ComponentJSONSave>(16);
        jsonContainer.Systems = new List<SystemIndexName>(16);

        foreach (var c in Container.Components)
        {
            var save = JsonUtility.ToJson(c.GetHECSComponent);
            var data = new ComponentJSONSave
            {
                ComponentIndex = c.GetHECSComponent.GetTypeHashCode,
                Data = save,
                Name = c.GetHECSComponent.GetType().Name,
            };

            jsonContainer.ComponentJSONSaves.Add(data);
        }

        foreach (var s in Container.Systems)
        {
            jsonContainer.Systems.Add(new SystemIndexName { Index = s.GetSystem.GetTypeHashCode, Name = s.GetSystem.GetType().Name });
        }

        SaveManager.SaveJson(PathToSave + Container.name + ".jsonContainer", JsonUtility.ToJson(jsonContainer));
    }
}

[Serializable]
public struct JSONSaveContainer
{
    public List<ComponentJSONSave> ComponentJSONSaves;
    public List<SystemIndexName> Systems;
}

[Serializable]
public struct SystemIndexName
{
    public string Name;
    public int Index;
}

[Serializable]
public struct AdditionalData
{
    public string Type;
    public string Data;
}

[Serializable]
public struct ComponentJSONSave
{
    public int ComponentIndex;
    public string Name;
    public string Data;
    public List<AdditionalData> AdditionalDatas;
}