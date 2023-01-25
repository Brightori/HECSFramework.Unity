#if UNITY_EDITOR
using HECSFramework.Core;
using HECSFramework.Core.Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace HECSFramework.Unity
{
    public class ChoosePresetActorWindow : OdinEditorWindow
    {
        [Searchable, AssetsOnly]
        public PresetContainer presetContainer;
        private bool inAction;
        private EntityContainer actorContainer;

        public void Init(EntityContainer actorContainer)
        {
            this.actorContainer = actorContainer;
        }

        [Button("Copy Preset ", ButtonSizes.Medium), HideIf("@presetContainer == null")]
        [PropertyTooltip("добавляем уникальные(актор будет иметь свобственный экземпляр блупринта) копии из пресета в актор")]
        public void Copy()
        {
            if (inAction)
                return;

            inAction = true;

            if (actorContainer is ActorReferenceContainer referenceContainer)
            {
                ReflectionHelpers.SetPrivateFieldValue(referenceContainer, "isInited", false);
            }

            foreach (var componentBP in presetContainer.ComponentsBluePrints)
            {
                if (actorContainer.IsHaveComponentBlueprint(componentBP))
                    continue;
                
                var asset = Instantiate(componentBP);
                AssetDatabase.AddObjectToAsset(asset, actorContainer);
                
                asset.name = componentBP.GetHECSComponent.GetType().Name;
                actorContainer.AddComponent(asset);
                HECSDebug.LogWarning($"we add component from preset {asset.name}");
            } 
            
            foreach (var sysBP in presetContainer.SystemsBluePrints)
            {
                if (actorContainer.IsHaveSystem(sysBP))
                    continue;

                var asset = Instantiate(sysBP);
                AssetDatabase.AddObjectToAsset(asset, actorContainer);
                
                asset.name = sysBP.GetSystem.GetType().Name;
                actorContainer.AddSystem(asset);
            }

            EditorUtility.SetDirty(actorContainer);
            AssetDatabase.SaveAssets();
            Close();
        }
        
        [Button("Replace container by preset", ButtonSizes.Medium), HideIf("@presetContainer == null")]
        public void ReplaceContainerByPreset()
        {
            if (inAction)
                return;
            
            actorContainer.Clear();
            AssetDatabase.SaveAssets();
            Copy();
        } 
        
        
        //[Button("Insert bluePrints from container", ButtonSizes.Medium), HideIf("@presetContainer == null")]
        //[PropertyTooltip("ссылки на эти объекты мы просто добавляем текущему актору, у акторов эти объекты будут общими")]
        //public void InsertContainerByPreset()
        //{
        //    if (inAction)
        //        return;

        //    inAction = true;

        //    foreach (var component in presetContainer.ComponentsBluePrints)
        //        actorContainer.AddComponent(component);

        //    foreach (var system in presetContainer.SystemsBluePrints)
        //        actorContainer.AddSystem(system);

        //    AssetDatabase.SaveAssets();
        //    Copy();
        //}
    }
}
#endif