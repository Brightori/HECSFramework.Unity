using Commands;
using HECSFramework.Core;
using HECSFramework.Core.Generator;
using HECSFramework.Unity.Editor;
using HECSFramework.Unity.Helpers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorControllerParameterType = UnityEngine.AnimatorControllerParameterType;
using AnimatorState = UnityEditor.Animations.AnimatorState;
using BlendTree = UnityEditor.Animations.BlendTree;

namespace HECSFramework.Unity
{
    [Documentation(Doc.HECS, Doc.Animation, Doc.Editor, "Window for generate Animator helpers")]
    public class BakeAnimatorsWindow : OdinEditorWindow
    {
        private const string AnimatorHelperSave = "AnimatorHelper.cs";
        private string FilePath => InstallHECS.ScriptPath + InstallHECS.HECSGenerated + AnimatorHelperSave;
        private string StateBluePrintsPath => "Assets/" + InstallHECS.BluePrints + InstallHECS.Identifiers + "AnimatorStateIdentifiers/";
        private string AnimParametersBluePrintsPath => InstallHECS.BluePrints + InstallHECS.Identifiers + "AnimatorParametersIdentifiers/";

        private const string SaveAnimatorHelpers = "SaveAnimatorHelpers";
        private const string EmptyAnimators = "SaveAnimatorHelpers";
        private const string SaveSetAnimatorsState = "SetAnimatorStateHelper.cs";
        private const char Split = '|';
        private const string AnimParametersMap = "AnimParametersMap.cs";

        private HashSet<string> animParameters = new HashSet<string>(16);
        private HashSet<string> animStates = new HashSet<string>(16);


        [OnValueChanged("UpdateAnimators")]
        public AnimatorController[] animators = new AnimatorController[0];

        [BoxGroup("Settings")]
        [HorizontalGroup("Settings/Split", Width = 100, LabelWidth = 100)]
        [LabelText("Serialization")]
        [OnInspectorInit("@SerializationAnimatorState")]
        public bool SerializationAnimatorState
        {
            get => PlayerPrefs.GetInt(nameof(SerializationAnimatorState), 0) == 1;
            set => PlayerPrefs.SetInt(nameof(SerializationAnimatorState), value ? 1 : 0);
        }

        [LabelText("| Networking")]
        [HorizontalGroup("Settings/Split/Next", Width = 100, LabelWidth = 100)]
        [OnInspectorInit("@AnimatorStateNetworking")]
        public bool AnimatorStateNetworking
        {
            get => PlayerPrefs.GetInt(nameof(AnimatorStateNetworking), 0) == 1;
            set => PlayerPrefs.SetInt(nameof(AnimatorStateNetworking), value ? 1 : 0);
        }

        [MenuItem("HECS Options/Animations/BakeAnimationHelper")]
        public static void BakeAnimationsWindow()
        {
            GetWindow<BakeAnimatorsWindow>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            var saveAnimators = PlayerPrefs.GetString(SaveAnimatorHelpers, EmptyAnimators);

            var list = new List<AnimatorController>();

            if (saveAnimators != EmptyAnimators)
            {
                var savePaths = saveAnimators.Split(Split);

                foreach (var s in savePaths)
                {
                    if (string.IsNullOrEmpty(s)) continue;

                    var assetController = AssetDatabase.LoadAssetAtPath<AnimatorController>(s);
                    list.Add(assetController);
                }
            }

            animators = list.ToArray();
            InstallHECS.CheckFolder(Application.dataPath + AnimParametersBluePrintsPath);
        }

        private void UpdateAnimators()
        {
            var stringAnimators = string.Empty;

            foreach (var a in animators)
            {
                var path = AssetDatabase.GetAssetPath(a);
                stringAnimators += (Split + path);
            }

            PlayerPrefs.SetString(SaveAnimatorHelpers, stringAnimators);
            PlayerPrefs.Save();
        }


        [PropertySpace(20)]
        [Button(ButtonSizes.Large)]
        private void GatherAnimators()
        {
            var getAnimators = new SOProvider<AnimatorController>().GetCollection().ToHashSet();
            var clearList = new List<AnimatorController>(32);

            foreach (var ga in getAnimators)
            {
                if (clearList.Any(x => x.name == ga.name))
                {
                    Debug.LogError($"we have animator controller with same name {ga.name}");
                    continue;
                }

                clearList.Add(ga);
            }

            animators = clearList.ToArray();
        }

