using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public static class CheckHierarchy
{
    static CheckHierarchy()
    {
        RunCheck();
    }

    [MenuItem("Bloque9/Debug Jerarquía y Posiciones")]
    public static void RunCheck()
    {
        string log = "=== REPORTE DE JERARQUIA Y POSICIONES ===\n";
        GameObject b9 = GameObject.Find("bloque9");
        if (b9 == null)
        {
            log += "NO SE ENCONTRO el objeto 'bloque9' en la escena!\n";
        }
        else
        {
            log += $"bloque9 encontrado en pos={b9.transform.position}, rot={b9.transform.eulerAngles}, scale={b9.transform.lossyScale}\n";
            log += $"Hijos de bloque9 ({b9.transform.childCount}):\n";
            for (int i = 0; i < b9.transform.childCount; i++)
            {
                Transform child = b9.transform.GetChild(i);
                log += $"  - {child.name}: pos={child.position}, rot={child.eulerAngles}, active={child.gameObject.activeSelf}\n";
            }
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            log += $"\nMain Camera: pos={cam.transform.position}, rot={cam.transform.eulerAngles}, near={cam.nearClipPlane}, far={cam.farClipPlane}\n";
        }

        File.WriteAllText("Assets/hierarchy_report.txt", log);
        Debug.Log("Reporte guardado en Assets/hierarchy_report.txt");
    }
}
