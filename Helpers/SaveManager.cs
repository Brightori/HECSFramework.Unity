using Components;
using HECSFramework.Core;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HECSFramework.Unity
{
    [Documentation("Gamelogic", "Player", "Хелпер который отвечает за сохранение и чтение файлов")]
    public class SaveManager
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

            Debug.LogAssertion("нет файла сохранения");
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