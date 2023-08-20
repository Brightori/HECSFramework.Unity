using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using HECSFramework.Unity.Editor;

public class AddressablesGroupsHistoryWindow : OdinEditorWindow
{
    [ShowInInspector, ValueDropdown("ShowAddressablesGroupsHistories", IsUniqueList = false)]
    public DateTime? addressablesGroupsHistoryTime = null;

    private List<AddressablesGroupsHistory> addressablesGroupsHistories = new List<AddressablesGroupsHistory>();

    private string PathToAddressablesHistoriesFolder => "Assets/BluePrints/History/AddressablesGroupsHistory/";

    public void InitOrUpdate()
    {
        InstallHECS.CheckFolder(PathToAddressablesHistoriesFolder);
        var files = Directory.GetFiles(PathToAddressablesHistoriesFolder, "*.addrHistory");

        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            addressablesGroupsHistories.Add(JsonUtility.FromJson<AddressablesGroupsHistory>(json));
        }
    }

    private IEnumerable<DateTime> ShowAddressablesGroupsHistories()
    {
        return addressablesGroupsHistories.Select(x => DateTime.FromBinary(x.DateTimeSave));
    }


    [Button("Save groups history"), PropertySpace(20)]
    private void SaveAddressablesGroupsHistory()
    {
        InstallHECS.CheckFolder(PathToAddressablesHistoriesFolder);

        var path = PathToAddressablesHistoriesFolder + "AddressablesHistory_" + $"{DateTime.UtcNow.ToFileTimeUtc()}" + ".addrHistory";
        var addressablesGroupsHistory = new AddressablesGroupsHistory();
        addressablesGroupsHistory.Init();
        var json = JsonUtility.ToJson(addressablesGroupsHistory, true);
        System.IO.File.WriteAllText(path, json);
        AssetDatabase.Refresh();
        InitOrUpdate();
        Debug.Log("Saving is successfully completed.");
    }

    [Button("Load from groups history"), PropertySpace(20)]
    private void LoadAddressablesHistory()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        if (addressablesGroupsHistoryTime != null)
        {
            var needHistory = addressablesGroupsHistories.FirstOrDefault(x => x.DateTimeSave == addressablesGroupsHistoryTime.Value.ToBinary());

            foreach (var groupFromHistory in needHistory.AddressablesGroups)
            {
                var group = settings.FindGroup(groupFromHistory.Name);
                if (!group)
                    group = settings.CreateGroup(groupFromHistory.Name, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
                                
                foreach (var assetFromHistory in groupFromHistory.Assets)
                {
                    AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetFromHistory.GUID);
                    if (assetEntry != null)
                    {
                        if (assetEntry.parentGroup.Name != groupFromHistory.Name)
                        {
                            Debug.LogWarning("Move " + assetEntry.AssetPath + " from " + assetEntry.parentGroup.Name + " group to " + groupFromHistory.Name + " group.");
                            settings.MoveEntry(assetEntry, group, false, false);
                            var entriesMoved = new List<AddressableAssetEntry> { assetEntry };
                            group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesMoved, false, true);
                            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesMoved, true, false);
                        }
                        continue;
                    }

                    assetEntry = settings.CreateOrMoveEntry(assetFromHistory.GUID, group, false, false);
                    if (assetEntry == null)
                    {
                        var guid = AssetDatabase.AssetPathToGUID(assetFromHistory.Path);
                        assetEntry = settings.CreateOrMoveEntry(guid, group, false, false);
                        if (assetEntry == null)
                        {
                            Debug.LogError("Can't find asset at path " + assetFromHistory.Path);
                            continue;
                        }
                    }
                 
                    var entriesAdded = new List<AddressableAssetEntry> { assetEntry };
                    group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entriesAdded, false, true);
                    settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entriesAdded, true, false);
                    Debug.Log(assetEntry.ToString() + " is successfully added to the group.");
                }
            }

        }
        else
        {
            Debug.LogError("You must select the history.");
        }

        Debug.Log("Loading is successfully completed.");
    }


    [MenuItem("HECS Options/Helpers/Addressables Groups History")]
    static void ShowAddressablesGroupsHistoryWindow()
    {
        var getWindow = EditorWindow.GetWindow<AddressablesGroupsHistoryWindow>();
        getWindow.InitOrUpdate();
    }


    [Serializable]
    public struct AddressablesGroupsHistory {
        public long DateTimeSave;
        public List<AddressablesGroup> AddressablesGroups;
        public void Init()
        {
            DateTimeSave = DateTime.Now.ToBinary();
            AddressablesGroups = new List<AddressablesGroup>();

            var groups = AddressableAssetSettingsDefaultObject.Settings.groups;
            foreach(var group in groups)
            {
                //todo выглядит как костыль
                if (group.Name == "Built In Data") continue;
                var groupName = group.Name;
                var assets = new List<AssetInGroup>();
                foreach(var entry in group.entries)
                {
                    assets.Add(new AssetInGroup()
                    {
                        GUID = entry.guid,
                        Path = entry.AssetPath
                    });;
                }
                AddressablesGroups.Add(new AddressablesGroup()
                {
                    Name = groupName,
                    Assets = assets
                });
            }
        }

        [Serializable]
        public struct AddressablesGroup
        {
            public string Name;
            public List<AssetInGroup> Assets;
        }

        [Serializable]
        public struct AssetInGroup
        {
            public string GUID;
            public string Path;
        }
    }
}
