using HECSFramework.Unity.Editor;
using HECSFramework.Unity.Helpers;
using Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ContainersCustomEditorsConfigsWindow : OdinMenuEditorWindow
{
    private SOProvider<ContainersFinderConfig> sOProvider= new();
    private CreateNewContainerFinderDrawler createDrawler;

    [Button("Refresh")]
    private void RefreshList()
    {
        ForceMenuTreeRebuild();
    }

    [MenuItem("HECS Options/Helpers/Custom containers editors")]
    private static void OpenWindow()
    {
        GetWindow<ContainersCustomEditorsConfigsWindow>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Config.DrawSearchToolbar = true;

        var collection = sOProvider.GetCollection().ToList();
        var sorted = collection.OrderBy(x => x.name).ToList();

        createDrawler = new CreateNewContainerFinderDrawler();
        tree.Add("Create New", createDrawler);

        foreach (var c in sorted) 
        {
            tree.Add(c.name, new ContainersFinderConfigDrawler(c));
        }

        // Создаем кастомный пункт меню с кнопкой
        var buttonItem = new OdinMenuItem(tree, "Actions", null)
        {
            OnDrawItem = DrawButtonMenuItem
        };

        tree.MenuItems.Add(buttonItem);

        return tree;
    }

    private void DrawButtonMenuItem(OdinMenuItem item)
    {
        var rect = item.Rect;

        Rect buttonRect = new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10);

        if (GUI.Button(buttonRect, "Refresh"))
        {
            ForceMenuTreeRebuild();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (createDrawler != null)
            createDrawler.Clear();
    }
}

[Serializable]
public class CreateNewContainerFinderDrawler
{
    [Space(10f)]
    public string SetName;
    [ComponentIDDropDown] public int[] FilterParameters;
    [ComponentIDDropDown] public int[] ShowingComponents;

    private ContainersFinderConfig newConfig;
    private bool isButtonEnable => (FilterParameters != null && FilterParameters.Length > 0) &&
        (ShowingComponents != null && ShowingComponents.Length > 0) &&
        !string.IsNullOrWhiteSpace(SetName);

    public void Clear()
    {
        if (newConfig != null)
            UnityEngine.Object.DestroyImmediate(newConfig);

        FilterParameters = null;
        ShowingComponents = null;
    }

    [Button("@ButtonText()")]
    [Tooltip("@ButtonText()")]
    [EnableIf("isButtonEnable")]
    private void CreateConfig()
    {
        newConfig = ScriptableObject.CreateInstance<ContainersFinderConfig>();
        var name = SetName == "" ? "ContainersFinderConfig" : SetName;

        newConfig.ContainersFilter = FilterParameters;
        newConfig.ShowingComponents = ShowingComponents;

        InstallHECS.CheckFolder("Assets/BluePrints/ContainersFinders/");

        AssetDatabase.CreateAsset(newConfig, "Assets/BluePrints/ContainersFinders/" + name + ".asset");
        AssetDatabase.SaveAssets();
    }

    private string ButtonText()
    {
        var isFiltersAndShowingSet = (FilterParameters == null || FilterParameters.Length == 0) || (ShowingComponents == null || ShowingComponents.Length == 0);

        if (isFiltersAndShowingSet && string.IsNullOrWhiteSpace(SetName))
        {
            return "Set name and add filters";
        }

        if (string.IsNullOrWhiteSpace(SetName))
        {
            return "Set name";
        }

        if (isFiltersAndShowingSet)
        {
            return "Add filters and showing components";
        }

        return $"Create '{SetName}'";
    }
}

[Serializable]
public class ContainersFinderConfigDrawler
{
    [HorizontalGroup("Header")]
    [ShowInInspector, HideLabel]
    public ContainersFinderConfig Config;
    [ComponentIDDropDown] public int[] FilterParameters;
    [ComponentIDDropDown] public int[] ShowingComponents;

    public ContainersFinderConfigDrawler(ContainersFinderConfig config)
    {
        Config = config;

        FilterParameters = config.ContainersFilter;
        ShowingComponents = config.ShowingComponents;
    }

    public static event Action<ContainersFinderConfig> OpenEditor;

    [HorizontalGroup("Header", Width = 80)]
    [Button("Remove", ButtonHeight = 25)]
    private void Remove()
    {
        string path = AssetDatabase.GetAssetPath(Config);

        AssetDatabase.DeleteAsset(path);
        AssetDatabase.SaveAssets();

        Config = null;
        FilterParameters = null;

        EditorUtility.FocusProjectWindow();
        AssetDatabase.Refresh();
    }

    [Button("Save")]
    private void SaveChanges()
    {
        Config.ContainersFilter = FilterParameters;
        Config.ShowingComponents = ShowingComponents;

        EditorUtility.SetDirty(Config);
        AssetDatabase.SaveAssets();
    }

    [Button("Show Containers")]
    private void OpenContainersEditWindow()
    {
        OpenEditor?.Invoke(Config);
    }
}
