using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace HECSFramework.Unity.Editor
{
    public class FindContainersByComponentsAndSystems : OdinEditorWindow
    {
        private BluePrintsProvider bluePrintsProvider = new BluePrintsProvider();
        private List<EntityContainer> entityContainers = new List<EntityContainer>();

        [ShowInInspector, OnValueChanged("UpdateFindedContainers"), ValueDropdown("ComponentsShow", IsUniqueList = false)]
        public List<Type> components = new List<Type>(6);
        [ShowInInspector, OnValueChanged("UpdateFindedContainers"), ValueDropdown("SystemsShow")]
        public List<Type> systems = new List<Type>(6);

        [ShowInInspector, HideIf("@containers.Count == 0")]
        public List<EntityContainer> containers = new List<EntityContainer>(16);

        [Button("Clear"), PropertySpace(20)]
        private void SystemsComponents()
        {
            components.Clear();
            systems.Clear();
            containers.Clear();
        }
        override protected void OnEnable()
        {
            base.OnEnable();

            entityContainers = AssetDatabase.FindAssets("t:EntityContainer")
                .Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
                .Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<EntityContainer>(x)).ToList();
        }

        private IEnumerable<Type> ComponentsShow()
        {
            return bluePrintsProvider.Components.Select(x => x.Key);
        }

        private IEnumerable<Type> SystemsShow()
        {
            return bluePrintsProvider.Systems.Select(x => x.Key);
        }

        private void UpdateFindedContainers()
        {
            containers.Clear();

            if (components.Count == 0 && systems.Count == 0)
                return;

            foreach (var e in entityContainers)
            {
                bool needed = true;

                if (components.Count != 0)
                    foreach (var c in components)
                    {
                        if (c == null)
                            continue;

                        if (e.Components.Any(x => x.GetHECSComponent.GetType() == c))
                            continue;

                        needed = false;
                        break;
                    }

                if (systems.Count != 0)
                    foreach (var s in systems)
                    {
                        if (e.Systems.Any(x => x.GetSystem.GetType() == s))
                            continue;

                        needed = false;
                        break;
                    }

                if (needed)
                    containers.Add(e);
            }
        }

        [MenuItem("HECS Options/Find containers", priority = 3)]
        static void FindContainers()
        {
            GetWindow<FindContainersByComponentsAndSystems>();
        }
    }
}