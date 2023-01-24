using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HECSFramework.Core;

namespace HECSFramework.Unity
{
    public interface IPredicateEntityContainer
    {
        bool IsReady(EntityContainer target, Entity owner = null);
    }
}