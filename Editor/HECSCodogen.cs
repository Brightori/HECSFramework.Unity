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
            if (EditorPrefs.HasKey(nameof(ClientScriptDirectory)))
            {
                if (!string.IsNullOrEmpty(EditorPrefs.GetString(nameof(ClientScriptDirectory))))
                    return;
            }

            EditorPrefs.SetString(nameof(ClientScriptDirectory), Application.dataPath);


            if (EditorPrefs.HasKey(nameof(CodegenExePath)))
            {
                if (!string.IsNullOrEmpty(EditorPrefs.GetString(nameof(CodegenExePath))))
                    return;
            }

            var find = Directory.GetFiles(Application.dataPath, "RoslynHECS.exe", SearchOption.AllDirectories);

            if (find != null && find.Length > 0 && !string.IsNullOrEmpty(find[0]))
                EditorPrefs.SetString(nameof(CodegenExePath), find[0]);
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
        get => EditorPrefs.GetString(nameof(CodegenExePath), "");
        set => EditorPrefs.SetString(nameof(CodegenExePath), value);
    }

    [FolderPath(AbsolutePath = true)]
    [OnInspectorInit("@ClientScriptDirectory")]
    public string ClientScriptDirectory
    {
        get => EditorPrefs.GetString(nameof(ClientScriptDirectory), "");
        set => EditorPrefs.SetString(nameof(ClientScriptDirectory), value);
    }

    [FolderPath(AbsolutePath = true)]
    [OnInspectorInit("@ServerScriptDirectory")]
    public string ServerScriptDirectory
    {
        get => EditorPrefs.GetString(nameof(ServerScriptDirectory), "");
        set => EditorPrefs.SetString(nameof(ServerScriptDirectory), value);
    }

    [PropertySpace]

    [FolderPath(AbsolutePath = true)]
    [OnInspectorInit("@MspScanDirectory")]
    [DisableIf("@!MspGenerationEnabled")]
    public string MspScanDirectory
    {
        get => EditorPrefs.GetString(nameof(MspScanDirectory), "");
        set => EditorPrefs.SetString(nameof(MspScanDirectory), value);
    }

    [FolderPath(AbsolutePath = true)]
    [OnInspectorInit("@MspFilePath")]
    [DisableIf("@!MspGenerationEnabled")]
    public string MspFilePath
    {
        get => EditorPrefs.GetString(nameof(MspFilePath), "");
        set => EditorPrefs.SetString(nameof(MspFilePath), value);
    }

    [BoxGroup("Settings")]
    [HorizontalGroup("Settings/Split", Width = 200, LabelWidth = 120)]
    [LabelText("MSP Generation")]
    [OnInspectorInit("@MspGenerationEnabled")]
    public bool MspGenerationEnabled
    {
        get => EditorPrefs.GetBool(nameof(MspGenerationEnabled), false);
        set => EditorPrefs.SetBool(nameof(MspGenerationEnabled), value);
    }

    [LabelText("| Serialization")]
    [HorizontalGroup("Settings/Split/Next", Width = 200, LabelWidth = 120)]
    [OnInspectorInit("@Serialization")]
    public bool Serialization
    {
        get => EditorPrefs.GetBool(nameof(Serialization), false);
        set => EditorPrefs.SetBool(nameof(Serialization), value);
    }

    [HorizontalGroup("Settings/Split/Next/Next", Width = 200, LabelWidth = 150)]
    [LabelText("| Network Command Map")]
    [OnInspectorInit("@NetworkCommandMap")]
    public bool NetworkCommandMap
    {
        get => EditorPrefs.GetBool(nameof(NetworkCommandMap), false);
        set => EditorPrefs.SetBool(nameof(NetworkCommandMap), value);
    }


    [MenuItem("HECS Options/Roslyn Codegen", priority = 0)]
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
        return tcs.Task;
    }
}