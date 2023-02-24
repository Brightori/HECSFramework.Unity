using System.Collections.Generic;
using UnityEngine;
using HECSFramework.Unity;

namespace HECSFramework.Core
{
    public static partial class DataHelper
    {
        public static bool FillEntitiesList<T>(T[] containers, ref List<Entity> entities, Entity owner, int worldIndex = 0) where T: EntityContainer 
        {
            if (entities.Count > 0)
                return false;

            foreach (T container in containers)
            {
                if (container == null)
                {
                    HECSDebug.LogWarning($"пустой контейнер у {owner.ID}");
                    continue;
                }

                var entity = Entity.Get(EntityManager.Worlds[worldIndex], container.name);
                entity.GenerateGuid();
                container.Init(entity);
                entities.Add(entity);
            }

            return true;
        }  
        
        public static void FillEntitiesList<T>(List<T> containers, ref List<Entity> entities, Entity owner, int worldIndex = 0) where T: EntityContainer 
        {
            if (entities.Count > 0)
                return;

            for (int i = 0; i < containers.Count; i++)
            {
                if (containers[i] == null)
                {
                    Debug.LogAssertion("пустой контейнер у " + owner.ID);
                    continue;
                }

                var container = containers[i];
                var entity = Entity.Get(EntityManager.Worlds[worldIndex], container.name);
                entity.GenerateGuid();
                container.Init(entity);
                entities.Add(entity);
            }
        } 
        
        public static void FillEntitiesList<T>(T[] containers, ref Entity[] entities, Entity owner, int worldIndex = 0) where T: EntityContainer 
        {
            entities = new Entity[containers.Length];

            for (int i = 0; i < containers.Length; i++)
            {
                if (containers[i] == null)
                {
                    Debug.LogAssertion("пустой контейнер у " + owner.ID);
                    continue;
                }

                var container = containers[i];
                var entity = Entity.Get(EntityManager.Worlds[worldIndex], container.name);
                entity.GenerateGuid();
                container.Init(entity);
                entities[i] = entity;
            }
        }
    }
}
