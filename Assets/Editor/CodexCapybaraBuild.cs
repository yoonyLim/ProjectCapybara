using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CodexCapybaraBuild
{
    public static void Windows64()
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "-capyBuildPath");
        if (index < 0 || index + 1 >= args.Length)
            throw new BuildFailedException("Pass -capyBuildPath with the executable output path.");
        var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (scenes.Length == 0 || scenes.Any(scene => !File.Exists(scene)))
            throw new BuildFailedException("Enabled build scenes are missing.");
        var path = Path.GetFullPath(args[index + 1]);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        Debug.Log("CAPYBARA_BUILD_SCENES: " + string.Join(", ", scenes));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        Debug.Log($"CAPYBARA_BUILD_RESULT: {report.summary.result}; errors={report.summary.totalErrors}; " +
                  $"warnings={report.summary.totalWarnings}; bytes={report.summary.totalSize}");
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("ProjectCapybara Windows build failed. See the build log.");
    }
}
