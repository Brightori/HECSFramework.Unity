using Sirenix.OdinInspector;
using UnityEngine;

public class AnimatorParameterIdentifier : ScriptableObject
{
    [ReadOnly]
    [SerializeField]private int id;

    public int Id { get => id; private set => id = value; }

    private void OnEnable()
    {
        id = Animator.StringToHash(name);
    }
}