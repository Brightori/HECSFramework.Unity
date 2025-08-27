using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HECSFramework.Unity;
using HECSFramework.Unity.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class CreateIdentifierHelper : OdinEditorWindow
{
    [ValueDropdown(nameof(GetAllowedTypes))]
    public Type Type;

    public string Name;

    [Button]
    public void CreateIdentifier()
    {
        if (Type == null || string.IsNullOrEmpty(Name))
            return;

        var fileName = $"{Name}.asset";

        var path = InstallHECS.DataPath + InstallHECS.BluePrints + InstallHECS.Identifiers + $"{Type.Name}/" + fileName;

        if (File.Exists(path))
            return;

        InstallHECS.CheckFolder(InstallHECS.DataPath + InstallHECS.BluePrints + InstallHECS.Identifiers + $"{Type.Name}/");

        var so =  ScriptableObject.CreateInstance(Type);
        var pathAssetDataBase = (InstallHECS.Assets + InstallHECS.BluePrints + InstallHECS.Identifiers + $"{Type.Name}/" + fileName).Replace("//", "/");

        AssetDatabase.CreateAsset(so, pathAssetDataBase);
    }


    [MenuItem("HECS Options/Helpers/Create Identifier Helper")]
    public static void ShowWindow()
    {
        GetWindow<CreateIdentifierHelper>();
    }

    private static IEnumerable<Type> GetAllowedTypes()
    {
        return typeof(IdentifierContainer).Assembly
            .GetTypes()
            .Where(t => typeof(IdentifierContainer).IsAssignableFrom(t) && !t.IsAbstract);
    }
}