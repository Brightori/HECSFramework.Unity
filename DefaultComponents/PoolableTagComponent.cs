using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    public partial class PoolableTagComponent : BaseComponent, IInitAfterView, IAfterEntityInit
    {
        private IStartOnPooling[] startOnPoolings = new IStartOnPooling[0];
        private IStopOnPooling[] stopOnPoolings = new IStopOnPooling[0];

        private void GatherPoolables()
        {
            var actor = Owner.AsActor();

            if (actor!= null)
            {
                actor.TryGetComponents(out startOnPoolings);
                actor.TryGetComponents(out stopOnPoolings);
            }
        }

        public void StopOnPooling()
        {
            foreach (var sp in stopOnPoolings)
                sp.Stop();
        }

        public void StartOnPooling()
        {
            foreach(var sp in startOnPoolings)
                sp.Start();
        }

        public void AfterEntityInit()
        {
            if (!Owner.ContainsMask<SetupAfterViewTagComponent>())
                GatherPoolables();
        }

        public void InitAferView()
        {
            GatherPoolables();
        }
    }
}