        [PropertySpace(20)]
        [Button(ButtonSizes.Large)]
        private void GenerateAnimatorHelpers()
        {
            var tree = new TreeSyntaxNode();
            var fields = new TreeSyntaxNode();
            var constructor = new TreeSyntaxNode();
            var initMethods = new TreeSyntaxNode();

            if (SerializationAnimatorState)
            {
                tree.Add(new UsingSyntax("HECSFramework.Serialize"));
            }

            tree.Add(new UsingSyntax("System.Collections.Generic", 1));

            tree.Add(new NameSpaceSyntax("HECSFramework.Unity"));
            tree.Add(new LeftScopeSyntax());
            tree.Add(new TabSimpleSyntax(1, "public static partial class AnimatorManager"));
            tree.Add(new LeftScopeSyntax(1));

            tree.Add(fields);
            tree.Add(new ParagraphSyntax());

            tree.Add(new TabSimpleSyntax(2, "static AnimatorManager()"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(constructor);
            tree.Add(new RightScopeSyntax(2));
            tree.Add(new ParagraphSyntax());
            tree.Add(initMethods);

            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax());


            foreach (var a in animators)
            {
                var animHelper = nameof(AnimatorHelper);
                fields.Add(new TabSimpleSyntax(2, $"public static {animHelper} {a.name};"));
                constructor.Add(new TabSimpleSyntax(3, $"Init{a.name}();"));
                initMethods.Add(GetInitMethod(a));
            }

            File.WriteAllText(FilePath, tree.ToString(), Encoding.UTF8);
            GenerateParametersAndAnimatorHelpers();
        }

        private void GenerateParametersAndAnimatorHelpers()
        {
            var mapPath = InstallHECS.ScriptPath + InstallHECS.HECSGenerated + "AnimParametersMap.cs";
            var animatorStateHashFile = InstallHECS.ScriptPath + InstallHECS.HECSGenerated + "AnimatorHashStatesMap.cs";
            var animatorStatesPath = InstallHECS.ScriptPath + InstallHECS.HECSGenerated + SaveSetAnimatorsState;

            var animatorStates = new TreeSyntaxNode();
            var tree = new TreeSyntaxNode();
            var bodyOfDic = new TreeSyntaxNode();
            var parametersMapBody = new TreeSyntaxNode();

            tree.Add(new UsingSyntax("System.Collections.Generic", 2));
            tree.Add(new TabSimpleSyntax(0, "public static partial class AnimParametersMap"));
            tree.Add(new LeftScopeSyntax());

            tree.Add(new TabSimpleSyntax(1, "static AnimParametersMap()"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(new TabSimpleSyntax(2, "AnimParameters = new Dictionary<string, int>"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(bodyOfDic);
            tree.Add(new RightScopeSyntax(2, true));
            tree.Add(new RightScopeSyntax(1));


            tree.Add(parametersMapBody);
            tree.Add(new RightScopeSyntax());

            var getAnimatorStatesBody = GetSetAnimatorStatesCore(animatorStates);

            foreach (var a in animators)
            {
                if (SerializationAnimatorState)
                    PutAnimatorStateCommandSyntax(getAnimatorStatesBody, a);

                foreach (var p in a.parameters)
                {
                    GenerateAnimationParameterIdentifier(p.name);
                    parametersMapBody.AddUnique(new TabSimpleSyntax(1, $"public static readonly int {p.name} = {Animator.StringToHash(p.name)};"));
                    bodyOfDic.AddUnique(new TabSimpleSyntax(3, $"{CParse.LeftScope}{CParse.Quote}{p.name}{CParse.Quote}, {Animator.StringToHash(p.name)}{CParse.RightScope},"));
                }
            }

            if (getAnimatorStatesBody.Tree.Count > 0 && getAnimatorStatesBody.Tree.Last() is ParagraphSyntax)
            {
                getAnimatorStatesBody.Tree.Remove(getAnimatorStatesBody.Tree.Last());
            }

            if (animatorStates.Tree.Count > 0 && animatorStates.Tree.Last() is ParagraphSyntax)
            {
                animatorStates.Tree.Remove(animatorStates.Tree.Last());
            }

            SaveFileHelper.SaveToFileIfExists(tree.ToString(), InstallHECS.ScriptPath + InstallHECS.HECSGenerated, AnimParametersMap);

            if (SerializationAnimatorState)
                File.WriteAllText(animatorStatesPath, animatorStates.ToString(), Encoding.UTF8);
        }

        private ISyntax GetSetAnimatorStatesCore(ISyntax root)
        {
            var tree = new TreeSyntaxNode();
            var body = new TreeSyntaxNode();

            tree.Add(new UsingSyntax("System"));
            tree.Add(new UsingSyntax("HECSFramework.Serialize", 1));
            tree.Add(new NameSpaceSyntax("Commands"));
            tree.Add(new LeftScopeSyntax());
            tree.Add(body);
            tree.Add(new RightScopeSyntax());


            root.Tree.Add(tree);
            return body;
        }

        private void PutAnimatorStateCommandSyntax(ISyntax helperClass, AnimatorController animator)
        {
            var tree = new TreeSyntaxNode();
            var fields = new TreeSyntaxNode();
            var methodBody = new TreeSyntaxNode();

            tree.Add(new TabSimpleSyntax(1, "[Serializable]"));
            tree.Add(new TabSimpleSyntax(1, $"public struct Set{animator.name}AnimatorState : ISetAnimatorState"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(fields);
            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(2, "public void SetState(AnimatorState animatorState)"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(methodBody);
            tree.Add(new RightScopeSyntax(2));
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new ParagraphSyntax());

            foreach (var p in animator.parameters)
            {
                //if (animParameters.Contains(p.name))
                //    continue;

                //animParameters.Add(p.name);

                switch (p.type)
                {
                    case AnimatorControllerParameterType.Float:
                        fields.Add(new TabSimpleSyntax(2, $"public {nameof(FloatId)} {p.name};"));
                        methodBody.Add(new TabSimpleSyntax(3, $"animatorState.SetFloat({p.name}.Id, {p.name}.Value);"));
                        break;
                    case AnimatorControllerParameterType.Int:
                        fields.Add(new TabSimpleSyntax(2, $"public {nameof(IntId)} {p.name};"));
                        methodBody.Add(new TabSimpleSyntax(3, $"animatorState.SetInt({p.name}.Id, {p.name}.Value);"));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        fields.Add(new TabSimpleSyntax(2, $"public {nameof(BoolId)} {p.name};"));
                        methodBody.Add(new TabSimpleSyntax(3, $"animatorState.SetBool({p.name}.Id, {p.name}.Value);"));
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        break;
                }
            }

            helperClass.Tree.Add(tree);
        }

        private void GenerateAnimationParameterIdentifier(string parameterName)
        {
            var path = "Assets/" + AnimParametersBluePrintsPath + parameterName + ".asset";

            if (!File.Exists(path))
            {
                AnimatorParameterIdentifier so = ScriptableObject.CreateInstance<AnimatorParameterIdentifier>();
                so.name = parameterName;
                AssetDatabase.CreateAsset(so, path);
            }
        }

        private ISyntax GetInitMethod(AnimatorController animatorController)
        {
            var tree = new TreeSyntaxNode();
            var methodBody = new TreeSyntaxNode();
            var dictionaryBody = new TreeSyntaxNode();

            tree.Add(new TabSimpleSyntax(2, $"private static void Init{animatorController.name}()"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(methodBody);
            tree.Add(new RightScopeSyntax(2));

            var dictionaryName = "dictionary" + animatorController.name;
            methodBody.Add(new TabSimpleSyntax(3, $"var {dictionaryName} = new Dictionary<int, string>()"));
            methodBody.Add(new LeftScopeSyntax(3));
            methodBody.Add(dictionaryBody);
            methodBody.Add(new RightScopeSyntax(3, true));
            methodBody.Add(new ParagraphSyntax());

            if (SerializationAnimatorState)
            {
                methodBody.Add(GetStateResolver(animatorController));
            }

            methodBody.Add(new TabSimpleSyntax(3, $"{animatorController.name} = new {nameof(AnimatorHelper)}({dictionaryName});"));
            methodBody.Add(new TabSimpleSyntax(3, $"animhelpers.Add({CParse.Quote}{animatorController.name}{CParse.Quote}, {animatorController.name});"));
            return tree;
        }

        private ISyntax GetStateResolver(AnimatorController animatorController)
        {
            var tree = new TreeSyntaxNode();
            var dictBoolBody = new TreeSyntaxNode();
            var dictIntBody = new TreeSyntaxNode();
            var dictFloatBody = new TreeSyntaxNode();

            tree.Add(new TabSimpleSyntax(3, "var resolver = new AnimatorStateResolver"));
            tree.Add(new LeftScopeSyntax(3));

            tree.Add(GetDictionary(4, "BoolStates",
                "int", "BoolParameterResolver", dictBoolBody, true));
            tree.Add(GetDictionary(4, "IntStates",
                "int", "IntParameterResolver", dictIntBody, true));
            tree.Add(GetDictionary(4, "FloatStates",
                "int", "FloatParameterResolver", dictFloatBody, true));

            tree.Add(new RightScopeSyntax(3, true));
            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(3, $"animStateProviders.Add({CParse.Quote}{animatorController.name}{CParse.Quote}, resolver);"));

            foreach (var p in animatorController.parameters)
            {
                switch (p.type)
                {
                    case AnimatorControllerParameterType.Float:
                        dictFloatBody.Add(GetDictionaryStroke(5, Animator.StringToHash(p.name).ToString(), $"new {"FloatParameterResolver"}()"));
                        break;
                    case AnimatorControllerParameterType.Int:
                        dictIntBody.Add(GetDictionaryStroke(5, Animator.StringToHash(p.name).ToString(), $"new {"IntParameterResolver"}()"));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        dictBoolBody.Add(GetDictionaryStroke(5, Animator.StringToHash(p.name).ToString(), $"new {"BoolParameterResolver"}()"));
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        break;
                }
            }

            return tree;
        }

        private ISyntax GetDictionaryStroke(int step, string key, string value)
        {
            var tree = new TreeSyntaxNode();
            tree.Add(new TabSimpleSyntax(step, $"{CParse.LeftScope}{key}, {value} {CParse.RightScope},"));
            return tree;
        }

        private ISyntax GetDictionary(int initialTab, string name, string key, string value, ISyntax bodyOfDic, bool isCommaAtEnd = false, bool isSemicolonAtEnd = false)
        {
            var tree = new TreeSyntaxNode();
            tree.Add(new TabSimpleSyntax(initialTab, $"{name} = new Dictionary<{key},{value}>()"));
            tree.Add(new LeftScopeSyntax(initialTab));
            tree.Add(bodyOfDic);

            if (isCommaAtEnd)
                tree.Add(new RightScopeSyntax(initialTab, false));
            else if (isSemicolonAtEnd)
                tree.Add(new RightScopeSyntax(initialTab, true));
            else
                tree.Add(new RightScopeSyntax(initialTab));

            return tree;
        }

        private void ProcessMotion(Motion motion, AnimatorState animatorState, ISyntax tree)
        {
            switch (motion)
            {
                case AnimationClip clip:

                    var filePath = StateBluePrintsPath + animatorState.name + ".asset";

                    if (!File.Exists(Application.dataPath + filePath))
                    {
                        AnimatorStateIdentifier so = ScriptableObject.CreateInstance<AnimatorStateIdentifier>();
                        so.name = animatorState.name;
                        AssetDatabase.CreateAsset(so, filePath);
                    }
                    else
                    {
                        var needed = AssetDatabase.LoadAssetAtPath<AnimatorStateIdentifier>(filePath);
                        needed.name = animatorState.name;
                        EditorUtility.SetDirty(needed);
                    }

                    tree.AddUnique(GetDictionaryPosition(clip, animatorState.name));
                    break;
                case BlendTree blendTree:

                    foreach (var c in blendTree.children)
                    {
                        //var name = animatorState.name.Replace(" ", "_") + "_" + c.motion.name;
                        //var blendTreeStatefilePath = StateBluePrintsPath + name + ".asset";

                        //if (!File.Exists(Application.dataPath + blendTreeStatefilePath))
                        //{
                        //    AnimatorStateIdentifier so = ScriptableObject.CreateInstance<AnimatorStateIdentifier>();
                        //    so.name = name;
                        //    AssetDatabase.CreateAsset(so, blendTreeStatefilePath);
                        //}
                        //else
                        //{
                        //    var needed = AssetDatabase.LoadAssetAtPath<AnimatorStateIdentifier>(blendTreeStatefilePath);
                        //    needed.name = name;
                        //    EditorUtility.SetDirty(needed);
                        //}
                        //tree.AddUnique(GetDictionaryPosition(c.motion, name));
                    }

                    break;
                default:
                    HECSDebug.LogWarning("нет нужного кейса");
                    break;
            }
        }

        private ISyntax GetDictionaryPosition(Motion motion, string stateName)
        {
            return new TabSimpleSyntax(4, $"{CParse.LeftScope}{IndexGenerator.GetIndexForType(stateName)}, {CParse.Quote}{motion.name}{CParse.Quote}{CParse.RightScope},");
        }
    }
}
