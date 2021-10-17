using Commands;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    public class CollisionProvider : MonoBehaviour, IHaveActor
    {
        private void Awake()
        {
            Actor ??= GetComponent<IActor>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            Actor.Command(new CollisionCommand { Collision = collision });
        }

        private void OnCollisionExit(Collision collision)
        {
            Actor.Command(new CollisionExitCommand { Collision = collision });
        }

        private void OnTriggerEnter(Collider other)
        {
            Actor.Command(new TriggerEnterCommand { Collider = other });
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;

            Actor.Command(new TriggerExitCommand { Collider = other });
        }

        public IActor Actor { get; set; }
    }
}