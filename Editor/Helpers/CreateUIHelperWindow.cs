using System.IO;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Editor;
using Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[Documentation(Doc.Editor, Doc.HECS, "Its helper for create ui, this window create ui identifier, ui blueprint, and set ui actor to prfb if needed, after this ui blueprint and uiactor will be added to addressables")]
public class CreateUIHelperWindow : OdinEditorWindow
{
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

        var uiidentifier = ScriptableObject.CreateInstance<UIIdentifier>();
        var uibluePrint = ScriptableObject.CreateInstance<UIBluePrint>();

        uiidentifier.name = IdentifierName;
        uibluePrint.name = $"{UIprfb.name}_UIBluePrint";

        AssetDatabase.CreateAsset(uibluePrint, InstallHECS.Assets + InstallHECS.BluePrints + InstallHECS.UIBluePrints + $"{uibluePrint.name}.asset");
        AssetDatabase.CreateAsset(uiidentifier, InstallHECS.Assets + InstallHECS.BluePrints + InstallHECS.Identifiers+InstallHECS.UIIdentifiers + $"{uiidentifier.name}.asset");

        

        var actor = UIprfb.GetOrAddMonoComponent<UIActor>();
        var path = AssetDatabase.GetAssetPath(UIprfb);

        //PrefabUtility.ApplyAddedComponent(actor, path, InteractionMode.AutomatedAction);
        //var components = PrefabUtility.GetAddedComponents(t);

        //foreach (var c in components)
        //{

        //    var ct = c.GetAssetObject();
        //    //if (c.GetAssetObject() != null)
        //}

        AssetDatabase.SaveAssets();
    }
}
