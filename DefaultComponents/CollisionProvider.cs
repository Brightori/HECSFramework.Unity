using Commands;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    [DisallowMultipleComponent]
    public class CollisionProvider : MonoBehaviour, IHaveActor
    {
        public IActor Actor { get; set; }

        private void Start()
        {
            if (Actor == null)
                Actor = GetComponent<IActor>();

            if (Actor == null)
                Actor = GetComponentInParent<IActor>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            Actor.Command(new CollisionCommand { Collision = collision });
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Actor.Command(new Collision2dCommand { Collision = collision });
        }

        private void OnCollisionExit(Collision collision)
        {
            Actor.Command(new CollisionExitCommand { Collision = collision });
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            Actor.Command(new Collision2dExitCommand { Collision = collision });
        }

        private void OnTriggerEnter(Collider other)
        {
            Actor.Command(new TriggerEnterCommand { Collider = other });
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Actor.Command(new Trigger2dEnterCommand { Collider = collision });
        }

        private void OnTriggerExit(Collider other)
        {
            Actor.Command(new TriggerExitCommand { Collider = other });
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Actor.Command(new Trigger2dExitCommand { Collider = collision });
        }

        
    }
}