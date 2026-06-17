using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;

public class MultipleContainerWindow : OdinEditorWindow
{
    [ListDrawerSettings(OnTitleBarGUI = nameof(DrawSearchButton))]
    public List<EntityContainer> Containers;

    [ValueDropdown(nameof(GetComponents))] public Type ComponentType;

    [ValueDropdown(nameof(GetSystems))] public Type SystemType;

    public PresetContainer ApplyContainer;

    private SearchContainerWindow searchWindow;

    [Button]
    public void ApplyPreset()
    {
        foreach (var container in Containers)
        {
            ApplyContainer.CopyOrReplaceComponents(container);
        }
    }

    [Button]
    public void AddComponent()
    {
        if (ComponentType == null)
            return;

        var bp = new BluePrintsProvider();

        var componentNode = new ComponentBluePrintNode(ComponentType.Name, bp.Components[ComponentType], Containers);
        componentNode.AddBluePrint();
    }

    [Button]
    public void AddSystem()
    {
        if (SystemType == null)
            return;

        var bp = new BluePrintsProvider();

        var componentNode = new SystemBluePrintNode(SystemType.Name, bp.Systems[SystemType], Containers);
        componentNode.AddBluePrint();
    }

    private IEnumerable<Type> GetComponents()
    {
        return new BluePrintsProvider().Components.Keys;
    }

    private IEnumerable<Type> GetSystems()
    {
        return new BluePrintsProvider().Systems.Keys;
    }

    [MenuItem("HECS Options/Helpers/MultipleContainerHelper")]
    public static void GetCreateUIHelperWindow()
    {
        GetWindow<MultipleContainerWindow>();
    }

    private void DrawSearchButton()
    {
        if (SirenixEditorGUI.ToolbarButton(EditorIcons.MagnifyingGlass))
        {
            searchWindow = SearchContainerWindow.OpenWindow();
            searchWindow.OnClose += OnSearchWindowClose;
            searchWindow.SearchFinished += OnSearchFinished;
        }
    }

    private void OnSearchFinished(List<EntityContainer> collection)
    {
        Containers = collection;
    }

    private void OnSearchWindowClose()
    {
        if (searchWindow  != null)
        {
            searchWindow.SearchFinished -= OnSearchFinished;
            searchWindow.OnClose -= OnSearchWindowClose;
            searchWindow = null;
        }
    }
    
}

public class SearchContainerWindow : OdinEditorWindow
{
    [ComponentIDDropDown, BoxGroup("Filter")] public int[] ContainersFilter;
    [ComponentIDDropDown, BoxGroup("Filter")] public int[] ExcludeContainersFilter;

    private SOProvider<EntityContainer> sOProvider = new();

    public event Action<List<EntityContainer>> SearchFinished;

    public static SearchContainerWindow OpenWindow()
    {
        var window = GetWindow<SearchContainerWindow>();
        window.Show();
        return window;
    }

    [Button]
    private void Search()
    {
        var collection = sOProvider.GetCollection()
            .Where(x => x is not PresetContainer
                        && ContainersFilter.All(p => x.ContainsComponent(p))
                        && ExcludeContainersFilter.All(p => !x.ContainsComponent(p))
                        && !x.ContainsComponent(ComponentProvider<IgnoreReferenceContainerTagComponent>.TypeIndex,
                            true))
            .ToList();

        SearchFinished?.Invoke(collection);
    }
}