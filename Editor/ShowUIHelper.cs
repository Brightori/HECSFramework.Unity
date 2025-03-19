using Commands;
using HECSFramework.Core;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

public class ShowUIHelper : OdinEditorWindow
{
    public UIIdentifier UIIdentifier;

    public bool IsMultiple;

    [MenuItem("HECS Options/Debug/ShowUIHelperWindow")]
    public static void ShowUIHelperWindow()
    {
        GetWindow<ShowUIHelper>();
    }

    [Button]
    public void ShowUI()
    {
        if (UIIdentifier == null)
            return;

        EntityManager.Command(new ShowUICommand { MultyView = IsMultiple, UIViewType = UIIdentifier });
    }
}
