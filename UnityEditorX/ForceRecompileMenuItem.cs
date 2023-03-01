
using UnityEditor;
using UnityEngine;

namespace UnityEditorX
{
    internal static class ForceRecompileMenuItem
    {
        [MenuItem("Tools/Force Recompile")]
        private static void ForceRecompile()
        {
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation(UnityEditor.Compilation.RequestScriptCompilationOptions.CleanBuildCache);
        }

        [MenuItem("Tools/Force Recompile", validate = true)]
        private static bool ForceRecompile_Validate()
        {
            return !Application.isPlaying;
        }
    }
}
