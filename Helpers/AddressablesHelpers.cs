using System.Collections.Generic;
using System.Linq;
using HECSFramework.Core;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

using UnityEngine;

namespace HECSFramework.Unity
{
    //this is not in Editor folder coz we need sometimes addressables operations from components with settings
    [Documentation(Doc.HECS, Doc.Helpers, Doc.Editor, "helpers for addressables editors funcs")]
    public static class AddressablesHelpers
    {
#if UNITY_EDITOR
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

        public static bool IsAssetAddressable(string guid)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
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
#endif
    }
}