using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CI
{
    internal static class DeployWebGL
    {
        // Sibling folder convention (see ai-context/current-context.md): the deploy
        // repo sits next to this project's folder, named "<project>-build". Derived
        // from Application.dataPath rather than hardcoded so this isn't tied to one
        // machine's absolute path.
        private static string BuildFolder
        {
            get
            {
                var projectRoot = Directory.GetParent(Application.dataPath)!;
                return Path.Combine(projectRoot.Parent!.FullName, projectRoot.Name + "-build");
            }
        }

        [MenuItem("Build/Build && Deploy WebGL")]
        public static void BuildAndDeploy()
        {
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = BuildFolder,
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

            var onDiskMb = GetDirectorySize(BuildFolder) / 1024 / 1024;
            Debug.Log($"Deployed WebGL build ({onDiskMb} MB on disk).\n{pushOutput}");
            EditorUtility.DisplayDialog("Deployed", $"Pushed to GitHub Pages.\nBuild size: {onDiskMb} MB", "OK");
        }

        // BuildReport.summary.totalSize reports pre-compression size, not the
        // actual .unityweb bytes that get transferred - measure the real thing.
        private static long GetDirectorySize(string path)
        {
            return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + ".git" + Path.DirectorySeparatorChar))
                .Sum(f => new FileInfo(f).Length);
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

            // Read the redirected streams before WaitForExit, not after - if git ever
            // writes enough output to fill the OS pipe buffer, waiting first deadlocks
            // (git blocks writing, we block waiting for exit that can't happen).
            using var process = Process.Start(psi);
            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            output = stdout + stderr;
            return process.ExitCode;
        }
    }
}
