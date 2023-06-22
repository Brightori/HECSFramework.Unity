using HECSFramework.Core;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HECSFramework.Unity
{
    [Documentation(Doc.GameLogic, Doc.Player, "Хелпер который отвечает за сохранение и чтение файлов")]
    public partial class SaveManager
    {
        public static string DefaultSaveDataPath => Application.persistentDataPath + "/saveData.dat";
        
        public static bool TryLoadFromFile(string path, out object data)
        {
            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open);
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    var loadData = formatter.Deserialize(fs);

                    data = loadData;
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogAssertion("загрузка данных накрылась" + ex.Message);
                }
                finally
                {
                    fs.Close();
                }
            }

            Debug.Log("нет файла сохранения");
            data = null;
            return false;
        }

        public static void SaveToFile(string path, object saveData)
        {
            FileStream fs = new FileStream(path, FileMode.Create);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, saveData);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        public static void SaveToFile(string path, byte[] data)
        {
            var folderPath = Path.GetDirectoryName(path);
            bool exists = Directory.Exists(folderPath);
            if (!exists) Directory.CreateDirectory(folderPath);

            FileStream fs = new FileStream(path, FileMode.Create);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            try
            {
                fs.Write(data, 0, data.Length);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

#if UNITY_EDITOR
        [MenuItem("HECS Options/Debug/Clear Data")]
#endif
        public static void ClearSavedData()
        {
            if (File.Exists(DefaultSaveDataPath))
                File.Delete(DefaultSaveDataPath);

            Debug.Log("удалили сейв");
        }
    }
}