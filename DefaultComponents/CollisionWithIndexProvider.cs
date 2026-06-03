using BluePrints.Identifiers;
using Commands;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    [DisallowMultipleComponent]
    [Documentation(Doc.Physics, "this component provider collision to actor, with index of collider")]
    public class CollisionWithIndexProvider : MonoBehaviour, IHaveActor, IStartOnPooling
    {
        public CollisionIdentifier CollisionIdentifier;
        public Actor Actor { get; set; }

        public void Start()
        {
            if (Actor == null)
                Actor = GetComponent<Actor>();

            if (Actor == null)
                Actor = GetComponentInParent<Actor>();
        }

        public void StartOnPooling()
        {
            if (Actor == null)
                Actor = GetComponent<Actor>();

            if (Actor == null)
                Actor = GetComponentInParent<Actor>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new CollisionIndexCommand { Index = CollisionIdentifier, Collision = collision });
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new Collision2dIndexCommand { Index = CollisionIdentifier, Collision = collision });
        }

        private void OnCollisionExit(Collision collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new CollisionExitIndexCommand { Index = CollisionIdentifier, Collision = collision });
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new Collision2dExitIndexCommand { Index = CollisionIdentifier, Collision = collision });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Actor.IsAlive())
                Actor.Command(new TriggerEnterIndexCommand { Index = CollisionIdentifier, Collider = other });
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new Trigger2dEnterIndexCommand { Index = CollisionIdentifier, Collider = collision });
        }

        private void OnTriggerExit(Collider other)
        {
            if (Actor.IsAlive())
                Actor.Command(new TriggerExitIndexCommand { Index = CollisionIdentifier, Collider = other });
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new Trigger2dExitIndexCommand { Index = CollisionIdentifier, Collider = collision });
        }
    }
}