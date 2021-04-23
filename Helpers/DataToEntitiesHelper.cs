using HECSFramework.Core;
using System;
using System.Collections.Generic;

namespace HECSFramework.Unity
{
    public static class DataToEntitiesHelper
    {
        public static void FillEntitiesList<T>(T[] containers, ref List<IEntity> entities, int worldIndex = 0) where T: EntityContainer 
        {
            if (entities.Count > 0)
                return;

            for (int i = 0; i < containers.Length; i++)
            {
                var container = containers[i];
                var entity = new Entity(container.name, worldIndex);
                container.Init(entity);
                entities.Add(entity);
            }
        } 
        
        public static void FillEntitiesList<T>(T[] containers, ref IEntity[] entities, int worldIndex = 0) where T: EntityContainer 
        {
            entities = new IEntity[containers.Length];

            for (int i = 0; i < containers.Length; i++)
            {
                var container = containers[i];
                var entity = new Entity(container.name, worldIndex);
                container.Init(entity);
                entities[i] = entity;
            }
        }
    }
}
