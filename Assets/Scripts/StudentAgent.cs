using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Agente de IA para cada estudiante del Bloque 9.
/// Navega por etapas hacia la entrada, patio y aulas, y reproduce animación humanoide real.
/// </summary>
public class StudentAgent : MonoBehaviour
{
    public enum JourneyState
    {
        WalkingToClassroom,
        InsideClassroom,
        WalkingToExit,
        Completed
    }

    [Header("Estado de Navegación")]
    public JourneyState journeyState = JourneyState.WalkingToClassroom;
    public bool isInsideBuilding = false;
    public float stayDuration = 12f;
    public float moveSpeed = 3.5f;

    [Header("Referencias")]
    public NavMeshAgent navAgent;

    // Rutas de IA
    private List<Vector3> currentWaypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;
    private Vector3 targetDoorPosition;
    private Vector3 assignedClassroomInterior;
    private Vector3 assignedLeavePoint;

    private float timerInClass = 0f;
    private bool forceExit = false;
    private CampusSimulationManager manager;

    // --- Sistema de Animación Procedimental Humanoide ---
    private Transform hipsBone;
    private Transform spineBone;
    private Transform chestBone;
    private Transform upperLegL, lowerLegL, footL;
    private Transform upperLegR, lowerLegR, footR;
    private Transform upperArmL, lowerArmL;
    private Transform upperArmR, lowerArmR;

    private Quaternion initRotUpperLegL, initRotUpperLegR;
    private Quaternion initRotLowerLegL, initRotLowerLegR;
    private Quaternion initRotUpperArmL, initRotUpperArmR;
    private Quaternion initRotLowerArmL, initRotLowerArmR;
    private Quaternion initRotHips, initRotChest;
    private Vector3 initLocalPosHips;

