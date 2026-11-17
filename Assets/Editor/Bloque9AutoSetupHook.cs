using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class Bloque9AutoSetupHook
{
    static Bloque9AutoSetupHook()
    {
        EditorApplication.delayCall += EnsureSceneIsConfigured;
    }

    private static void EnsureSceneIsConfigured()
    {
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            Debug.Log("<color=#FFDD00>[Bloque 9 Hook]</color> Verificando configuración permanente en la escena...");
            Bloque9SetupEditor.SetupSceneHierarchy();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Bloque 9 Hook] Error: {ex}");
        }
    }
}
