using System.Collections.Generic;
using System.Linq;
using Systems;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace HECSFramework.Unity
{
    public static class AddressablesHelpers
    {
        public static AddressableAssetEntry SetAddressableGroup(Object obj, string groupName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings)
            {
                var group = settings.FindGroup(groupName);
                if (!group)
                    group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

                var assetpath = AssetDatabase.GetAssetPath(obj);
                var guid = AssetDatabase.AssetPathToGUID(assetpath);

                var e = settings.CreateOrMoveEntry(guid, group, false, false);
                var entriesAdded = new List<AddressableAssetEntry> { e };

                group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);
                return e;
            }

            return default;
        }

        public static bool IsAssetAddressable(Object obj)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)));
            return entry != null;
        }

        public static string GetGuidOfObject(Object obj)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
        }

        public static bool TryGetAddressableAssetEntry(UnityEngine.Object obj, out AddressableAssetEntry addressableAssetEntry)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            addressableAssetEntry = settings.FindAssetEntry(GetGuidOfObject(obj));

            return addressableAssetEntry != null;
        }


        public static void AddLabel(AddressableAssetEntry assetEntry, string label)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            var uiBluePrintsLabel = settings.GetLabels().FirstOrDefault(x => x == label);

            if (uiBluePrintsLabel == null)
                settings.AddLabel(label);

            assetEntry.SetLabel(label, true);
        }
    }
}