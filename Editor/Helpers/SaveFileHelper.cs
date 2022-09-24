using System.IO;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity
{
    public static class SaveFileHelper
    {
        public static void SaveToFileIfExists(string data, string directory, string fileName)
        {
            var pathLocal = directory +"/"+fileName; ;

            var find = Directory.GetFiles(Application.dataPath, fileName, SearchOption.AllDirectories);

            if (find != null && find.Length > 0 && !string.IsNullOrEmpty(find[0]))
            {
                pathLocal = find[0];
            }

            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(pathLocal, data);
                var sourceFile2 = pathLocal.Replace(Application.dataPath, "Assets");
                AssetDatabase.ImportAsset(sourceFile2);
            }
            catch
            {
                Debug.LogError("we cant write file at path " + pathLocal);
            }
        }
    }
}
