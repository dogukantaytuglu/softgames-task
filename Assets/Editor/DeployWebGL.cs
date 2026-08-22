using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CI
{
    internal static class DeployWebGL
    {
        private const string BuildFolder = @"E:\Projects\UnityProjects\softgames-task-build";

        [MenuItem("Build/Build && Deploy WebGL")]
        public static void BuildAndDeploy()
        {
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = BuildFolder + "/index.html",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                EditorUtility.DisplayDialog("Build failed", $"WebGL build result: {report.summary.result}\nSee the Console for details.", "OK");
                return;
            }

            if (RunGit("add -A", out var addOutput) != 0)
            {
                EditorUtility.DisplayDialog("Deploy failed", $"git add failed:\n{addOutput}", "OK");
                return;
            }

            if (RunGit("diff --cached --quiet", out _) == 0)
            {
                EditorUtility.DisplayDialog("Nothing to deploy", "Build output is identical to the last deploy - nothing to commit.", "OK");
                return;
            }

            var commitMessage = $"Build {DateTime.Now:yyyy-MM-dd HH:mm}";
            if (RunGit($"commit -m \"{commitMessage}\"", out var commitOutput) != 0)
            {
                EditorUtility.DisplayDialog("Deploy failed", $"git commit failed:\n{commitOutput}", "OK");
                return;
            }

            if (RunGit("push origin master", out var pushOutput) != 0)
            {
                EditorUtility.DisplayDialog("Deploy failed", $"git push failed:\n{pushOutput}", "OK");
                return;
            }

            Debug.Log($"Deployed WebGL build ({report.summary.totalSize / 1024 / 1024} MB).\n{pushOutput}");
            EditorUtility.DisplayDialog("Deployed", $"Pushed to GitHub Pages.\nBuild size: {report.summary.totalSize / 1024 / 1024} MB", "OK");
        }

        private static int RunGit(string arguments, out string output)
        {
            var psi = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = BuildFolder,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            process.WaitForExit();
            output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            return process.ExitCode;
        }
    }
}
