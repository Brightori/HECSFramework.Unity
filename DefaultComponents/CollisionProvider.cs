using Commands;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    public class CollisionProvider : MonoBehaviour
    {
        private IActor actor;

        private void Awake()
        {
            actor = GetComponent<IActor>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            actor.Command(new CollisionCommand { Collision = collision });
        }

        private void OnTriggerEnter(Collider other)
        {
            //if (other.isTrigger) return;

            actor.Command(new TriggerEnterCommand { Collider = other });
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;

            actor.Command(new TriggerExitCommand { Collider = other });
        }
    }
}