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

public class HECSRoslynCodegen : OdinEditorWindow
{
    [Serializable]
    private sealed class CodegenConfig
    {
        private const string FileName = "HECSCodegenSettings.json";

        [Sirenix.OdinInspector.FilePath(AbsolutePath = true)]
        [OnValueChanged(nameof(Save))]
        public string CodegenExePath = string.Empty;

        [FolderPath(AbsolutePath = true)]
        [OnValueChanged(nameof(Save))]
        public string ClientScriptDirectory = string.Empty;

        [FolderPath(AbsolutePath = true)]
        [OnValueChanged(nameof(Save))]
        public string ServerScriptDirectory = string.Empty;

        [PropertySpace]

        [FolderPath(AbsolutePath = true)]
        [DisableIf("@!MspGenerationEnabled")]
        [OnValueChanged(nameof(Save))]
        public string MspScanDirectory = string.Empty;

        [Sirenix.OdinInspector.FilePath(AbsolutePath = true)]
        [DisableIf("@!MspGenerationEnabled")]
        [OnValueChanged(nameof(Save))]
        public string MspFilePath = string.Empty;

        [BoxGroup("Settings")]
        [HorizontalGroup("Settings/Split", Width = 200, LabelWidth = 120)]
        [LabelText("MSP Generation")]
        [OnValueChanged(nameof(Save))]
        public bool MspGenerationEnabled;

        [LabelText("| Serialization")]
        [HorizontalGroup("Settings/Split/Next", Width = 200, LabelWidth = 120)]
        [OnValueChanged(nameof(Save))]
        public bool Serialization;

        [HorizontalGroup("Settings/Split/Next/Next", Width = 200, LabelWidth = 150)]
        [LabelText("| Network Command Map")]
        [OnValueChanged(nameof(Save))]
        public bool NetworkCommandMap;

        [HorizontalGroup("Settings/Split/Next/Next/Next", Width = 200, LabelWidth = 120)]
        [LabelText("| Force Rebuild")]
        [Tooltip("Полностью чистит Containers/Resolvers/FastComponentsProviders и пишет генерат без сверки. " +
                 "Разовая операция: снимается сама после успешного прогона.")]
        [OnValueChanged(nameof(Save))]
        public bool ForceRebuild;

        private static string ConfigPath
            => Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? string.Empty, "Library", FileName);

        private static string DefaultMspFilePath
            => Path.Combine(InstallHECS.ScriptPath.TrimEnd('/'), InstallHECS.HECSGenerated.Trim('/'), "msp.cs");

