using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador interactivo para las puertas de las aulas del Bloque 9.
/// Permite que las puertas se abran suavemente hacia el interior del salón cuando los estudiantes
/// se aproximan, y se cierren automáticamente una vez que han pasado o ingresado.
/// </summary>
[SelectionBase]
public class DoorController : MonoBehaviour
{
    public enum DoorState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    [Header("Configuración de Rotación")]
    [Tooltip("Ángulo de apertura en grados. Negativo (-90) para salones Norte, positivo (+90) para salones Sur.")]
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float closedAngle = 0f;
    [SerializeField] private float swingSpeed = 160f; // Grados por segundo

    [Header("Detección de Proximidad")]
    [SerializeField] private float detectionRadius = 2.2f;
    [SerializeField] private float autoCloseDelay = 1.2f;

    [Header("Dimensiones de Puerta")]
    [SerializeField] private float doorWidth = 0.95f;
    [SerializeField] private float doorHeight = 2.20f;
    [SerializeField] private float doorThickness = 0.10f;

    [Header("Estado Actual")]
    [SerializeField] private DoorState currentState = DoorState.Closed;
    [SerializeField] private float currentAngle = 0f;

    private float targetAngle = 0f;
    private float closeTimer = 0f;
    private float holdOpenUntilTime = 0f;
    private Vector3 doorwayCenter;
    private BoxCollider doorCollider;

    public bool IsOpen => Mathf.Abs(currentAngle - closedAngle) > 20f;
    public bool IsFullyOpen => Mathf.Abs(currentAngle - openAngle) < 5f;
    public Vector3 DoorwayCenter => doorwayCenter;
    public float OpenAngle => openAngle;

    private void Awake()
    {
        InitializeDoor();
    }

    /// <summary>
    /// Configura el centro del umbral y colisionador de la puerta.
    /// </summary>
    public void InitializeDoor()
    {
        // En Unity, la bisagra está en transform.position. La hoja se extiende en +X en espacio local.
        doorwayCenter = transform.position + transform.right * (doorWidth * 0.5f);

        // Auto-detectar sentido según posición Z
        if (transform.position.z > 0)
        {
            openAngle = -90f; // Norte abre hacia +Z (interior del salón)
        }
        else
        {
            openAngle = 90f;  // Sur abre hacia -Z (interior del salón)
        }

        currentAngle = closedAngle;
        targetAngle = closedAngle;
        currentState = DoorState.Closed;
        transform.localRotation = Quaternion.Euler(0f, closedAngle, 0f);

        EnsureCollider();
    }

    private void EnsureCollider()
    {
        doorCollider = GetComponent<BoxCollider>();
        if (doorCollider == null)
        {
            doorCollider = gameObject.AddComponent<BoxCollider>();
        }

        // Colisionador ajustado al tamaño de la hoja de la puerta
        doorCollider.center = new Vector3(doorWidth * 0.5f, doorHeight * 0.5f, 0f);
        doorCollider.size = new Vector3(doorWidth, doorHeight, doorThickness);
    }

    private void Update()
    {
        CheckProximity();
        UpdateDoorMotion();
    }

    /// <summary>
    /// Detecta si hay algún estudiante aproximándose al umbral de la puerta.
    /// </summary>
    private void CheckProximity()
    {
        // Si hay una orden explícita de mantener abierta
        if (Time.time < holdOpenUntilTime)
        {
            SetTargetOpen();
            return;
        }

        bool studentNear = false;
        var allStudents = CampusSimulationManager.Instance != null 
            ? CampusSimulationManager.Instance.ActiveStudents 
            : null;

        if (allStudents != null)
        {
            for (int i = 0; i < allStudents.Count; i++)
            {
                StudentAgent s = allStudents[i];
                if (s == null) continue;

                float distToThreshold = Vector3.Distance(doorwayCenter, s.transform.position);
                if (distToThreshold <= detectionRadius)
                {
                    studentNear = true;
                    break;
                }
            }
        }

        if (studentNear)
        {
            closeTimer = 0f;
            SetTargetOpen();
        }
        else
        {
            if (currentState == DoorState.Open || currentState == DoorState.Opening)
            {
                closeTimer += Time.deltaTime;
                if (closeTimer >= autoCloseDelay)
                {
                    SetTargetClosed();
                }
            }
        }
    }

    private void SetTargetOpen()
    {
        targetAngle = openAngle;
        if (currentState != DoorState.Open)
        {
            currentState = DoorState.Opening;
        }
    }

    private void SetTargetClosed()
    {
        targetAngle = closedAngle;
        if (currentState != DoorState.Closed)
        {
            currentState = DoorState.Closing;
        }
    }

    /// <summary>
    /// Solicita que la puerta se abra y permanezca abierta durante una cantidad de segundos.
    /// </summary>
    public void OpenDoor(float holdDuration = 2.0f)
    {
        holdOpenUntilTime = Mathf.Max(holdOpenUntilTime, Time.time + holdDuration);
        SetTargetOpen();
    }

    /// <summary>
    /// Solicita el cierre inmediato de la puerta.
    /// </summary>
    public void CloseDoor()
    {
        holdOpenUntilTime = 0f;
        SetTargetClosed();
    }

    private void UpdateDoorMotion()
    {
        if (Mathf.Abs(currentAngle - targetAngle) > 0.1f)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, swingSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(0f, currentAngle, 0f);

            if (Mathf.Abs(currentAngle - targetAngle) <= 0.1f)
            {
                currentAngle = targetAngle;
                transform.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
                currentState = (targetAngle == openAngle) ? DoorState.Open : DoorState.Closed;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsOpen ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(doorwayCenter, 0.25f);
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.35f);
        Gizmos.DrawWireSphere(doorwayCenter, detectionRadius);

        // Dibujar bisagra
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.08f);
    }
}
