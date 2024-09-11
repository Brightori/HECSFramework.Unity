using Components;
using HECSFramework.Core;
using Helpers;
using UnityEngine;

namespace Systems
{
    [Documentation(Doc.Abilities, Doc.HECS, Doc.Helpers, "this class contains some heplers usable in various object")]
    [Documentation(Doc.Abilities, Doc.HECS, Doc.Helpers, "TryGetClosestEntity return closest entity by mask and physics sphere overlap")]
    public static class HECSPhysicHelper
    {
        /// <summary>
        /// TryGetClosestEntity return closest entity by mask and physics sphere overlap
        /// </summary>
        /// <param name="colliders"></param>
        /// <param name="mask">u should provide </param>
        /// <param name="fromPosition"></param>
        /// <param name="radius"></param>
        /// <param name="entity"></param>
        /// <param name="exclude"></param>
        /// <returns></returns>
        public static bool TryGetClosestEntity(Collider[] colliders, Filter mask, Vector3 fromPosition, float radius, out Entity entity, int layermask = -1, Entity exclude = null)
        {
            entity = null;
            var dist = float.MaxValue;

            var amount = Physics.OverlapSphereNonAlloc(fromPosition, radius, colliders, layermask);

            for (int i = 0; i < amount; i++)
            {
                if (colliders[i].TryGetActorFromCollision(out var actor))
                {
                    if (actor.IsInited && actor.Entity != exclude)
                    {
                        if (actor.Entity.ContainsMask(mask))
                        {
                            if (actor.Entity.TryGetComponent(out UnityTransformComponent unityTransformComponent))
                            {
                                var currentDist = (actor.transform.position - fromPosition).sqrMagnitude;

                                if (dist > currentDist)
                                {
                                    entity = actor.Entity;
                                    dist = currentDist;
                                }
                            }
                        }
                    }
                }
            }

            return entity != null;
        }
    }
}
