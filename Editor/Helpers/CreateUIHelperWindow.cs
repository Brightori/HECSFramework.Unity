using System.Collections.Generic;
using System.IO;
using System.Linq;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Editor;
using Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

[Documentation(Doc.Editor, Doc.HECS, Doc.UI, "Its helper for create ui, this window create ui identifier, ui blueprint, and set ui actor to prfb if needed, after this ui blueprint and uiactor will be added to addressables")]
public class CreateUIHelperWindow : OdinEditorWindow
{
    private const string UIBluePrintsGroup = "UIBluePrints";
    private const string UIActors = "UIBluePrints";

    public string IdentifierName;
    public GameObject UIprfb;
    public UIGroupIdentifier[] Groups;
    public bool IsNeedContainer = true;

    [MenuItem("HECS Options/Helpers/Create UI Window")]
    public static void GetCreateUIHelperWindow()
    {
        GetWindow<CreateUIHelperWindow>();
    }

    [Button]
    public void CreateUI()
    {
        var pathBluePrints = InstallHECS.DataPath + InstallHECS.BluePrints + InstallHECS.UIBluePrints;
        var pathUIIdentifiers = InstallHECS.DataPath + InstallHECS.BluePrints + InstallHECS.Identifiers+InstallHECS.UIIdentifiers;
        InstallHECS.CheckFolder(pathBluePrints);
        InstallHECS.CheckFolder(pathUIIdentifiers);

        if (string.IsNullOrEmpty(IdentifierName))
        {
            this.ShowNotification(new GUIContent("Identifier name not setted properly"));
            return;
        }

        if (File.Exists(pathUIIdentifiers + $"{IdentifierName}.asset"))
        {
            this.ShowNotification(new GUIContent("We alrdy have identifier like this"));
            return;
        }

        if (UIprfb == null)
        {
            this.ShowNotification(new GUIContent("Set ui prfb in field"));
            return;
        }

        var uiidentifier = CreateInstance<UIIdentifier>();
        var uibluePrint = CreateInstance<UIBluePrint>();

        uiidentifier.name = IdentifierName;
        uibluePrint.name = $"{UIprfb.name}_UIBluePrint";

        AssetDatabase.CreateAsset(uibluePrint, InstallHECS.Assets + InstallHECS.BluePrints + InstallHECS.UIBluePrints + $"{uibluePrint.name}.asset");
        AssetDatabase.CreateAsset(uiidentifier, InstallHECS.Assets + InstallHECS.BluePrints + InstallHECS.Identifiers+InstallHECS.UIIdentifiers + $"{uiidentifier.name}.asset");

        var actor = UIprfb.GetOrAddMonoComponent<UIActor>();
        var path = AssetDatabase.GetAssetPath(UIprfb);
        AssetDatabase.SaveAssets();

        var addressablesSettings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
        var addressablesSchemas = CreateInstance<PlayerDataGroupSchema>();

        var uiactorsGroup = addressablesSettings.groups.FirstOrDefault(x => x.name == UIActors);
        var uiBluePrintsGroup = addressablesSettings.groups.FirstOrDefault(x => x.name == UIBluePrintsGroup);

        if (uiactorsGroup == null)
        {
            uiactorsGroup = addressablesSettings.CreateGroup(UIActors, false, false, false, 
                new List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroupSchema>() 
                    { addressablesSchemas } , null);
        }

        if (uiBluePrintsGroup == null)
        {
            uiBluePrintsGroup = addressablesSettings.CreateGroup(UIActors, false, false, false, 
                new List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroupSchema>() 
                    { addressablesSchemas }, null);
        }


        var prfbGuid = AssetDatabase.GUIDFromAssetPath(path);
        var entry = addressablesSettings.CreateOrMoveEntry(prfbGuid.ToString(), uiactorsGroup);

    }
}