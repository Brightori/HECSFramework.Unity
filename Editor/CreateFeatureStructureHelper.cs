using HECSFramework.Unity.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

public class CreateFeatureStructureHelper : OdinEditorWindow
{
    public string FeatureName;

    [MenuItem("HECS Options/Helpers/Create Feature Folder")]
    public static void CreateFeatureStructureHelperFunc()
    {
        GetWindow<CreateFeatureStructureHelper>();
    }

    [Button]
    public void CreateStructure()
    {
        if (string.IsNullOrEmpty(FeatureName))
            throw new System.Exception("Feature name is empty");

        var path = InstallHECS.ScriptPath + "Features/"+ $"{FeatureName}/";

        InstallHECS.CheckFolder(path);
        InstallHECS.CheckFolder(path + "Components/");
        InstallHECS.CheckFolder(path + "Systems/");
        InstallHECS.CheckFolder(path + "Commands/");
        InstallHECS.CheckFolder(path + "Actions/");
        InstallHECS.CheckFolder(path + "Identifiers/");
        AssetDatabase.Refresh();
    }
}
