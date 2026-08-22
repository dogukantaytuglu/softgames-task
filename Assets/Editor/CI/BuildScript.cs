using UnityEditor;
using UnityEditor.Build.Reporting;

namespace CI
{
    internal static class BuildScript
    {
        public static void BuildWebGL()
        {
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = "build/WebGL/index.html",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            // BuildPipeline's own batchmode exit code is unreliable, so the
            // build result is the actual pass/fail signal for CI to check.
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
