using System;
using System.Collections.Generic;
using HECSFramework.Unity;
using HECSFramework.Unity.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class SaveLoadContainerToJSONWindow : OdinEditorWindow
{
    [TabGroup("Save")]
    public EntityContainer ContainerToSave;

    [TabGroup("Save")]
    [Sirenix.OdinInspector.FilePath(AbsolutePath = true)]
    public string PathToSave = InstallHECS.ScriptPath + InstallHECS.HECSGenerated + "/SavedContainers/";

    [MenuItem("HECS Options/Helpers/SaveLoadContainerToJSONWindow")]
    public static void GetSaveLoadToJSONContainerWindow()
    {
        GetWindow<SaveLoadContainerToJSONWindow>();
     
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        InstallHECS.CheckFolder(PathToSave);
    }

    [Button]
    [TabGroup("Save")]
    public void SaveData()
    {
        var jsonContainer = new JSONSaveContainer();
        jsonContainer.ComponentJSONSaves = new List<ComponentJSONSave>(16);
        jsonContainer.Systems = new List<int>(16);

        foreach (var c in ContainerToSave.Components)
        {
            var save = JsonUtility.ToJson(c.GetHECSComponent);
            jsonContainer.ComponentJSONSaves.Add(new ComponentJSONSave
            {
                ComponentIndex = c.GetHECSComponent.GetTypeHashCode,
                Data = save,
            });
        }

        foreach (var s in ContainerToSave.Systems)
        {
            jsonContainer.Systems.Add(s.GetSystem.GetTypeHashCode);
        }

        SaveManager.SaveJson(PathToSave + ContainerToSave.name + "jsonContainer", JsonUtility.ToJson(jsonContainer));
    }
}

[Serializable]
public struct JSONSaveContainer
{
    public List<ComponentJSONSave> ComponentJSONSaves;
    public List<int> Systems;
}

[Serializable]
public struct ComponentJSONSave
{
    public int ComponentIndex;
    public string Data;
}