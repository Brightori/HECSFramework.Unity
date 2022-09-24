namespace HECSFramework.Unity
{
    /// <summary>
    /// this is special interface for cases when we load location to new world and need start some things 
    /// after init root actors, bcz start event was alrdy passed and we need new one
    /// </summary>
    public interface IStartOnScene
    {
        void StartOnScene();
    }
}