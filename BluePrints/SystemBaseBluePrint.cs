using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class SystemBaseBluePrint : ScriptableObject, ISystemContainer
    {
        #region Editor
#if UNITY_EDITOR
#if !DeveloperMode
        private string DrawLabelAsBox()
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(Name);
            Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
            return Name;
        }

        [CustomValueDrawer(nameof(DrawLabelAsBox))]
        [ShowInInspector, HideLabel]
#endif

#endif
        private string Name => name.Replace("BluePrint", "");
        #endregion

        public abstract ISystem GetSystem { get; }

        public ISystem GetSystemInstance()
        {
            var t = Instantiate(this);
            return t.GetSystem;
        }

    }
}
