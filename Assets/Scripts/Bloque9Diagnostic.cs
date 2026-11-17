using UnityEngine;
using System.IO;

[ExecuteAlways]
public class Bloque9Diagnostic : MonoBehaviour
{
    private void Start()
    {
        RunDiagnosis();
    }

    private void Update()
    {
        // Solo corre una vez por recarga
    }

    [ContextMenu("Diagnosticar")]
    public void RunDiagnosis()
    {
        string report = "=== DIAGNOSTICO BLOQUE 9 ===\n";
        GameObject b9 = GameObject.Find("bloque9");
        if (b9 == null)
        {
            report += "ERROR: 'bloque9' no existe en la escena!\n";
        }
        else
        {
            report += $"bloque9: Pos={b9.transform.position}, Rot={b9.transform.eulerAngles}, Scale={b9.transform.lossyScale}\n";
            Renderer[] renderers = b9.GetComponentsInChildren<Renderer>(true);
            report += $"Total Renderers: {renderers.Length}\n";
            Bounds totalBounds = new Bounds(b9.transform.position, Vector3.zero);
            foreach (var r in renderers)
            {
                totalBounds.Encapsulate(r.bounds);
                report += $"  R: {r.name} (Parent: {r.transform.parent?.name}) -> Bounds Center={r.bounds.center}, Size={r.bounds.size}, ActiveInHierarchy={r.gameObject.activeInHierarchy}\n";
            }
            report += $"\nBOUNDS TOTALES DE BLOQUE9: Center={totalBounds.center}, Size={totalBounds.size}, Min={totalBounds.min}, Max={totalBounds.max}\n";
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            report += $"\nMAIN CAMERA: Pos={cam.transform.position}, Rot={cam.transform.eulerAngles}, FOV={cam.fieldOfView}, Far={cam.farClipPlane}\n";
        }

        File.WriteAllText("Assets/diag_output.txt", report);
        Debug.Log("<color=#00FFAA>[Bloque 9 Diagnostic]</color> Reporte guardado en Assets/diag_output.txt");
    }
}
