using HECSFramework.Core.Generator;
using HECSFramework.Unity.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
#pragma warning disable

[InitializeOnLoad]
public class HECSRoslynCodegen : OdinEditorWindow
{
    static HECSRoslynCodegen()
    {
        try
        {
            if (PlayerPrefs.HasKey(nameof(ClientScriptDirectory)))
            {
                if (!string.IsNullOrEmpty(EditorPrefs.GetString(nameof(ClientScriptDirectory))))
                    return;
            }

            PlayerPrefs.SetString(nameof(ClientScriptDirectory), Application.dataPath);


            if (PlayerPrefs.HasKey(nameof(CodegenExePath)))
            {
                if (!string.IsNullOrEmpty(EditorPrefs.GetString(nameof(CodegenExePath))))
                    return;
            }

#if OSX_ARM
            var find = Directory.GetFiles(Application.dataPath, "script.sh.command", SearchOption.AllDirectories);
#elif OSX_INTEL
            var find = Directory.GetFiles(Application.dataPath, "script_x86.sh.command", SearchOption.AllDirectories);
#else
            var find = Directory.GetFiles(Application.dataPath, "RoslynHECS.exe", SearchOption.AllDirectories);
#endif
            if (find != null && find.Length > 0 && !string.IsNullOrEmpty(find[0]))
                PlayerPrefs.SetString(nameof(CodegenExePath), find[0]);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Юнити шалит, попробуйте переоткрыть окно или юнити");
        }
    }

    [Sirenix.OdinInspector.FilePath(AbsolutePath = true)]
    [OnInspectorInit("@CodegenExePath")]
    public string CodegenExePath
    {
        get => PlayerPrefs.GetString(nameof(CodegenExePath), "");
        set => PlayerPrefs.SetString(nameof(CodegenExePath), value);
    }

    [FolderPath(AbsolutePath = true)]
    [OnInspectorInit("@ClientScriptDirectory")]
    public string ClientScriptDirectory
    {
        get => PlayerPrefs.GetString(nameof(ClientScriptDirectory), "");
        set => PlayerPrefs.SetString(nameof(ClientScriptDirectory), value);
    }

    [FolderPath(AbsolutePath = true)]
    [OnInspectorInit("@ServerScriptDirectory")]
    public string ServerScriptDirectory
    {
        get => PlayerPrefs.GetString(nameof(ServerScriptDirectory), "");
        set => PlayerPrefs.SetString(nameof(ServerScriptDirectory), value);
    }

    [PropertySpace]

    [FolderPath(AbsolutePath = true)]
    [OnInspectorInit("@MspScanDirectory")]
    [DisableIf("@!MspGenerationEnabled")]
    public string MspScanDirectory
    {
        get => PlayerPrefs.GetString(nameof(MspScanDirectory), "");
        set => PlayerPrefs.SetString(nameof(MspScanDirectory), value);
    }

    [FolderPath(AbsolutePath = true)]
    [OnInspectorInit("@MspFilePath")]
    [DisableIf("@!MspGenerationEnabled")]
    public string MspFilePath
    {
        get => PlayerPrefs.GetString(nameof(MspFilePath), "");
        set => PlayerPrefs.SetString(nameof(MspFilePath), value);
    }

    [BoxGroup("Settings")]
    [HorizontalGroup("Settings/Split", Width = 200, LabelWidth = 120)]
    [LabelText("MSP Generation")]
    [OnInspectorInit("@MspGenerationEnabled")]
    public bool MspGenerationEnabled
    {
        get => PlayerPrefs.GetInt(nameof(MspGenerationEnabled), 0) == 1;
        set => PlayerPrefs.SetInt(nameof(MspGenerationEnabled), value ? 1 : 0);
    }

    [LabelText("| Serialization")]
    [HorizontalGroup("Settings/Split/Next", Width = 200, LabelWidth = 120)]
    [OnInspectorInit("@Serialization")]
    public bool Serialization
    {
        get => PlayerPrefs.GetInt(nameof(Serialization), 0) == 1;
        set => PlayerPrefs.SetInt(nameof(Serialization), value ? 1 : 0);
    }

    [HorizontalGroup("Settings/Split/Next/Next", Width = 200, LabelWidth = 150)]
    [LabelText("| Network Command Map")]
    [OnInspectorInit("@NetworkCommandMap")]
    public bool NetworkCommandMap
    {
        get => PlayerPrefs.GetInt(nameof(NetworkCommandMap), 0) == 1;
        set => PlayerPrefs.SetInt(nameof(NetworkCommandMap), value ? 1 : 0);
    }


