using HECSFramework.Core;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity.Generator
{
    public class UnityGenerator
    {
        private readonly string DefaultPath = "/Scripts/HECSGenerated/";
        private string dataPath = Application.dataPath;

        private const string TypeProvider = "TypeProvider.cs";
        private const string MaskProvider = "MaskProvider.cs";
        private const string HecsMasks = "HECSMasks.cs";
        private const string SystemBindings = "SystemBindings.cs";
        private const string ComponentContext = "ComponentContext.cs";


        [MenuItem("HECS Options/HECS Codogen")]
        public static void Test()
        {
            var generator = new CodeGenerator();
            var unityGenerator = new UnityGenerator();
            generator.GatherAssembly();
            unityGenerator.SaveToFile(TypeProvider, generator.GenerateTypesMap());
            unityGenerator.SaveToFile(MaskProvider, generator.GenerateMaskProvider());
            unityGenerator.SaveToFile(SystemBindings, generator.GetSystemBinds());
            unityGenerator.SaveToFile(ComponentContext, generator.GetComponentContext());
            unityGenerator.SaveToFile(HecsMasks, generator.GenerateHecsMasks());
        }

        private void SaveToFile(string name, string data)
        {
            var path = dataPath + DefaultPath + name;

            if (!Directory.Exists(dataPath + DefaultPath))
                Directory.CreateDirectory(dataPath + DefaultPath);

            File.WriteAllText(path, data);

            var sourceFile2 = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(sourceFile2);
        }
    }
}
