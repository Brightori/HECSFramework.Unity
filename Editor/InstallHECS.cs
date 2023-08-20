using System;
using System.IO;
using System.Text;
using HECSFramework.Core;
using HECSFramework.Core.Generator;
using Strategies;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HECSFramework.Unity.Editor
{
    public class InstallHECS : UnityEditor.Editor
    {
        #region fields
        private static string Abilities = "/Abilities/";
        private static string Actors = "/Actors/";
        public static string BluePrints = "/BluePrints/";
        private static string Predicates = "/Predicates/";
        private static string Commands = "/Commands/";
        private static string Components = "/Components/";
        private static string Helpers = "/Helpers/";
        private static string Systems = "/Systems/";
        private static string Strategies = "/Strategies/";
        private static string Content = "/Content/";
        private static string History = "/History/";
        private static string AddressablesGroupsHistory = "/AddressablesGroupsHistory/";
        public readonly static string HECSGenerated = "/HECSGenerated/";

        //BluePrintsFolders
        public static string Identifiers = "/Identifiers/";
        public static string Actions = "/Actions/";
        private static string GlobalEntities = "/GlobalEntities/";
        private static string Presets = "/Presets/";
        public static string UIBluePrints = "/UIBluePrints/";

        //Identifiers
        public static string UIIdentifiers = "/UIIdentifiers/";

        //ScriptBluePrintsFolders
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
        private static string ActionTemplate = "/84b-HECS__Action-Action.cs.txt";
        private static string PredicateTemplate = "/85a-HECS__Predicate-Predicate.cs.txt";
        private static string InterDecision = "/86a-HECS__InterDecision-InterDecision.cs.txt";
        private static string DilemmaDecision = "/87a-HECS__DilemmaDecision-DilemmaDecision.cs.txt";
        private static string GenericNode = "/88a-HECS__GenericNode-GenericNode.cs.txt";

        public static string DataPath => Application.dataPath;
        public static string ScriptPath => Application.dataPath + "/Scripts/";
        public static string Assets => "Assets/";

        private static string ComponentID = "ComponentID.cs";
        private static string GameController = "GameController.cs";
        private static string RegisterService = "RegisterService.cs";
        private static string ReserveNamspaces = "ReserveNamespaces.cs";
        #endregion

        [MenuItem("HECS Options/Install HECS", priority = 1)]
        public static void InstallHECSFunc()
        {
            CreateFolders();
            CreateTemplates();
            CreateGameController();
            //CreatePartialRegisterService();
            CreateReserveNamespace();
        }

        public static void SaveToFile(string data, string path)
{
            File.WriteAllText(path, data, Encoding.UTF8);
        }

        #region ReservedNameSpaces
        private static void CreateReserveNamespace()
        {
            var path = (ScriptPath + Helpers + ReserveNamspaces).Replace("//", "/");

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
            var path = (ScriptPath + Helpers + RegisterService).Replace("//", "/");

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
            CheckFolder(ScriptPath);
            CheckFolder(ScriptPath + Actors);
            CheckFolder(ScriptPath + BluePrints);
            CheckFolder(ScriptPath + Commands);
            CheckFolder(ScriptPath + Components);
            CheckFolder(ScriptPath + Helpers);
            CheckFolder(ScriptPath + Systems);
            CheckFolder(ScriptPath + Predicates);
            CheckFolder(ScriptPath + Strategies);
            CheckFolder(ScriptPath + HECSGenerated);

            //ScriptBluePrintsFolders
            CheckFolder(ScriptPath + BluePrints + AbilitiesBlueprints);
            CheckFolder(ScriptPath + BluePrints + PredicatesBlueprints);
            CheckFolder(ScriptPath + BluePrints + ComponentsBluePrints);
            CheckFolder(ScriptPath + BluePrints + SystemsBluePrint);
            CheckFolder(ScriptPath + BluePrints + Actions);
            CheckFolder(ScriptPath + BluePrints + Identifiers);

            //BluePrintsFolders
            CheckFolder(DataPath + BluePrints);
            CheckFolder(DataPath + BluePrints + Identifiers);
            CheckFolder(DataPath + BluePrints + Identifiers + UIIdentifiers);
            CheckFolder(DataPath + BluePrints + Abilities);
            CheckFolder(DataPath + BluePrints + GlobalEntities);
            CheckFolder(DataPath + BluePrints + Strategies);
            CheckFolder(DataPath + BluePrints + Presets);
            CheckFolder(DataPath + BluePrints + UIBluePrints);
            CheckFolder(DataPath + BluePrints + Actions);
            CheckFolder(DataPath + BluePrints + History);
            CheckFolder(DataPath + BluePrints + History+AddressablesGroupsHistory);

            //MonoBehaviourComponents
            CheckFolder(ScriptPath + Components + MonoBehaviourComponents);

            //ScriptTemplates
            CheckFolder(ScriptPath + Components + MonoBehaviourComponents);

            //Content
            CheckFolder(DataPath + Content);
        }

        public static void CheckFolder(string path)
        {
            var folder = new DirectoryInfo(path);

            if (folder == null || !folder.Exists)
                Directory.CreateDirectory(path);
        }
        #endregion

        #region Templates
        private static void CreateTemplates()
        {
            CheckFolder(DataPath + ScriptTemplates);

            if (!File.Exists(DataPath + ScriptTemplates + ComponentsTemplate))
                CreateComponentsTemplate();

            if (!File.Exists(DataPath + ScriptTemplates + SystemTemplate))
                CreateSystemTemplate();

            if (!File.Exists(DataPath + ScriptTemplates + CommandTemplate))
                CreateCommandTemplate();

            if (!File.Exists(DataPath + ScriptTemplates + AbilityTemplate))
                CreateAbilityTemplate();

            if (!File.Exists(DataPath + ScriptTemplates + PredicateTemplate))
                CreatePredicateTemplate();

            if (!File.Exists(DataPath + ScriptTemplates + ActionTemplate))
                CreateActionTemplate();
            
            if (!File.Exists(DataPath + ScriptTemplates + InterDecision))
                CreateInterDecisionTemplate();

            if (!File.Exists(DataPath + ScriptTemplates + DilemmaDecision))
                CreateDilemmaDecisionTemplate();
            
            if (!File.Exists(DataPath + ScriptTemplates + GenericNode))
                CreateGenericNodeTemplate();
        }

        private static void CreateGenericNodeTemplate()
        {
            var template =
@"using HECSFramework.Core;

namespace Strategies
{
    [Documentation(Doc.Strategy, """")]
    public class #SCRIPTNAME# : GenericNode<T>
        {
            public override string TitleOfNode { get; } = ""#SCRIPTNAME#"";

            [Connection(ConnectionPointType.Out, "" <> Out"")]
        public BaseDecisionNode Out;

            public override void Execute(Entity entity)
            {
                throw new System.NotImplementedException();
            }

            public override T Value(Entity entity)
            {
                throw new System.NotImplementedException();
            }
        }
    }
";

            var path = (DataPath + ScriptTemplates + GenericNode).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreateDilemmaDecisionTemplate()
        {
            var template =
 @"using HECSFramework.Core;
using Strategies;


namespace Strategies
{
    [Documentation(Doc.NONE, """")]
    public class #SCRIPTNAME# : DilemmaDecision
    {
        public override string TitleOfNode { get; } = ""#SCRIPTNAME#"";

        protected override void Run(Entity entity)
        {
            if (true)
            {
                Positive.Execute(entity);
                return;
            }
            else
            {
                Negative.Execute(entity);
                return;
            }
        }
    }
}
";

            var path = (DataPath + ScriptTemplates + DilemmaDecision).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreateInterDecisionTemplate()
        {
            var template =
  @"using HECSFramework.Core;


namespace Strategies
{
    [Documentation(Doc.Strategy, ""#SCRIPTNAME#"")]
    public class #SCRIPTNAME# : InterDecision
    {
        public override string TitleOfNode { get; } = ""#SCRIPTNAME#"";

        protected override void Run(Entity entity)
        {

           Next.Execute(entity);
        }
    }
}
";

            var path = (DataPath + ScriptTemplates + InterDecision).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreateActionTemplate()
        {
            var template =
   @"using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Action, """")]
    public sealed class #SCRIPTNAME# : IAction
    {
        public void Action(Entity entity)
        {
            throw new System.NotImplementedException();
        }
    }
}";

            var path = (DataPath + ScriptTemplates + ActionTemplate).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreatePredicateTemplate()
        {
            var template =
    @"using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using System;

namespace Predicates
{
    [Serializable][Documentation(Doc.NONE, """")]
    public sealed class #SCRIPTNAME# : IPredicate
    {
        public bool IsReady(Entity target, Entity owner = null)
        {
            return false;
        }
    }
}";

            var path = (DataPath + ScriptTemplates + PredicateTemplate).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreateAbilityTemplate()
        {
            var template =
    @"using System;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Abilities, """")]
    public sealed class #SCRIPTNAME# : BaseAbilitySystem
    {
        public override void Execute(Entity owner = null, Entity target = null, bool enable = true)
        {
            throw new NotImplementedException();
        }

        public override void InitSystem()
        {
            throw new NotImplementedException();
        }
    }
}";

            var path = (DataPath + ScriptTemplates + AbilityTemplate).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreateCommandTemplate()
        {
            var template =
    @"using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.NONE, """")]
	public struct #SCRIPTNAME# : ICommand
	{
	}
}";


            var path = (DataPath + ScriptTemplates + CommandTemplate).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreateSystemTemplate()
        {
            var template =
    @"using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;

namespace Systems
{
	[Serializable][Documentation(Doc.NONE, """")]
    public sealed class #SCRIPTNAME# : BaseSystem 
    {
        public override void InitSystem()
        {
        }
    }
}";


            var path = (DataPath + ScriptTemplates + SystemTemplate).Replace("//", "/");
            File.WriteAllText(path, template, Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static void CreateComponentsTemplate()
        {
            var path = (DataPath + ScriptTemplates + ComponentsTemplate).Replace("//", "/");

            if (File.Exists(path))
                return;

            var template =
    @"using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.NONE, """")]
    public sealed class #SCRIPTNAME# : BaseComponent
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
            var path = (ScriptPath + Components + ComponentID).Replace("//", "/");

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
            var path = (ScriptPath + GameController).Replace("//", "/");

            if (File.Exists(path))
                return;

            var template = new TreeSyntaxNode();
            template.Add(new UsingSyntax("System.Collections.Generic"));
            template.Add(new UsingSyntax("HECSFramework.Core"));
            template.Add(new UsingSyntax("UnityEngine", 1));
            template.Add(new NameSpaceSyntax("HECSFramework.Unity"));
            template.Add(new LeftScopeSyntax());
            template.Add(new TabSimpleSyntax(1, "public class GameController : BaseGameController"));
            template.Add(new LeftScopeSyntax(1));
            template.Add(new TabSimpleSyntax(2, "[SerializeField] private ActorContainer[] additionalGlobalEntities = default;"));
            template.Add(new TabSimpleSyntax(2, "private List<Entity> additionalEntities = new List<Entity>(8);"));
            template.Add(new ParagraphSyntax());
            template.Add(new TabSimpleSyntax(2, "public override void BaseAwake()"));
            template.Add(new LeftScopeSyntax(2));
            template.Add(GetAwakeBody());
            template.Add(new RightScopeSyntax(2));
            template.Add(new ParagraphSyntax());
            template.Add(new TabSimpleSyntax(2, "public override void BaseStart()"));
            template.Add(new LeftScopeSyntax(2));
            template.Add(StartBody());
            template.Add(new RightScopeSyntax(2));
            template.Add(new RightScopeSyntax(1));
            template.Add(new RightScopeSyntax());

            File.WriteAllText(path, template.ToString(), Encoding.UTF8);

            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(path);
        }

        private static ISyntax GetAwakeBody()
        {
            var body = new TreeSyntaxNode();
            body.Add(new TabSimpleSyntax(3, "foreach (var a in additionalGlobalEntities)"));
            body.Add(new LeftScopeSyntax(3));
            body.Add(new TabSimpleSyntax(4, "var additionlEntity = Entity.Get(a.name);"));
            body.Add(new TabSimpleSyntax(4, "a.Init(additionlEntity);"));
            body.Add(new TabSimpleSyntax(4, "additionalEntities.Add(additionlEntity);"));
            body.Add(new RightScopeSyntax(3));
            return body;
        }

        private static ISyntax StartBody()
        {
            var body = new TreeSyntaxNode();

            body.Add(new TabSimpleSyntax(4, "foreach (var a in additionalEntities)"));
            body.Add(new TabSimpleSyntax(5, "a.Init();"));

            return body;
        }

        #endregion
    }
}