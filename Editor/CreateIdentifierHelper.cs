using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Components;
using HECSFramework.Core.Generator;
using HECSFramework.Unity;
using HECSFramework.Unity.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class CreateIdentifierHelper : OdinEditorWindow
{
    [BoxGroup("Instance")]
    [ValueDropdown(nameof(GetAllowedTypes))]
    public Type Type;

    [BoxGroup("Instance")]
    public string Name;

    [BoxGroup("Instance")]
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
    
    [BoxGroup("Type")]
    public string TypeName;
    
    [BoxGroup("Type")]
    [Button("Create")]
    public void CreateIdentifierType()
    {
        if (TypeName == null || string.IsNullOrEmpty(TypeName))
            return;

        var identifierName = TypeName + "Identifier";
        var maps = string.Empty;

        maps += new UsingSyntax("HECSFramework.Unity", 1).ToString();
        maps += new UsingSyntax("UnityEngine", 1).ToString();
        
        maps += GetContainersBody(identifierName);

        SaveToFile(maps,identifierName);
    }
    
    private static string GetContainersBody(string idName)
    {
        var tree = new TreeSyntaxNode();

        tree.Add(new SimpleSyntax($"[CreateAssetMenu(fileName = \"{idName}\", menuName = \"Identifiers/{idName}\")]" + CParse.Paragraph));
        tree.Add(new SimpleSyntax($"public class {idName} : IdentifierContainer" + CParse.Paragraph));
        tree.Add(new LeftScopeSyntax());
        tree.Add(new RightScopeSyntax());

        return tree.ToString();
    }
    
    private static void SaveToFile(string data, string fileName)
    {
        var pathToDirectory = InstallHECS.ScriptPath + InstallHECS.BluePrints+ InstallHECS.Identifiers;
        var path = pathToDirectory + $"{fileName}.cs";
    
        try
        {
            if (!Directory.Exists(pathToDirectory))
                Directory.CreateDirectory(pathToDirectory);
    
            File.WriteAllText(path, data);
            var sourceFile2 = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(sourceFile2);
        }
        catch
        {
            Debug.LogError("не смогли ослить " + pathToDirectory);
        }
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