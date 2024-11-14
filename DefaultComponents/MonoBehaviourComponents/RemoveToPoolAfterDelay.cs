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

        public async void StartOnPooling()
        {
            await UniTask.Delay(delay.ToMilliseconds());
            EntityManager.GetSingleSystem<PoolingSystem>().ReleaseView(gameObject);
        }
    }
}