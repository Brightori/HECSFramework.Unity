using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using Systems;
using UnityEngine;

namespace Components
{
    [Documentation(Doc.Visual, "Return to pool after delay")]
    public class RemoveToPoolAfterDelay : MonoBehaviour, IStartOnPooling
    {
        [SerializeField] private float delay = 4;

        private bool alive = true;

        public async void StartOnPooling()
        {
            await UniTask.Delay(delay.ToMilliseconds());

            if (!alive)
                return;

            EntityManager.GetSingleSystem<PoolingSystem>().ReleaseView(gameObject);
        }

        private void OnDestroy()
        {
            alive = false;
        }
    }
}