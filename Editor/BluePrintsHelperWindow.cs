using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class BluePrintsHelperWindow : OdinEditorWindow
{
    [ValueDropdown("GetActualBP")]
    public Type BluePrint;
    
    private List<Type> BluePrints = new List<Type>();

    [MenuItem("HECS Options/Helpers/BluePrintsHelperWindow %&#F12")]
    public static void OpenBluePrintsHelperWindow()
    {
        GetWindow<BluePrintsHelperWindow>().Init();
    }

    public IEnumerable GetActualBP()
    {
        return BluePrints;
    }

    public void Init()
    {
        BluePrints = GetBlueprintClasses();
    }

    public static List<Type> GetBlueprintClasses()
    {
        List<Type> result = new List<Type>(1024);

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            string assemblyName = assembly.GetName().Name;
            if (assemblyName.StartsWith("System") || assemblyName.StartsWith("UnityEngine") || assemblyName.StartsWith("UnityEditor"))
                continue;

            try
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    CreateAssetMenuAttribute attribute = type.GetCustomAttribute<CreateAssetMenuAttribute>();

                    if (attribute != null && !string.IsNullOrEmpty(attribute.menuName))
                    {
                        bool hasBlueprint = attribute.menuName.Contains("BluePrints", StringComparison.OrdinalIgnoreCase);

                        if (hasBlueprint)
                        {
                            result.Add(type);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
        }

        return result;
    }

    [Button]
    public void GenerateBP()
    {
        if (BluePrint == null)
            return;

        var so = ScriptableObject.CreateInstance(BluePrint);
        so.name = BluePrint.Name;
        var path = GetCurrentFolder()+$"/{so.name}.asset";

        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), path)))
            return;

        AssetDatabase.CreateAsset(so, path);
    }

    public static string GetCurrentFolder()
    {
        Type projectWindowUtilType = typeof(ProjectWindowUtil);
        MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
        object obj = getActiveFolderPath.Invoke(null, new object[0]);
        string pathToCurrentFolder = obj.ToString();
        return pathToCurrentFolder;
    }
}
