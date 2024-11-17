using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
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

        public static bool TryGetClosestEntity2D(Collider2D[] colliders, Filter mask, Vector3 fromPosition, float radius, out Entity entity, int layermask = -1, Entity exclude = null)
        {
            entity = null;
            var dist = float.MaxValue;

            var amount = Physics2D.OverlapCircleNonAlloc(fromPosition, radius, colliders, layermask);

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

        public static int BoxRayCast(Vector3 center, Vector3 halfExtents, Vector3 direction, HECSList<Actor> targets, Filter filter, RaycastHit[] results, Quaternion orientation, float maxDistance, int layerMask)
        {
            targets.ClearFast();
            var result = Physics.BoxCastNonAlloc(center, halfExtents, direction, results, orientation, maxDistance, layerMask);

            for (int i = 0; i < result; i++)
            {
                if (results[i].collider.TryGetActorFromCollision(out var actor))
                {
                    if (actor.Entity.ContainsMask(filter))
                        targets.Add(actor);
                    else if (filter.Lenght == 0)
                        targets.Add(actor);
                }
            }

            return result;
        }
    }
}
