using System.Collections.Generic;
using System.IO;
using System.Linq;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Editor;
using Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Systems;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

[Documentation(Doc.Editor, Doc.HECS, Doc.UI, "Its helper for create ui, this window create ui identifier, ui blueprint, and set ui actor to prfb if needed, after this ui blueprint and uiactor will be added to addressables")]
public class CreateUIHelperWindow : OdinEditorWindow
{
    //Addressables Group names
    private const string UIActors = "UI_Actors";
    private const string UIBluePrints = "UI_BluePrints";

    public string IdentifierName;

    [AssetSelector()]
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
        //make path to needed folder, we take constants from InstallHECS
        var pathBluePrints = InstallHECS.DataPath + InstallHECS.BluePrints + InstallHECS.UIBluePrints;
        var pathUIIdentifiers = InstallHECS.DataPath + InstallHECS.BluePrints + InstallHECS.Identifiers+InstallHECS.UIIdentifiers;

        //Here we check and create folders if that needed
        InstallHECS.CheckFolder(pathBluePrints);
        InstallHECS.CheckFolder(pathUIIdentifiers);

        //check all fields
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

        //Create objects of blueprints
        var uiidentifier = CreateInstance<UIIdentifier>();
        var uibluePrint = CreateInstance<UIBluePrint>();

        //Assign their names, names depends from fields
        uiidentifier.name = IdentifierName;
        uibluePrint.name = $"{UIprfb.name}_UIBluePrint";

        //save SO to project
        AssetDatabase.CreateAsset(uibluePrint, InstallHECS.Assets + InstallHECS.BluePrints + InstallHECS.UIBluePrints + $"{uibluePrint.name}.asset");
        AssetDatabase.CreateAsset(uiidentifier, InstallHECS.Assets + InstallHECS.BluePrints + InstallHECS.Identifiers+InstallHECS.UIIdentifiers + $"{uiidentifier.name}.asset");

        //Try to add uiactor component to prfb and save it
        var actor = UIprfb.GetOrAddMonoComponent<UIActor>();
        var uiActorPath = AssetDatabase.GetAssetPath(UIprfb);

        //This all around adding ui blueprint and prfb to addressables groups
        var addressablesSettings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
        var addressablesSchemas = CreateInstance<PlayerDataGroupSchema>();

        var uiactorsGroup = addressablesSettings.groups.FirstOrDefault(x => x.name == UIActors);
        var uiBluePrintsGroup = addressablesSettings.groups.FirstOrDefault(x => x.name == UIBluePrints);

        var uiBluePrintsLabel = addressablesSettings.GetLabels().FirstOrDefault(x=> x == UISystem.UIBluePrints);

        if (uiBluePrintsLabel == null)
            addressablesSettings.AddLabel(UISystem.UIBluePrints);

        //if we dont have group for ui actor we add group
        if (uiactorsGroup == null)
        {
            uiactorsGroup = addressablesSettings.CreateGroup(UIActors, false, false, false, 
                new List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroupSchema>() 
                    { addressablesSchemas } , new System.Type[0]);
        }

        //if we dont have group for ui actor we add group
        var addressablesSchemas2 = CreateInstance<PlayerDataGroupSchema>();
        if (uiBluePrintsGroup == null)
        {
            uiBluePrintsGroup = addressablesSettings.CreateGroup(UIBluePrints, false, false, false, 
                new List<UnityEditor.AddressableAssets.Settings.AddressableAssetGroupSchema>() 
                    { addressablesSchemas2 }, new System.Type[0]);
        }

        //take guid from prfb and save prfb to addressables groupd
        var prfbGuid = AssetDatabase.GUIDFromAssetPath(uiActorPath);
        var entry = addressablesSettings.CreateOrMoveEntry(prfbGuid.ToString(), uiactorsGroup);

        //take guid from bluePrint and save prfb to addressables groupd
        var uiBluePrintPath = AssetDatabase.GetAssetPath(uibluePrint);
        var uiBluePrintGuid = AssetDatabase.GUIDFromAssetPath(uiBluePrintPath);
        var uiBluePrintEntry = addressablesSettings.CreateOrMoveEntry(uiBluePrintGuid.ToString(), uiBluePrintsGroup);
        uiBluePrintEntry.SetLabel(UISystem.UIBluePrints, true);

        //assign fields of blueprints
        uibluePrint.UIType = uiidentifier;
        uibluePrint.UIActor = new UIActorReference(prfbGuid.ToString());
        var uiGroupsType = uibluePrint.Groups.GetType();

        var fields = uiGroupsType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        foreach (var f in fields)
        {
            if (f.Name == "Groups")
            {
                f.SetValue(uibluePrint.Groups, Groups);
                break;
            }
        }



        AssetDatabase.SaveAssets();
    }
}