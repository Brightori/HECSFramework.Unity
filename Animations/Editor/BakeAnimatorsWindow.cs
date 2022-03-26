using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Animancer.Editor;
using HECSFramework.Core;
using HECSFramework.Core.Generator;
using HECSFramework.Serialize;
using HECSFramework.Unity.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorState = UnityEditor.Animations.AnimatorState;

namespace HECSFramework.Unity
{
    [Documentation(Doc.HECS, Doc.Animation, Doc.Editor, "Window for generate Animator helpers")]
    public class BakeAnimatorsWindow : OdinEditorWindow
    {
        private const string AnimatorHelperSave = "AnimatorHelper.cs";
        private string FilePath => InstallHECS.ScriptPath + InstallHECS.HECSGenerated + AnimatorHelperSave;
        private string BluePrintsPath => "Assets/" + InstallHECS.BluePrints + InstallHECS.Identifiers + "AnimatorStateIdentifiers/";

        private const string SaveAnimatorHelpers = "SaveAnimatorHelpers";
        private const string EmptyAnimators = "SaveAnimatorHelpers";
        private const char Split = '|';
        
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
            InstallHECS.CheckFolder(BluePrintsPath);
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

            foreach (var l in animatorController.layers)
            {
                foreach (var s in l.stateMachine.states)
                {
                    var motion = s.state.motion;

                    if (motion == null || string.IsNullOrEmpty(motion.name)) continue;

                    ProcessMotion(motion, s.state, dictionaryBody);
                }

                var stateMachines = animatorController.layers
                    .Select(x => x.stateMachine).SelectMany(z => z.stateMachines).SelectMany(y => y.stateMachine.states);

                foreach (var sm in stateMachines)
                {
                    var motion = sm.state.motion;

                    if (motion == null || string.IsNullOrEmpty(motion.name)) continue;

                    ProcessMotion(motion, sm.state, dictionaryBody);
                }
            }

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
            
            tree.Add(GetDictionary(4, nameof(AnimatorStateResolver.BoolStates), 
                "int", nameof(BoolParameterResolver), dictBoolBody, true));
            tree.Add(GetDictionary(4, nameof(AnimatorStateResolver.IntStates),
                "int", nameof(IntParameterResolver), dictIntBody, true));
            tree.Add(GetDictionary(4, nameof(AnimatorStateResolver.FloatStates),
                "int", nameof(FloatParameterResolver), dictFloatBody, true));

            tree.Add(new RightScopeSyntax(3, true));
            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(3, $"animStateProviders.Add({CParse.Quote}{animatorController.name}{CParse.Quote}, resolver);"));

            foreach (var p in animatorController.parameters)
            {
                switch (p.type)
                {
                    case AnimatorControllerParameterType.Float:
                        dictFloatBody.Add(GetDictionaryStroke(5, Animator.StringToHash(p.name).ToString(), $"new {nameof(FloatParameterResolver)}()"));
                        break;
                    case AnimatorControllerParameterType.Int:
                        dictIntBody.Add(GetDictionaryStroke(5, Animator.StringToHash(p.name).ToString(), $"new {nameof(IntParameterResolver)}()"));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        dictBoolBody.Add(GetDictionaryStroke(5, Animator.StringToHash(p.name).ToString(), $"new {nameof(BoolParameterResolver)}()"));
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

                    var filePath = BluePrintsPath + animatorState.name + ".asset";

                    if (!File.Exists(Application.dataPath+filePath))
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

                    tree.Tree.Add(GetDictionaryPosition(clip, animatorState.name));
                    break;
                case BlendTree blendTree:

                    foreach (var c in blendTree.children)
                    {
                        var name = animatorState.name + "_BlendTree_" + c.motion.name;
                        var blendTreeStatefilePath = BluePrintsPath + name + ".asset";

                        if (!File.Exists(Application.dataPath + blendTreeStatefilePath))
                        {
                            AnimatorStateIdentifier so = ScriptableObject.CreateInstance<AnimatorStateIdentifier>();
                            so.name = name;
                            AssetDatabase.CreateAsset(so, blendTreeStatefilePath);
                        }
                        else
                        {
                            var needed = AssetDatabase.LoadAssetAtPath<AnimatorStateIdentifier>(blendTreeStatefilePath);
                            needed.name = name;
                            EditorUtility.SetDirty(needed);
                        }
                        tree.Tree.Add(GetDictionaryPosition(c.motion, name));
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
