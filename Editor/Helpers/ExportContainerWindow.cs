using System.Collections.Generic;
using System.IO;
using System.Linq;
using HECSFramework.Unity.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity
{
    public class ExportContainerWindow : OdinEditorWindow
    {
        [FolderPath(AbsolutePath = true)]
        public string CopyTo;

        public EntityContainer EntityContainer;

        private List<FileInfo> Components = new List<FileInfo>();
        private List<FileInfo> Systems = new List<FileInfo>();

        [MenuItem("HECS Options/Helpers/Export/Export Container Window")]
        public static void GetExportContainerWindow()
        {
            GetWindow<ExportContainerWindow>();
        }

        [Button]
        public void Copy()
        {
            if (string.IsNullOrEmpty(CopyTo) || EntityContainer == null)
                return;

            DirectoryInfo lookingFor = new DirectoryInfo(Application.dataPath);

            foreach (var c in EntityContainer.Components)
            {
                var needed = c.GetHECSComponent.GetType().Name;

                var find = lookingFor.GetFiles(needed + ".cs", SearchOption.AllDirectories);

                foreach (var f in find)
                {
                    if (f.FullName.Contains("HECS"))
                        continue;

                    Components.Add(f);
                }
            }

            foreach (var c in EntityContainer.Systems)
            {
                var needed = c.GetSystem.GetType().Name;

                var find = lookingFor.GetFiles(needed + ".cs", SearchOption.AllDirectories);

                foreach (var f in find)
                {
                    if (f.FullName.Contains("HECS"))
                        continue;

                    Systems.Add(f);
                }
            }

            int count = 1;

            InstallHECS.CheckFolder(CopyTo + "/Components/");
            InstallHECS.CheckFolder(CopyTo + "/Systems/");
            var componentsCopyDirectory = new DirectoryInfo(CopyTo + "/Components/");
            var systemsCopyDirectory = new DirectoryInfo(CopyTo + "/Systems/");

            foreach (var c in Components)
            {
                if (componentsCopyDirectory.EnumerateFiles().Any(x => x.Name == c.Name))
                {
                    File.Copy(c.FullName, CopyTo + "/" + "/Components/" + count.ToString() + c.Name, true);
                    count++;
                    continue;
                }

                File.Copy(c.FullName, CopyTo + "/" + "/Components/" + c.Name);
            }

            foreach (var c in Systems)
            {
                if (systemsCopyDirectory.EnumerateFiles().Any(x => x.Name == c.Name))
                {
                    File.Copy(c.FullName, CopyTo + "/" + "/Systems/" + count.ToString() + c.Name, true);
                    count++;
                    continue;
                }

                File.Copy(c.FullName, CopyTo + "/" + "/Systems/" + c.Name);
            }
        }
    }
}