using System;
using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(ContainersFinderConfig), menuName = "Tools/ContainersFinderConfig", order = 0)]
public class ContainersFinderConfig : ScriptableObject
{
    [ComponentIDDropDown] public int[] ContainersFilter;
    [ComponentIDDropDown] public int[] ShowingComponents;

    #region Editor
#if UNITY_EDITOR
    public static event Action<ContainersFinderConfig> OpenEditor;

    [Button("Show Containers")]
    private void OpenContainersEditWindow()
    {
        OpenEditor?.Invoke(this);
    }
#endif
    #endregion
}