    [MenuItem("HECS Options/Roslyn Codegen", priority = -30)]
    public static void RoslynCodegenMenu()
        => GetWindow<HECSRoslynCodegen>();

    [Button]
    public async void CodegenClient()
        => await Generate($"path:{ClientScriptDirectory} {ClientArguments()}", false);

    [Button]
    public async void CodegenServer()
        => await Generate($"path:{ServerScriptDirectory} server no_blueprints", true);

    [Button]
    public void CodegenAll()
    {
        CodegenServer();
        CodegenClient();
    }

    public async Task CodegenAsync()
    {
        await Generate($"path:{ServerScriptDirectory} server no_blueprints", true);
        await Generate($"path:{ClientScriptDirectory} {ClientArguments()}", false);
    }

    private string ClientArguments()
    {
        string args = string.Empty;

        if (!NetworkCommandMap)
            args += " no_commands";

        if (!Serialization)
            args += " no_resolvers";

        return args;
    }

    private async Task Generate(string args, bool isServer)
    {
        Debug.Log("Generating Roslyn files...");

#if UNITY_EDITOR_OSX
        OSX();
        return;
#endif

        Process myProcess = new Process
        {
            StartInfo =
            {
                FileName = CodegenExePath,
                Arguments = args,
                WorkingDirectory = CodegenExePath
            },
            EnableRaisingEvents = true
        };
        myProcess.Start();

        if (isServer) return;

        //Debug.Log("Generating counters map...");
        //GenerateCountersMap.GenerateCountersMapFunc();

        if (!MspGenerationEnabled) return;

        EditorApplication.LockReloadAssemblies();

        var tcs = new TaskCompletionSource<bool>();
        myProcess.Exited += (a, b) => tcs.SetResult(true);
        await tcs.Task;

        Debug.Log("Generating MessagePack files...");
        var result = await MspGeneration(MspScanDirectory, MspFilePath, Application.dataPath);
        Debug.Log(result);

        EditorApplication.UnlockReloadAssemblies();
    }

    private void OSX()
    {
        string open = $@"open {CodegenExePath}";
        Process.Start(@"/System/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal"
            , open);
    }

    private static Task<string> MspGeneration(string input, string output, string dataPath)
    {
        var fileName = "mpc";
        var arguments = $"-i {input} -o {output}";
        var psi = new ProcessStartInfo
        {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = dataPath
        };

        Process p;
        try
        {
            p = Process.Start(psi);
        }
        catch (Exception ex)
        {
            return Task.FromException<string>(ex);
        }

        var tcs = new TaskCompletionSource<string>();
        p.EnableRaisingEvents = true;
        p.Exited += (a, b) =>
        {
            var data = p.StandardOutput.ReadToEnd();
            p.Dispose();
            p = null;
            tcs.TrySetResult(data);
        };

        var path = InstallHECS.ScriptPath + InstallHECS.HECSGenerated + "mpcHeader.cs";
        File.WriteAllText(path, GetResolverMapStaticConstructor().ToString());
        return tcs.Task;
    }

    public static ISyntax GetResolverMapStaticConstructor()
    {
        var tree = new TreeSyntaxNode();

        tree.Add(new UsingSyntax("MessagePack"));
        tree.Add(new UsingSyntax("MessagePack.Resolvers", 1));

        tree.Add(new NameSpaceSyntax("HECSFramework.Core"));
        tree.Add(new LeftScopeSyntax());

        tree.Add(new TabSimpleSyntax(1, "public partial class ResolversMap"));
        tree.Add(new LeftScopeSyntax(1));

        tree.Add(new TabSimpleSyntax(2, "private static bool isMessagePackInited;"));
        tree.Add(new TabSimpleSyntax(2, "static ResolversMap()"));
        tree.Add(new LeftScopeSyntax(2));
        tree.Add(new TabSimpleSyntax(3, "if (isMessagePackInited)"));
        tree.Add(new TabSimpleSyntax(4, "return;"));
        tree.Add(new TabSimpleSyntax(3, "StaticCompositeResolver.Instance.Register(StandardResolver.Instance, GeneratedResolver.Instance);"));
        tree.Add(new TabSimpleSyntax(3, "isMessagePackInited = true;"));
        tree.Add(new TabSimpleSyntax(3, "MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);"));
        tree.Add(new RightScopeSyntax(2));
        tree.Add(new RightScopeSyntax(1));

        tree.Add(new RightScopeSyntax());
        return tree;
    }
}