    private float walkCycle = 0f;
    private float currentAnimSpeed = 0f;
    private bool hasBones = false;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        CacheBones();
    }

    private void CacheBones()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (var t in allChildren)
        {
            switch (t.name)
            {
                case "Hips": hipsBone = t; break;
                case "Spine": spineBone = t; break;
                case "Chest": chestBone = t; break;
                case "UpperLeg.L": upperLegL = t; break;
                case "LowerLeg.L": lowerLegL = t; break;
                case "Foot.L": footL = t; break;
                case "UpperLeg.R": upperLegR = t; break;
                case "LowerLeg.R": lowerLegR = t; break;
                case "Foot.R": footR = t; break;
                case "UpperArm.L": upperArmL = t; break;
                case "LowerArm.L": lowerArmL = t; break;
                case "UpperArm.R": upperArmR = t; break;
                case "LowerArm.R": lowerArmR = t; break;
            }
        }

        if (upperLegL != null && upperLegR != null)
        {
            hasBones = true;
            initRotUpperLegL = upperLegL.localRotation;
            initRotUpperLegR = upperLegR.localRotation;
            initRotLowerLegL = lowerLegL != null ? lowerLegL.localRotation : Quaternion.identity;
            initRotLowerLegR = lowerLegR != null ? lowerLegR.localRotation : Quaternion.identity;

            if (upperArmL != null) initRotUpperArmL = upperArmL.localRotation;
            if (upperArmR != null) initRotUpperArmR = upperArmR.localRotation;
            if (lowerArmL != null) initRotLowerArmL = lowerArmL.localRotation;
            if (lowerArmR != null) initRotLowerArmR = lowerArmR.localRotation;

            if (hipsBone != null)
            {
                initRotHips = hipsBone.localRotation;
                initLocalPosHips = hipsBone.localPosition;
            }
            if (chestBone != null) initRotChest = chestBone.localRotation;
        }
    }

    public void Initialize(CampusSimulationManager simManager, Vector3 spawnPos, Vector3 doorPos, Vector3 leavePos, float stayTime, float speed)
    {
        manager = simManager;
        targetDoorPosition = doorPos;
        assignedLeavePoint = leavePos;
        stayDuration = stayTime;
        moveSpeed = speed;

        transform.position = spawnPos;
        isInsideBuilding = false;
        forceExit = false;
        timerInClass = 0f;

        float roomZOffset = doorPos.z > 0 ? 2.5f : -2.5f;
        assignedClassroomInterior = new Vector3(doorPos.x + Random.Range(-1f, 1f), doorPos.y, doorPos.z + roomZOffset);

        EnsureVisualMarker();
        BuildEntrancePath(spawnPos);
    }

    private void EnsureVisualMarker()
    {
        Transform existingMarker = transform.Find("Indicador_Visual");
        if (existingMarker == null)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Indicador_Visual";
            marker.transform.SetParent(transform);
            marker.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            marker.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            Destroy(marker.GetComponent<Collider>());

            Material markerMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
            markerMat.color = new Color(1f, 0.88f, 0.1f);
            if (markerMat.HasProperty("_BaseColor")) markerMat.SetColor("_BaseColor", markerMat.color);
            marker.GetComponent<Renderer>().material = markerMat;
        }
    }

    private void BuildEntrancePath(Vector3 startPos)
    {
        currentWaypoints.Clear();
        currentWaypointIndex = 0;

        // Etapa 1: Aproximación a la Plaza Exterior
        Vector3 plazaPoint = new Vector3(Mathf.Clamp(startPos.x * 0.4f, -8f, 8f), startPos.y, -18f);
        currentWaypoints.Add(plazaPoint);

        // Etapa 2: Enfilarse con el Pórtico de Entrada
        Vector3 porticoOutside = new Vector3(Random.Range(-1.5f, 1.5f), startPos.y, -13f);
        currentWaypoints.Add(porticoOutside);

        // Etapa 3: Cruzar el Pórtico de Entrada hacia adentro
        Vector3 porticoInside = new Vector3(Random.Range(-1.2f, 1.2f), startPos.y, -10f);
        currentWaypoints.Add(porticoInside);

        // Etapa 4: Patio Central Distribuidor
        Vector3 lobbyPoint = new Vector3(targetDoorPosition.x * 0.5f, startPos.y, 0f);
        currentWaypoints.Add(lobbyPoint);

        // Etapa 5: Corredor frente a la Puerta del Salón
        float corridorZ = targetDoorPosition.z > 0 ? 5.0f : -5.0f;
        Vector3 corridorPoint = new Vector3(targetDoorPosition.x, startPos.y, corridorZ);
        currentWaypoints.Add(corridorPoint);

        // Etapa 6: Umbral de la Puerta del Salón
        Vector3 doorThreshold = new Vector3(targetDoorPosition.x, startPos.y, targetDoorPosition.z);
        currentWaypoints.Add(doorThreshold);

        // Etapa 7: Interior del Salón
        currentWaypoints.Add(assignedClassroomInterior);

        journeyState = JourneyState.WalkingToClassroom;
    }

    private void BuildExitPath()
    {
        currentWaypoints.Clear();
        currentWaypointIndex = 0;

        float corridorZ = targetDoorPosition.z > 0 ? 5.0f : -5.0f;

        // 1. Salir del salón hacia la puerta
        currentWaypoints.Add(new Vector3(targetDoorPosition.x, transform.position.y, targetDoorPosition.z));

        // 2. Salir al corredor
        currentWaypoints.Add(new Vector3(targetDoorPosition.x, transform.position.y, corridorZ));

        // 3. Regresar al Patio Central
        currentWaypoints.Add(new Vector3(0f, transform.position.y, 0f));

        // 4. Cruzar el pórtico hacia la plaza exterior
        currentWaypoints.Add(new Vector3(0f, transform.position.y, -13f));
        currentWaypoints.Add(new Vector3(Random.Range(-4f, 4f), transform.position.y, -18f));

        // 5. Alejarse hacia el andén o vía
        currentWaypoints.Add(assignedLeavePoint);

        journeyState = JourneyState.WalkingToExit;
    }

    private void Update()
    {
        if (journeyState == JourneyState.InsideClassroom)
        {
            timerInClass += Time.deltaTime;
            currentAnimSpeed = Mathf.MoveTowards(currentAnimSpeed, 0f, Time.deltaTime * 5f);

            if (timerInClass >= stayDuration || forceExit)
            {
                BuildExitPath();
            }
        }
        else if (journeyState == JourneyState.WalkingToClassroom || journeyState == JourneyState.WalkingToExit)
        {
            NavigateWaypoints();
        }
    }

    private void NavigateWaypoints()
    {
        if (currentWaypoints == null || currentWaypoints.Count == 0) return;

        // Comprobación de seguridad contra desbordamiento de índice
        if (currentWaypointIndex >= currentWaypoints.Count)
        {
            OnPathFinished();
            return;
        }

        Vector3 target = currentWaypoints[currentWaypointIndex];

        // Detección en plano horizontal XZ
        Vector2 currentXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetXZ = new Vector2(target.x, target.z);
        float dist = Vector2.Distance(currentXZ, targetXZ);

        float threshold = (currentWaypointIndex == currentWaypoints.Count - 1) ? 0.35f : 0.65f;

        if (dist <= threshold)
        {
            currentWaypointIndex++;
            UpdateInsideBuildingState();

            if (currentWaypointIndex >= currentWaypoints.Count)
            {
                OnPathFinished();
                return;
            }
            return;
        }

        // Movimiento hacia el objetivo
        Vector2 dir2D = (targetXZ - currentXZ).normalized;
        Vector3 moveDir = new Vector3(dir2D.x, 0f, dir2D.y);

        float actualSpeed = moveSpeed;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, moveDir, out RaycastHit hit, 1.2f))
        {
            if (hit.collider.GetComponentInParent<StudentAgent>() != null)
            {
                actualSpeed *= 0.5f;
                moveDir = Quaternion.Euler(0, 25f, 0) * moveDir;
            }
        }

        transform.position += moveDir * actualSpeed * Time.deltaTime;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 9f);
        }

        currentAnimSpeed = Mathf.MoveTowards(currentAnimSpeed, actualSpeed, Time.deltaTime * 6f);
    }

    private void OnPathFinished()
    {
        if (journeyState == JourneyState.WalkingToClassroom)
        {
            journeyState = JourneyState.InsideClassroom;
            timerInClass = 0f;
            currentAnimSpeed = 0f;
        }
        else if (journeyState == JourneyState.WalkingToExit)
        {
            journeyState = JourneyState.Completed;
            currentAnimSpeed = 0f;
            if (manager != null)
            {
                manager.OnStudentCompletedJourney(this);
            }
        }
    }

    private void UpdateInsideBuildingState()
    {
        if (transform.position.z > -11f && !isInsideBuilding && journeyState == JourneyState.WalkingToClassroom)
        {
            isInsideBuilding = true;
            if (manager != null) manager.OnStudentInsideStateChanged(this, true);
        }
        else if (transform.position.z <= -12f && isInsideBuilding && journeyState == JourneyState.WalkingToExit)
        {
            isInsideBuilding = false;
            if (manager != null) manager.OnStudentInsideStateChanged(this, false);
        }
    }

    private void LateUpdate()
    {
        if (!hasBones) return;

        if (currentAnimSpeed > 0.1f)
        {
            walkCycle += Time.deltaTime * currentAnimSpeed * 3.8f;

            float legSwingAngle = Mathf.Sin(walkCycle) * 30f;
            float kneeBendL = Mathf.Clamp(Mathf.Cos(walkCycle), 0f, 1f) * 42f;
            float kneeBendR = Mathf.Clamp(-Mathf.Cos(walkCycle), 0f, 1f) * 42f;

            float armSwingAngle = -Mathf.Sin(walkCycle) * 26f;

            // Piernas (eje local X)
            upperLegL.localRotation = initRotUpperLegL * Quaternion.Euler(legSwingAngle, 0, 0);
            upperLegR.localRotation = initRotUpperLegR * Quaternion.Euler(-legSwingAngle, 0, 0);

            if (lowerLegL != null)
                lowerLegL.localRotation = initRotLowerLegL * Quaternion.Euler(-kneeBendL, 0, 0);
            if (lowerLegR != null)
                lowerLegR.localRotation = initRotLowerLegR * Quaternion.Euler(-kneeBendR, 0, 0);

            // Brazos contrarios a las piernas
            if (upperArmL != null)
                upperArmL.localRotation = initRotUpperArmL * Quaternion.Euler(armSwingAngle, 0, 0);
            if (upperArmR != null)
                upperArmR.localRotation = initRotUpperArmR * Quaternion.Euler(-armSwingAngle, 0, 0);

            // Caderas
            if (hipsBone != null)
            {
                float bob = Mathf.Sin(walkCycle * 2f) * 0.035f;
                hipsBone.localPosition = initLocalPosHips + Vector3.up * bob;
                hipsBone.localRotation = initRotHips * Quaternion.Euler(0, Mathf.Sin(walkCycle) * 3f, Mathf.Cos(walkCycle) * 2f);
            }
        }
        else
        {
            float breathe = Mathf.Sin(Time.time * 2f) * 1.5f;

            upperLegL.localRotation = Quaternion.Slerp(upperLegL.localRotation, initRotUpperLegL, Time.deltaTime * 6f);
            upperLegR.localRotation = Quaternion.Slerp(upperLegR.localRotation, initRotUpperLegR, Time.deltaTime * 6f);
            if (lowerLegL != null) lowerLegL.localRotation = Quaternion.Slerp(lowerLegL.localRotation, initRotLowerLegL, Time.deltaTime * 6f);
            if (lowerLegR != null) lowerLegR.localRotation = Quaternion.Slerp(lowerLegR.localRotation, initRotLowerLegR, Time.deltaTime * 6f);

            if (upperArmL != null) upperArmL.localRotation = Quaternion.Slerp(upperArmL.localRotation, initRotUpperArmL, Time.deltaTime * 6f);
            if (upperArmR != null) upperArmR.localRotation = Quaternion.Slerp(upperArmR.localRotation, initRotUpperArmR, Time.deltaTime * 6f);

            if (hipsBone != null)
            {
                hipsBone.localPosition = Vector3.Lerp(hipsBone.localPosition, initLocalPosHips, Time.deltaTime * 6f);
                hipsBone.localRotation = Quaternion.Slerp(hipsBone.localRotation, initRotHips, Time.deltaTime * 6f);
            }
            if (chestBone != null)
            {
                chestBone.localRotation = initRotChest * Quaternion.Euler(breathe, 0, 0);
            }
        }
    }

    public void ForceLeaveNow()
    {
        forceExit = true;
        if (journeyState == JourneyState.InsideClassroom || journeyState == JourneyState.WalkingToClassroom)
        {
            BuildExitPath();
        }
    }
}
