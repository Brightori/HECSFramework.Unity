using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Unity.Editor
{
    public class InstallHECS : UnityEditor.Editor
    {
        #region fields
        private static string Abilities = "/Abilities/";
        private static string Actors = "/Actors/";
        private static string BluePrints = "/BluePrints/";
        private static string Predicates = "/Predicates/";
        private static string Commands = "/Commands/";
        private static string Components = "/Components/";
        private static string Helpers = "/Helpers/";
        private static string Systems = "/Systems/";
        private static string Strategies = "/Strategies/";

        //BluePrintsFolders
        private static string AbilitiesBlueprints = "/AbilitiesBlueprints/";
        private static string PredicatesBlueprints = "/PredicatesBlueprints/";
        private static string ComponentsBluePrints = "/ComponentsBluePrints/";
        private static string SystemsBluePrint = "/SystemsBluePrint/";

        //MonoBehaviourComponents
        private static string MonoBehaviourComponents = "/MonoBehaviourComponents/";

        //Templates
        private static string ScriptTemplates = "/ScriptTemplates/";
        private static string ComponentsTemplate = "/81a-HECS__HECSComponent-Component.cs.txt";
        private static string SystemTemplate = "/82a-HECS__HECSSystem-System.cs.txt";
        private static string CommandTemplate = "/83a-HECS__HECSCommand-Command.cs.txt";
        private static string AbilityTemplate = "/84a-HECS__HECSAbility-Ability.cs.txt";
        private static string PredicateTemplate = "/85a-HECS__HECSPredicate-Predicate.cs.txt";

        private static string dataPath => Application.dataPath;
        private static string scriptPath => Application.dataPath + "/Scripts/";

        private static string ComponentID = "ComponentID.cs";
        private static string GameController = "GameController.cs";
        private static string RegisterService = "RegisterService.cs";
        private static string ReserveNamspaces = "ReserveNamespaces.cs";
        #endregion

        [MenuItem("HECS Options/Install HECS", priority = 10)]
        public static void InstallHECSFunc()
        {
            CreateFolders();
            CreateTemplates();
            //CreateGameController();
            //CreatePartialRegisterService();
            CreateReserveNamespace();
        }

        #region ReservedNameSpaces
        private static void CreateReserveNamespace()
        {
            var path = (scriptPath + Helpers + ReserveNamspaces).Replace("//", "/");

            if (File.Exists(path))
                return;

            var template = @"
namespace Components { }
namespace Systems { }
namespace Commands { }
namespace Abilities { }
namespace Predicates { }
namespace Helpers { }
namespace Actors { }
";
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }
        #endregion

        #region RegisterService
        private static void CreatePartialRegisterService()
        {
            var path = (scriptPath + Helpers + RegisterService).Replace("//", "/");

            if (File.Exists(path))
                return;

            var template = @"
namespace HECSFrameWork 
{
    using Components;
    using Commands;
    
    
    public partial class RegisterService 
    {
        private void BindSystem<T>(T system) where T : HECSFrameWork.Systems.ISystem 
        {
        }
    }
}
";
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }
        #endregion

        #region CreateFolders
        private static void CreateFolders()
        {
            CheckFolder(scriptPath);
            CheckFolder(scriptPath + Abilities);
            CheckFolder(scriptPath + Actors);
            CheckFolder(scriptPath + BluePrints);
            CheckFolder(scriptPath + Commands);
            CheckFolder(scriptPath + Components);
            CheckFolder(scriptPath + Helpers);
            CheckFolder(scriptPath + Systems);
            CheckFolder(scriptPath + Predicates);
            CheckFolder(scriptPath + Strategies);

            //ScriptBluePrintsFolders
            CheckFolder(scriptPath + BluePrints + AbilitiesBlueprints);
            CheckFolder(scriptPath + BluePrints + PredicatesBlueprints);
            CheckFolder(scriptPath + BluePrints + ComponentsBluePrints);
            CheckFolder(scriptPath + BluePrints + SystemsBluePrint);

            //BluePrintsFolders
            CheckFolder(dataPath + BluePrints);

            //MonoBehaviourComponents
            CheckFolder(scriptPath + Components + MonoBehaviourComponents);

            //ScriptTemplates
            CheckFolder(scriptPath + Components + MonoBehaviourComponents);
        }

        private static void CheckFolder(string path)
        {
            var folder = new DirectoryInfo(path);

            if (folder == null || !folder.Exists)
                Directory.CreateDirectory(path);
        }
        #endregion

        #region Templates
        private static void CreateTemplates()
        {
            CheckFolder(dataPath + ScriptTemplates);

            if (!File.Exists(dataPath + ScriptTemplates + ComponentsTemplate))
                CreateComponentsTemplate();

            if (!File.Exists(dataPath + ScriptTemplates + SystemTemplate))
                CreateSystemTemplate();

            if (!File.Exists(dataPath + ScriptTemplates + CommandTemplate))
                CreateCommandTemplate();

            if (!File.Exists(dataPath + ScriptTemplates + AbilityTemplate))
                CreateAbilityTemplate();

            if (!File.Exists(dataPath + ScriptTemplates + PredicateTemplate))
                CreatePredicateTemplate();
        }

        private static void CreatePredicateTemplate()
        {
            var template =
    @"using HECSFrameWork;
using HECSFrameWork.Components;
using Components;
using UnityEngine;

namespace Predicates
{
    [System.Serializable, BluePrint]
    public class #SCRIPTNAME# : BasePredicate
    {
        public override bool IsReady(IEntity target)
        {
            throw new System.NotImplementedException();
        }

        public override void LocalInit(IEntity owner)
        {
        }
    }
}";

            var path = (dataPath + ScriptTemplates + PredicateTemplate).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreateAbilityTemplate()
        {
            var template =
    @"using HECSFrameWork;
using HECSFrameWork.Components;
using Commands;
using Components;
using System;

namespace Abilities
{
    [Serializable, BluePrint]
    public class #SCRIPTNAME# : AbilityBase
    {
        public override void Execute<T>(T owner, T target = default, bool enable = true)
        {
        }

        protected override void LocalInit(IEntity actor)
        {
        }
    }
}";

            var path = (dataPath + ScriptTemplates + AbilityTemplate).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreateCommandTemplate()
        {
            var template =
    @"using HECSFrameWork.Components;
using HECSFrameWork;

namespace Commands
{
	public struct #SCRIPTNAME# : ICommand
	{
	}
}";


            var path = (dataPath + ScriptTemplates + CommandTemplate).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreateSystemTemplate()
        {
            var template =
    @"using System;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
	[Serializable, BluePrint]
    public class #SCRIPTNAME# : BaseSystem, I#SCRIPTNAME#
    {
        public override void InitSystem()
        {
        }
    }

    public interface I#SCRIPTNAME# : ISystem { }
}";


            var path = (dataPath + ScriptTemplates + SystemTemplate).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreateComponentsTemplate()
        {
            var path = (dataPath + ScriptTemplates + ComponentsTemplate).Replace("//", "/");

            if (File.Exists(path))
                return;

            var template =
    @"using HECSFramework.Core;
using HECSFramework.Unity;
using System;

namespace Components
{
    [Serializable, BluePrint]
    public class #SCRIPTNAME# : BaseComponent, I#SCRIPTNAME#
    {
       
    }

    public interface I#SCRIPTNAME# : IComponent
    {
    }
}";



            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }
        #endregion

        #region CreateComponentID
        private static void CreateComponentsID()
        {
            var path = (scriptPath + Components + ComponentID).Replace("//", "/");

            if (File.Exists(path))
                return;

            var template = @"
namespace HECSFrameWork.Components
{
    public enum ComponentID
    {
        Default,
        ActorContainerID,
        TransformComponentID,
        RectTransformComponentID,
        AbilityHolderComponentID,
		AnimationComponentID,
        ViewReferenceComponentID,
        UIViewReferenceComponentID,
        MainCanvasTagComponentID,
        UITagComponentID,
    }
}
";

            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }
        #endregion

        #region CreateGameController
        private static void CreateGameController()
        {
            var path = (scriptPath + GameController).Replace("//", "/");

            if (File.Exists(path))
                return;

            string template = @"
using HECSFrameWork;
using HECSFrameWork.Components;
using HECSFrameWork.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using Components;
using Systems;

[DefaultExecutionOrder(-50000)]
public class GameController : MonoBehaviour
{
    [SerializeField] private ActorContainer playerContainer = default;
    [SerializeField] private ActorContainer gameLogicContainer = default;
    [SerializeField] private ActorContainer uiManagerContainer = default;

    private EntityManager entityManager;
    private SaveManager saveManager;

    private IEntity gameLogic;
    private IEntity player;
    private IEntity battleSlot;
    private IEntity uiManager;

    public static bool IsLoad { get; private set; }

    private void Awake()
    {
        entityManager = new EntityManager();
        saveManager = new SaveManager();

        gameLogic = new Entity(""GameLogic"");
        player = new Entity(""Player"");
        uiManager = new Entity(""UiManager"");
    }


    private void InitEntities()
    {
        playerContainer.Init(player);
        gameLogicContainer?.Init(gameLogic);
        uiManagerContainer.Init(uiManager);

        uiManager.Init();
        gameLogic.Init();

        player.Init();
        player.GenerateId();
    }

    private void Start()
    {

    }

    [Button]
    private void Save() => GlobalCommander.Commander.Invoke(new SaveGlobalCommand());

    [Button]
    private void Load()
    {
        GlobalCommander.Commander.Invoke(new LoadGlobalCommand());
        IsLoad = true;
    }

    public static void LoadComplete()
    {
        IsLoad = false;
    }

    private void OnDisable()
    {
        entityManager?.Dispose();
        gameLogic.Dispose();
    }
}

";

            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }
        #endregion
    }
}