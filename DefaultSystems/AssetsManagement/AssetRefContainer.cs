using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace AssetsManagement.Containers
{
    public class AssetRefContainer<TRef, TObject> : IAssetContainer<TObject>
        where TRef : AssetReference
        where TObject : Object 
    {
        protected readonly TObject asset;
        protected readonly TRef reference;

        public TObject Asset => asset;
        public TRef Reference => reference;

        public AssetRefContainer(TRef loadedReference)
        {
            asset = loadedReference.Asset as TObject;
            reference = loadedReference;
        }
    }
}