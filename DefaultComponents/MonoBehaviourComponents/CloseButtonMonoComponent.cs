using Commands;
using HECSFramework.Unity;
using UnityEngine;
using UnityEngine.UI;

public class CloseButtonMonoComponent : MonoBehaviour, IHaveActor
{
    public Actor Actor { get; set; }

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(CloseButtonReact);
    }

    private void CloseButtonReact()
    {
        Actor.Command(new HideUICommand());
    }
}