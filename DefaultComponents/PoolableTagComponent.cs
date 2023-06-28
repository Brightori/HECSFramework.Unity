using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    public partial class PoolableTagComponent : BaseComponent, IInitAfterView, IAfterEntityInit
    {
        private IStartOnPooling[] startOnPoolings = Array.Empty<IStartOnPooling>();
        private IStopOnPooling[] stopOnPoolings = Array.Empty<IStopOnPooling>();

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

        public void InitAfterView()
        {
            GatherPoolables();
        }

        public void Reset()
        {
            startOnPoolings = Array.Empty<IStartOnPooling>();
            stopOnPoolings = Array.Empty<IStopOnPooling>();
        }
    }
}