using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.HECS, Doc.Layer, Doc.HECS, "this component holds layer identifiers and can return unity index of layer")]
    public sealed class LayerProviderComponent : BaseComponent
    {
        public LayerIdentifier[] Layers; 

        /// <summary>
        /// here we return unity index of layer
        /// </summary>
        /// <param name="layerIdentifier"></param>
        /// <returns></returns>
        public int GetLayerIndex(int layerIdentifier)
        {
            for (int i = 0; i < Layers.Length; i++)
            {
                if (Layers[i] == layerIdentifier)
                    return Layers[i].LayerID;
            }

            throw new Exception("we dont have such id in layers " + layerIdentifier);
        }
    }
}