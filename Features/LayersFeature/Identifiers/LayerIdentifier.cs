using HECSFramework.Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "LayerIdentifier", menuName = "Identifiers/LayerIdentifier")]
public class LayerIdentifier : IdentifierContainer
{
   public int LayerID;

    private void OnEnable()
    {
        LayerID = LayerMask.NameToLayer(name);
    }
}