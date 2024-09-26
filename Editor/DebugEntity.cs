using HECSFramework.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;

namespace HECSFramework.Unity.Editor
{
    public class DebugEntityByID : OdinEditorWindow
    {
        [ShowInInspector]
        public int EntityID;

        public int WorldID;

        [OdinSerialize]
        private ActorPresentation ActorPresentation;


        [MenuItem("HECS Options/Debug/Draw Entity by ID")]
        public static void ShowWindow()
        {
            GetWindow<DebugEntityByID>();
        }

        [Button]
        public void UpdateEntityInfo()
        {
            ActorPresentation = new ActorPresentation(EntityManager.Worlds[WorldID].Entities[EntityID]);
            ActorPresentation.UpdateIfo();
        }
    }
}