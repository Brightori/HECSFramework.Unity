#if UNITY_EDITOR
using Sirenix.OdinInspector;
using HECSFramework.Unity.Editor;
#endif
using System.Collections.Generic;
using UnityEngine;


namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "ActorPresetContainer")]
    public class PresetContainer : EntityContainer
    {
        public List<ComponentBluePrint> ComponentsBluePrints => holder.components;
        public List<SystemBaseBluePrint> SystemsBluePrints => holder.systems;

#if UNITY_EDITOR
        [Button(ButtonSizes.Large)]
        public void MakePresetFromEntityContainer()
        {
            var window = UnityEditor.EditorWindow.GetWindow<MakePresetFromEntityContainer>();
            window.Init(this);
        }
#endif
    }
}

#if UNITY_EDITOR

namespace HECSFramework.Unity.Editor
{
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;

    public class MakePresetFromEntityContainer : OdinEditorWindow
    {
        [Searchable, AssetsOnly]
        public EntityContainer presetContainer;
        private EntityContainer currentContainer;

        private bool inAction;

        public void Init(EntityContainer actorContainer)
        {
            this.currentContainer = actorContainer;
        }

        [Button("Make clean copy", ButtonSizes.Medium), HideIf("@presetContainer == null")]
        public void ReplaceContainerByPreset()
        {
            if (inAction)
                return;

            currentContainer.Clear();
            AssetDatabase.SaveAssets();
            Copy();
        }

        [Button("Copy Unique Data to Preset ", ButtonSizes.Medium), HideIf("@presetContainer == null")]
        [PropertyTooltip("добавляем уникальные(актор будет иметь свобственный экземпляр блупринта) копии из пресета в актор")]
        public void Copy(bool close = true)
        {
            if (inAction)
                return;

            inAction = true;

            foreach (var componentBP in presetContainer.Components)
            {
                if (currentContainer.IsHaveComponentBlueprint(componentBP))
                    continue;

                var asset = Instantiate(componentBP);
                AssetDatabase.AddObjectToAsset(asset, currentContainer);

                asset.name = componentBP.GetHECSComponent.GetType().Name;
                currentContainer.AddComponent(asset);
            }

            foreach (var sysBP in presetContainer.Systems)
            {
                if (currentContainer.IsHaveSystem(sysBP))
                    continue;

                var asset = Instantiate(sysBP);
                AssetDatabase.AddObjectToAsset(asset, currentContainer);

                asset.name = sysBP.GetSystem.GetType().Name;
                currentContainer.AddSystem(asset);
            }

            AssetDatabase.SaveAssets();
            
            if (close)
                Close();
        }
    }
}

#endif
