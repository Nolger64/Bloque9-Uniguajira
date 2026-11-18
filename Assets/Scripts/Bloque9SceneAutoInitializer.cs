using UnityEngine;

/// <summary>
/// Inicializador automático de la escena Bloque 9.
/// Se ejecuta automáticamente al entrar en PlayMode o al cargar la escena.
/// Configura la cámara panorámica, las puertas interactivas y los colisionadores de suelo.
/// </summary>
public class Bloque9SceneAutoInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void AutoInitializeScene()
    {
        Debug.Log("<color=#FFDD00>[Bloque 9]</color> Inicializando sistema de simulación y puertas interactivas...");

        // 1. Configurar la Cámara Principal con vista panorámica óptima
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0f, 18f, -48f);
            mainCam.transform.eulerAngles = new Vector3(20f, 0f, 0f);
            mainCam.nearClipPlane = 0.3f;
            mainCam.farClipPlane = 1500f;

            if (mainCam.GetComponent<CameraController>() == null)
            {
                mainCam.gameObject.AddComponent<CameraController>();
            }
        }

        // 2. Configurar o encontrar el Manager de Simulación
        CampusSimulationManager manager = Object.FindFirstObjectByType<CampusSimulationManager>();
        if (manager == null)
        {
            GameObject managerObj = new GameObject("--- SISTEMA_SIMULACION_BLOQUE9 ---");
            manager = managerObj.AddComponent<CampusSimulationManager>();
            managerObj.AddComponent<SimulationUIPanel>();
        }
        else
        {
            if (manager.GetComponent<SimulationUIPanel>() == null)
            {
                manager.gameObject.AddComponent<SimulationUIPanel>();
            }
        }

        // 3. Configurar todas las puertas interactivas del Bloque 9
        SetupAllDoors();

        if (manager != null)
        {
            manager.RefreshDoors();
        }

        // 4. Colisionadores en suelos
        AddColliderIfMissing("Piso_Planta_Baja");
        AddColliderIfMissing("Andenes_y_Via");
        AddColliderIfMissing("Terreno_Cesped");
        AddColliderIfMissing("Losas_Pisos_Blancas");

        Debug.Log("<color=#00FFAA>[Bloque 9]</color> Escena, puertas y cámara configuradas con éxito. Pulsa [Tab] para minimizar el panel.");
    }

    /// <summary>
    /// Recorre todos los objetos que representan puertas en el campus y les asegura su DoorController.
    /// </summary>
    public static void SetupAllDoors()
    {
        GameObject b9 = GameObject.Find("bloque9");
        Transform[] allTransforms = b9 != null 
            ? b9.GetComponentsInChildren<Transform>(true)
            : Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int doorCount = 0;
        foreach (var t in allTransforms)
        {
            if (t.name.StartsWith("Puerta_") && !t.name.EndsWith("_Mesh"))
            {
                DoorController dc = t.GetComponent<DoorController>();
                if (dc == null)
                {
                    dc = t.gameObject.AddComponent<DoorController>();
                }
                dc.InitializeDoor();
                doorCount++;
            }
        }

        Debug.Log($"<color=#00FFAA>[Bloque 9 Puertas]</color> {doorCount} puertas configuradas con controladores automáticos de apertura/cierre.");
    }

    private static void AddColliderIfMissing(string objectName)
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
