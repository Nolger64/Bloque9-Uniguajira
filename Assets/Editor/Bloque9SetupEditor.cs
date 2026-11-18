using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Utilidad del Editor para configurar con un solo clic la escena de Bloque 9 en la jerarquía.
/// </summary>
public static class Bloque9SetupEditor
{
    [MenuItem("Bloque9/🚀 Configurar Escena y Sistema de Estudiantes")]
    public static void SetupSceneHierarchy()
    {
        Debug.Log("<color=#FFDD00>[Bloque 9 Setup]</color> Configurando jerarquía de la escena...");

        // 1. Configurar la Cámara en la posición panorámica frontal ideal
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Undo.RecordObject(mainCam.transform, "Posicionar Cámara Bloque 9");
            mainCam.transform.position = new Vector3(0f, 18f, -48f);
            mainCam.transform.eulerAngles = new Vector3(20f, 0f, 0f);
            mainCam.nearClipPlane = 0.3f;
            mainCam.farClipPlane = 1500f;

            if (mainCam.GetComponent<CameraController>() == null)
            {
                Undo.AddComponent<CameraController>(mainCam.gameObject);
            }
        }

        // 2. Crear o actualizar el Manager
        CampusSimulationManager manager = Object.FindFirstObjectByType<CampusSimulationManager>();
        if (manager == null)
        {
            GameObject managerObj = new GameObject("--- SISTEMA_SIMULACION_BLOQUE9 ---");
            manager = managerObj.AddComponent<CampusSimulationManager>();
            managerObj.AddComponent<SimulationUIPanel>();
            Undo.RegisterCreatedObjectUndo(managerObj, "Crear Manager de Simulación");
        }
        else
        {
            if (manager.GetComponent<SimulationUIPanel>() == null)
            {
                Undo.AddComponent<SimulationUIPanel>(manager.gameObject);
            }
        }

        // 3. Activar los estudiantes del modelo
        GameObject b9 = GameObject.Find("bloque9");
        if (b9 != null)
        {
            Transform[] children = b9.GetComponentsInChildren<Transform>(true);
            foreach (var t in children)
            {
                if (t.name.StartsWith("NPC_Estudiante_0") && !t.name.EndsWith("_Mesh"))
                {
                    Undo.RecordObject(t.gameObject, "Activar estudiante");
                    t.gameObject.SetActive(true);
                }
            }
        }

        // 4. Configurar todas las puertas interactivas
        Bloque9SceneAutoInitializer.SetupAllDoors();
        if (manager != null)
        {
            manager.RefreshDoors();
        }

        // 5. Colisionadores en suelos
        EnsureCollider("Piso_Planta_Baja");
        EnsureCollider("Andenes_y_Via");
        EnsureCollider("Terreno_Cesped");
        EnsureCollider("Losas_Pisos_Blancas");

        // Guardar escena
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("<color=#00FFAA>[Bloque 9 Setup]</color> ¡Configuración completada y escena guardada!");
    }

    private static void EnsureCollider(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null && obj.GetComponent<Collider>() == null)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                MeshCollider mc = obj.AddComponent<MeshCollider>();
                mc.convex = false;
            }
            else
            {
                obj.AddComponent<BoxCollider>();
            }
        }
    }
}
