using HECSFramework.Core.Generator;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity.Generator
{
    public partial class UnityProcessGeneration
    {
        public const string BluePrint = "BluePrint";

        private const string DefaultPath = "/Scripts/HECSGenerated/";
        private string dataPath = Application.dataPath;
        private const string Documentation = "Documentation.cs";

        [MenuItem("HECS Options/Generate Documentation")]
        public static void CodogenDocumentation()
        {
            var generator = new CodeGenerator();
            var unityProcessGeneration = new UnityProcessGeneration();
            generator.GatherAssembly();
            unityProcessGeneration.SaveToFile(Documentation, generator.GetDocumentation(), needToImport: true);
        }

        private void SaveToFile(string name, string data, string pathToDirectory = DefaultPath, bool needToImport = false)
        {
            var path = dataPath + pathToDirectory + name;

            try
            {
                if (!Directory.Exists(Application.dataPath + pathToDirectory))
                    Directory.CreateDirectory(Application.dataPath + pathToDirectory);

                File.WriteAllText(path, data);

                var sourceFile2 = path.Replace(Application.dataPath, "Assets");

                if (needToImport)
                    AssetDatabase.ImportAsset(sourceFile2);
            }
            catch
            {
                Debug.LogError("не смогли ослить " + pathToDirectory);
            }
        }
    }
}
