using System;
using System.Linq;
using UnityEditor;

namespace HECSFramework.Unity.Editor
{
    [InitializeOnLoad]
    public static class DebugModeSwitcher
    {
        private const string Developer = "HECS Options/Debug/Mode/Developer";
        private const string Modify = "HECS Options/Debug/Mode/Modify";
        private const string Basic = "HECS Options/Debug/Mode/Basic";

        private static HecsEditorMode EditorMode
        {
            get => (HecsEditorMode)EditorPrefs.GetInt("HecsEditorMode");
            set => EditorPrefs.SetInt("HecsEditorMode", (int)value);
        }

        static DebugModeSwitcher()
        {
            EditorApplication.delayCall += SetCheckBox;
        }

        [MenuItem(Developer)]
        private static void SetDeveloperMode()
        {
            EditorMode = HecsEditorMode.Developer;
            SetCheckBox();
        }

        [MenuItem(Modify)]
        private static void SetModifyMode()
        {
            EditorMode = HecsEditorMode.Modify;
            SetCheckBox();
        }

        [MenuItem(Basic)]
        private static void SetBasicMode()
        {
            EditorMode = HecsEditorMode.Basic;
            SetCheckBox();
        }

        private static void SetCheckBox()
        {
            Menu.SetChecked(Developer, false);
            Menu.SetChecked(Modify, false);
            Menu.SetChecked(Basic, false);
            switch (EditorMode)
            {
                case HecsEditorMode.Basic:
                    SetDefines(BuildTargetGroup.Standalone, "BasicMode");
                    Menu.SetChecked(Basic, true);
                    break;
                case HecsEditorMode.Modify:
                    SetDefines(BuildTargetGroup.Standalone, "ModifyMode");
                    Menu.SetChecked(Modify, true);
                    break;
                case HecsEditorMode.Developer:
                    SetDefines(BuildTargetGroup.Standalone, "DeveloperMode");
                    Menu.SetChecked(Developer, true);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static void SetDefines(BuildTargetGroup target, string mode)
        {
            string existingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            var existingDefinesList = existingDefines.Split(';').ToList();
            existingDefinesList.Remove("DeveloperMode");
            existingDefinesList.Remove("ModifyMode");
            existingDefinesList.Remove("BasicMode");
            existingDefinesList.Add(mode);
            var newDefines = string.Join(";", existingDefinesList);
            if (string.Equals(newDefines, existingDefines)) return;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, newDefines);
        }
    }
}