        public static CodegenConfig Load()
        {
            var result = new CodegenConfig();

            try
            {
                if (File.Exists(ConfigPath))
                    JsonUtility.FromJsonOverwrite(File.ReadAllText(ConfigPath), result);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[HECS] Failed to read codegen settings from {ConfigPath}, using defaults: {e}");
            }

            result.ApplyDefaults();
            return result;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                File.WriteAllText(ConfigPath, JsonUtility.ToJson(this, true));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[HECS] Failed to write codegen settings to {ConfigPath}: {e}");
            }
        }

        private void ApplyDefaults()
        {
            var changed = false;

            if (!Directory.Exists(ClientScriptDirectory))
            {
                ClientScriptDirectory = Application.dataPath;
                changed = true;
            }

            if (!File.Exists(CodegenExePath))
            {
                CodegenExePath = FindCodegenExecutable();
                changed = true;
            }

            if (!Directory.Exists(MspScanDirectory))
            {
                MspScanDirectory = Application.dataPath;
                changed = true;
            }

            if (string.IsNullOrEmpty(MspFilePath))
            {
                MspFilePath = DefaultMspFilePath;
                changed = true;
            }

            if (changed)
                Save();
        }

        private static string FindCodegenExecutable()
        {
#if UNITY_EDITOR_OSX
#if OSX_INTEL
            var patterns = new[] { "script_x86.sh.command", "script.sh.command" };
#else
            var patterns = new[] { "script.sh.command", "script_x86.sh.command" };
#endif
#else
            var patterns = new[] { "RoslynHECS.exe" };
#endif
            try
            {
                foreach (var pattern in patterns)
                {
                    var found = Directory.GetFiles(Application.dataPath, pattern, SearchOption.AllDirectories);

                    if (found.Length > 0)
                        return found[0];
                }

                Debug.LogWarning($"[HECS] Codegen executable not found under {Application.dataPath}.");
                return string.Empty;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[HECS] Failed to locate the codegen executable under {Application.dataPath}: {e}");
                return string.Empty;
            }
        }
    }

    private const int GenerationTimeoutMs = 5 * 60 * 1000;

    private static CodegenConfig config;

    [ShowInInspector, InlineProperty, HideLabel, HideReferenceObjectPicker, PropertyOrder(-1)]
    private static CodegenConfig Config
    {
        get => config ??= CodegenConfig.Load();
        set => config = value;
    }

    [MenuItem("HECS Options/Roslyn Codegen %&#F10", priority = -30)]
    public static void RoslynCodegenMenu()
        => GetWindow<HECSRoslynCodegen>();

    [Button]
    public async void CodegenClient()
    {
        if (await Generate($"{PathArgument(Config.ClientScriptDirectory)}{ClientArguments()}{ForceRebuildArgument()}", false))
            ResetForceRebuild();
    }

    [Button]
    public async void CodegenServer()
    {
        if (await Generate($"{PathArgument(Config.ServerScriptDirectory)} server no_blueprints{ForceRebuildArgument()}", true))
            ResetForceRebuild();
    }

    [Button]
    public async void CodegenAll()
        => await CodegenAsync();

    public async Task CodegenAsync()
    {
        //оба захода читают флаг до сброса, поэтому force_rebuild уходит и в сервер, и в клиент
        var server = await Generate($"{PathArgument(Config.ServerScriptDirectory)} server no_blueprints{ForceRebuildArgument()}", true);
        var client = await Generate($"{PathArgument(Config.ClientScriptDirectory)}{ClientArguments()}{ForceRebuildArgument()}", false);

        if (server && client)
            ResetForceRebuild();
    }

    private static string PathArgument(string directory)
        => directory != null && directory.Contains(' ') ? $"\"path:{directory}\"" : $"path:{directory}";

    private static string Quote(string value)
        => value != null && value.Contains(' ') ? $"\"{value}\"" : value;

    private string ClientArguments()
    {
        string args = string.Empty;

        if (!Config.NetworkCommandMap)
            args += " no_commands";

        if (!Config.Serialization)
            args += " no_resolvers";

        return args;
    }

    private static string ForceRebuildArgument()
        => Config.ForceRebuild ? " force_rebuild" : string.Empty;

    //force_rebuild чистит директории генерата и пишет без сверки — это разовая операция,
    //забытая включённой она молча убивает write-if-changed и заставляет Unity реимпортить всё
    private static void ResetForceRebuild()
    {
        if (!Config.ForceRebuild)
            return;

        Config.ForceRebuild = false;
        Config.Save();
        Debug.Log("[HECS] Force rebuild выполнен, галка снята.");
    }

    private async Task<bool> Generate(string args, bool isServer)
    {
        Debug.Log("Generating Roslyn files...");

        var exePath = Config.CodegenExePath;

        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
        {
            Debug.LogError($"[HECS] Codegen executable not found at '{exePath}'. " +
                           "Set Codegen Exe Path in HECS Options / Roslyn Codegen.");
            return false;
        }

#if UNITY_EDITOR_OSX
        var fileName = "/bin/bash";
        var arguments = $"{Quote(exePath)} {args}";
#else
        var fileName = exePath;
        var arguments = args;
#endif

        Process myProcess = new Process
        {
            StartInfo =
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            },
            EnableRaisingEvents = true
        };

        var exited = new TaskCompletionSource<bool>();
        myProcess.Exited += (a, b) => exited.TrySetResult(true);

        EditorApplication.LockReloadAssemblies();

        try
        {
            try
            {
                myProcess.Start();
            }
            catch (Exception e)
            {
                Debug.LogError($"[HECS] Failed to start codegen '{fileName}': {e}");
                return false;
            }

            if (await Task.WhenAny(exited.Task, Task.Delay(GenerationTimeoutMs)) != exited.Task)
            {
                Debug.LogError($"[HECS] Codegen did not finish within {GenerationTimeoutMs / 1000} seconds.");
                return false;
            }

            if (myProcess.ExitCode != 0)
            {
                Debug.LogError($"[HECS] Codegen exited with code {myProcess.ExitCode}.");
                return false;
            }

            Debug.Log("Roslyn files generated.");

            if (isServer)
                return true;

            //Debug.Log("Generating counters map...");
            //GenerateCountersMap.GenerateCountersMapFunc();

            if (!Config.MspGenerationEnabled)
                return true;

            Debug.Log("Generating MessagePack files...");
            var result = await MspGeneration(Config.MspScanDirectory, Config.MspFilePath, Application.dataPath);
            Debug.Log(result);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[HECS] Codegen failed: {e}");
            return false;
        }
        finally
        {
            EditorApplication.UnlockReloadAssemblies();
            myProcess.Dispose();
        }
    }

    private static Task<string> MspGeneration(string input, string output, string dataPath)
    {
        var fileName = "mpc";
        var arguments = $"-i {Quote(input)} -o {Quote(output)}";
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

        if (p == null)
            return Task.FromException<string>(new InvalidOperationException($"Failed to start '{fileName}'"));

        var tcs = new TaskCompletionSource<string>();
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        p.EnableRaisingEvents = true;

        p.OutputDataReceived += (a, b) =>
        {
            if (b.Data != null)
                stdout.AppendLine(b.Data);
        };

        p.ErrorDataReceived += (a, b) =>
        {
            if (b.Data != null)
                stderr.AppendLine(b.Data);
        };

        p.Exited += (a, b) =>
        {
            p.WaitForExit();

            if (stderr.Length > 0)
                Debug.LogWarning($"[HECS] {fileName}: {stderr}");

            tcs.TrySetResult(stdout.ToString());
        };

        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        tcs.Task.ContinueWith(_ => p.Dispose(), TaskScheduler.Default);

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