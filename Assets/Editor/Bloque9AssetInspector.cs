using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class Bloque9AssetInspector
{
    static Bloque9AssetInspector()
    {
        InspectScene();
    }

    [MenuItem("Bloque9/Diagnostico Completo de Escena")]
    public static void InspectScene()
    {
        try
        {
            Scene activeScene = SceneManager.GetActiveScene();
            string log = $"=== DIAGNOSTICO COMPLETO (Escena: '{activeScene.name}', path: '{activeScene.path}') ===\n";
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            log += $"Root objects en la escena ({rootObjects.Length}):\n";
            foreach (var go in rootObjects)
            {
                log += $"- {go.name} (Active: {go.activeSelf}, Children: {go.transform.childCount})\n";
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    Transform c = go.transform.GetChild(i);
                    log += $"    -> {c.name} (Active: {c.gameObject.activeSelf})\n";
                }
            }

            File.WriteAllText("Assets/scene_diag.txt", log);
        }
        catch (System.Exception ex)
        {
            File.WriteAllText("Assets/scene_diag_err.txt", ex.ToString());
        }
    }
}
