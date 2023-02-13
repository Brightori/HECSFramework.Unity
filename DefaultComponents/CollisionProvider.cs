using Commands;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    [DisallowMultipleComponent]
    public class CollisionProvider : MonoBehaviour, IHaveActor, IStartOnPooling
    {
        public Actor Actor { get; set; }

        public void Start()
        {
            if (Actor == null)
                Actor = GetComponent<Actor>();

            if (Actor == null)
                Actor = GetComponentInParent<Actor>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new CollisionCommand { Collision = collision });
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new Collision2dCommand { Collision = collision });
        }

        private void OnCollisionExit(Collision collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new CollisionExitCommand { Collision = collision });
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new Collision2dExitCommand { Collision = collision });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Actor.IsAlive())
                Actor.Command(new TriggerEnterCommand { Collider = other });
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new Trigger2dEnterCommand { Collider = collision });
        }

        private void OnTriggerExit(Collider other)
        {
            if (Actor.IsAlive())
                Actor.Command(new TriggerExitCommand { Collider = other });
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (Actor.IsAlive())
                Actor.Command(new Trigger2dExitCommand { Collider = collision });
        }
    }
}