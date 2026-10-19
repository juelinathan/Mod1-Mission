using System.Linq;
using UnityEditor;

public static class WebBuild
{
    public static void BuildForGitHubPages()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
        BuildPipeline.BuildPlayer(scenes, "docs", BuildTarget.WebGL, BuildOptions.None);
    }
}
