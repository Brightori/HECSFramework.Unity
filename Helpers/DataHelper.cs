using HECSFramework.Serialize;
using UnityEngine;

namespace HECSFramework.Core
{
    public static partial class DataHelper
    {
        public static IEntity GetEntityCopy(IEntity entity)
        {
            var newEntity = new Entity(entity.ID, entity.WorldId);

            foreach (var c in entity.GetAllComponents)
            {
                if (c == null)
                    continue;
                
                if (TryGetComponentResolver(c.GetTypeHashCode, c, out HECSResolver hECSResolver))
                {
                    var newComp = TypesMap.GetComponentFromFactory(c.GetTypeHashCode);
                    newEntity.AddHecsComponent(newComp);
                    hECSResolver.To(newEntity);
                }
                else
                {
                    var data = UnityEngine.JsonUtility.ToJson(c);
                    var newCompFromJSON = JsonUtility.FromJson(data, TypesMap.GetTypeByComponentHECSHash(c.GetTypeHashCode));
                    newEntity.AddHecsComponent(newCompFromJSON as IComponent);
                }
            }

            foreach (var s in entity.GetAllSystems)
            {
                var newSys = TypesMap.GetSystemFromFactory(s.GetTypeHashCode);
                newEntity.AddHecsSystem(newSys);
            }

            return newEntity;
        }
    }
}