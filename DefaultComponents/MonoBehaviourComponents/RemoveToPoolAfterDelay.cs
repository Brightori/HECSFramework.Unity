using HECSFramework.Core;
using Systems;
using UnityEngine;

public class RemoveToPoolAfterDelay : MonoBehaviour, IStartOnPooling
{

    [SerializeField] private float delay = 4;
    private float currentDelay;

    public void Start()
    {
        currentDelay = delay;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentDelay <= 0)
        {
            EntityManager.GetSingleSystem<PoolingSystem>().ReleaseView(gameObject);
        }
        else
            currentDelay -= Time.deltaTime;
    }
}
