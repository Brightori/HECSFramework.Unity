using System;
using System.Collections.Generic;
using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity.Editor
{
    [Serializable]
    public struct History
    {
        public string Name;
        public long DateTimeSave;

        public List<ComponentHistory> componentHistories;
        public List<SystemHistory> systemHistories;

        public History(EntityContainer entityContainer)
        {
            componentHistories = new List<ComponentHistory>();
            systemHistories = new List<SystemHistory>();
            DateTimeSave = DateTime.Now.ToBinary();
            Name = entityContainer.name;

            foreach (var c in entityContainer.Components)
                componentHistories.Add(new ComponentHistory(c));

            foreach (var c in entityContainer.Systems)
                systemHistories.Add(new SystemHistory(c));
        }
    }

    [Serializable]
    public struct ComponentHistory
    {
        public int Index;
        public string Name;
        public string JSON;

        public ComponentHistory(ComponentBluePrint componentBluePrint)
        {
            Name = componentBluePrint.GetHECSComponent.GetType().Name;
            Index = IndexGenerator.GenerateIndex(Name);
            JSON = JsonUtility.ToJson(componentBluePrint, true);
        }
    }

    [Serializable]
    public struct SystemHistory
    {
        public int Index;
        public string Name;
        public string JSON;

        public SystemHistory(SystemBaseBluePrint componentBluePrint)
        {
            Name = componentBluePrint.GetSystem.GetType().Name;
            Index = componentBluePrint.GetSystem.GetTypeHashCode;
            JSON = JsonUtility.ToJson(componentBluePrint, true);
        }
    }
}