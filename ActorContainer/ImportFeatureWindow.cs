#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace HECSFramework.Unity
{
    public class ImportFeatureWindow : OdinEditorWindow
    {
        [ValueDropdown(nameof(GetFeatures))]
        public string Feature;

        private Dictionary<string, HashSet<Type>> componentsByFeature = new(128);
        private Dictionary<string, HashSet<Type>> systemsByFeature = new (128);
        private HashSet<string> featuresByFeature = new();
        private EntityContainer entityContainer;

        protected override void OnEnable()
        {
            var provider = new BluePrintsProvider();

            foreach (var component in provider.Components) 
            { 
                foreach (var a in component.Key.GetCustomAttributes(typeof(FeatureAttribute), true))
                {
                    if (a is FeatureAttribute feature)
                    {
                        foreach (var currentFeature in feature.Features)
                        {
                            featuresByFeature.Add(currentFeature);

                            if (componentsByFeature.ContainsKey(currentFeature))
                            {
                                componentsByFeature[currentFeature].Add(component.Key);
                            }
                            else
                            {
                                componentsByFeature.Add(currentFeature, new HashSet<Type>() { component.Key });
                            }
                        }
                    }
                }
            }

            foreach (var system in provider.Systems)
            {
                foreach (var a in system.Key.GetCustomAttributes(typeof(FeatureAttribute), true))
                {
                    if (a is FeatureAttribute feature)
                    {
                        foreach (var currentFeature in feature.Features)
                        {
                            featuresByFeature.Add(currentFeature);

                            if (systemsByFeature.ContainsKey(currentFeature))
                            {
                                systemsByFeature[currentFeature].Add(system.Key);
                            }
                            else
                            {
                                systemsByFeature.Add(currentFeature, new HashSet<Type>() { system.Key });
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<string> GetFeatures()
        {
            return featuresByFeature;
        }


        public void Init(EntityContainer container)
        {
            entityContainer = container;
        }

        [Button]
        public void AddFeature()
        {
            if (string.IsNullOrEmpty(Feature))
                return;

            var provider = new BluePrintsProvider();

            if (componentsByFeature.TryGetValue(Feature, out var components)) 
            {
                foreach (var key in components)
                {
                    var componentNode = new ComponentBluePrintNode(key.Name, provider.Components[key], new List<EntityContainer> { entityContainer });
                    componentNode.AddBluePrint();
                    Debug.LogWarning($"we add {key.Name}");
                }
            }

            if (systemsByFeature.TryGetValue(Feature, out var systems))
            {
                foreach (var key in systems)
                {
                    var componentNode = new SystemBluePrintNode(key.Name, provider.Systems[key], new List<EntityContainer> { entityContainer });
                    componentNode.AddBluePrint();
                    Debug.LogWarning($"we add {key.Name}");
                }
            }
        }
    }
}
#endif