using HECSFramework.Core;

namespace HECSFramework.Unity
{


    [Documentation(Doc.GameLogic, "Интерфейс которым помечаем монобех компоненты которые запускаем после смерти актора")]
    public interface IAfterLifeAction
    {
        void Action();
    }
}