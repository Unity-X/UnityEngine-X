
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditorX
{
    internal static class EditorApplicationMenuItems
    {
        [MenuItem("Tools/Editor Application/Force Recompile")]
        private static void ForceRecompile()
        {
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation(UnityEditor.Compilation.RequestScriptCompilationOptions.CleanBuildCache);
        }

        [MenuItem("Tools/Editor Application/Force Recompile", validate = true)]
        private static bool ForceRecompile_Validate()
        {
            return !Application.isPlaying;
        }

        [MenuItem("Tools/Editor Application/Reload Domain")]
        private static void ReloadDomain()
        {
            EditorUtility.RequestScriptReload();
        }

        [MenuItem("Tools/Editor Application/Reload Domain", validate = true)]
        private static bool ReloadDomain_Validate()
        {
            return !Application.isPlaying;
        }

        [MenuItem("Tools/Editor Application/Reload Scene")]
        private static void ReloadScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene != null)
            {
                var opts = new LoadSceneParameters { };
                EditorSceneManager.LoadSceneInPlayMode(scene.path, opts);
            }
        }

        [MenuItem("Tools/Editor Application/Reload Scene", validate = true)]
        private static bool ReloadScene_Validate()
        {
            return !Application.isPlaying;
        }
    }
}
