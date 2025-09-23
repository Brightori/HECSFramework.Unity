using Components;
using HECSFramework.Core;
using UnityEngine;

public delegate bool EntityPredicate (Entity entity); 

namespace HECSFramework.Unity
{
    [Documentation(Doc.Helpers, Doc.HECS, "here we hold various popular operations around entities, like find closest")]
    public static partial class EntitiesHelper
    {
        public static Entity GetClosestEntity(Vector3 pos, EntitiesFilter entities)
        {
            float dist = float.MaxValue;
            int index = -1;

            for (int i = 0; i < entities.Count; i++)
            {
                var sqrMagnitude = (entities[i].GetPosition() - pos).sqrMagnitude;

                if (sqrMagnitude < dist)
                {
                    dist = sqrMagnitude;
                    index = i;
                }
            }

            if (index == -1)
                return null;

            return entities[index];
        }

        public static Entity GetClosestEntity(Vector3 pos, EntitiesFilter entities, EntityPredicate entityPredicate)
        {
            float dist = float.MaxValue;
            int index = -1;

            for (int i = 0; i < entities.Count; i++)
            {
                var sqrMagnitude = (entities[i].GetPosition() - pos).sqrMagnitude;

                if (sqrMagnitude < dist && entityPredicate(entities[i]))
                {
                    dist = sqrMagnitude;
                    index = i;
                }
            }

            if (index == -1)
                return null;

            return entities[index];
        }
    }
}
