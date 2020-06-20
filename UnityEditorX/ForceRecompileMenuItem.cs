
using UnityEditor;
using UnityEngine;

public static class ForceRecompileMenuItem
{
    [MenuItem("Tools/Force Recompile")]
    private static void ForceRecompile()
    {
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
    }

    [MenuItem("Tools/Force Recompile", validate = true)]
    private static bool ForceRecompile_Validate()
    {
        return !Application.isPlaying;
    }
}
