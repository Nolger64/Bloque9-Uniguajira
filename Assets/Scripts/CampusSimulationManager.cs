using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Administrador del flujo, generación y monitoreo de estudiantes en el Bloque 9.
/// </summary>
public class CampusSimulationManager : MonoBehaviour
{
    public static CampusSimulationManager Instance { get; private set; }

    [Header("Parámetros de Simulación")]
    [Range(1, 40)]
    [SerializeField] private int targetStudents = 12;

    [Range(2f, 60f)]
    [SerializeField] private float averageStayTime = 14f;

    [SerializeField] private float studentSpeed = 3.6f;

    [Header("Métricas en Tiempo Real")]
    [SerializeField] private int studentsInside = 0;
    [SerializeField] private int totalExited = 0;
    [SerializeField] private bool isPaused = false;

    private List<StudentAgent> activeStudents = new List<StudentAgent>();
    private List<GameObject> studentTemplates = new List<GameObject>();
    private Transform studentContainer;
    private float nextSpawnTime = 0f;

    // 7 Puntos de Spawn perimétricos exteriores
    private Vector3[] perimeterSpawnPoints = new Vector3[]
    {
        new Vector3(-38f, 0f, -28f), // 1. Andén Extremo Oeste
        new Vector3(-22f, 0f, -32f), // 2. Paso Peatonal Suroeste
        new Vector3(-12f, 0f, -30f), // 3. Sendero Verde Oeste
        new Vector3(0f, 0f, -35f),   // 4. Acceso Central desde la Calle
        new Vector3(12f, 0f, -30f),  // 5. Sendero Verde Este
        new Vector3(22f, 0f, -32f),  // 6. Paso Peatonal Sureste
        new Vector3(38f, 0f, -28f)   // 7. Andén Extremo Este
    };

    // 14 Puertas de Salones en los dos corredores del Bloque 9
    private Vector3[] classroomDoors = new Vector3[]
    {
        // Corredor Sur (Z = -6.8m)
        new Vector3(-16.6f, 0f, -6.8f),
        new Vector3(-11.6f, 0f, -6.8f),
        new Vector3(-6.6f, 0f, -6.8f),
        new Vector3(-1.6f, 0f, -6.8f),
        new Vector3(6.6f, 0f, -6.8f),
        new Vector3(11.6f, 0f, -6.8f),
        new Vector3(16.6f, 0f, -6.8f),

        // Corredor Norte (Z = +6.8m)
        new Vector3(-16.6f, 0f, 6.8f),
        new Vector3(-11.6f, 0f, 6.8f),
        new Vector3(-6.6f, 0f, 6.8f),
        new Vector3(-1.6f, 0f, 6.8f),
        new Vector3(6.6f, 0f, 6.8f),
        new Vector3(11.6f, 0f, 6.8f),
        new Vector3(16.6f, 0f, 6.8f)
    };

    private Vector3[] leavePoints = new Vector3[]
    {
        new Vector3(-42f, 0f, -28f),
        new Vector3(42f, 0f, -28f),
        new Vector3(0f, 0f, -38f)
    };

    public int TargetStudents
    {
        get => targetStudents;
        set => targetStudents = Mathf.Clamp(value, 1, 50);
    }

    public float AverageStayTime
    {
        get => averageStayTime;
        set => averageStayTime = Mathf.Clamp(value, 2f, 60f);
    }

    public int StudentsInside => studentsInside;
    public int ActiveStudentsCount => activeStudents.Count;
    public int TotalExited => totalExited;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GameObject containerObj = GameObject.Find("Contenedor_Estudiantes_Activos");
        if (containerObj == null)
        {
            containerObj = new GameObject("Contenedor_Estudiantes_Activos");
        }
        studentContainer = containerObj.transform;

        InitializeAndDistributeStudents();
    }

    /// <summary>
    /// Toma los estudiantes del modelo y los distribuye a los puntos de spawn exteriores para que no arranquen en fila.
    /// </summary>
    private void InitializeAndDistributeStudents()
    {
        studentTemplates.Clear();

        GameObject b9 = GameObject.Find("bloque9");
        Transform[] allChildren = b9 != null 
            ? b9.GetComponentsInChildren<Transform>(true) 
            : FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        List<GameObject> existing = new List<GameObject>();
        foreach (var t in allChildren)
        {
            if (t.name.StartsWith("NPC_Estudiante_0") && !t.name.EndsWith("_Mesh"))
            {
                existing.Add(t.gameObject);
                studentTemplates.Add(t.gameObject);
            }
        }

        Debug.Log($"<color=#00FFAA>[Bloque 9 Manager]</color> {existing.Count} estudiantes encontrados en el campus.");

        // Distribuir los estudiantes a los puntos perimétricos exteriores
        for (int i = 0; i < existing.Count; i++)
        {
            GameObject s = existing[i];
            s.SetActive(true);

            // Asegurar que el SkinnedMeshRenderer actualice siempre
            SkinnedMeshRenderer smr = s.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (smr != null)
            {
                smr.updateWhenOffscreen = true;
            }

            Vector3 spawnPoint = perimeterSpawnPoints[i % perimeterSpawnPoints.Length];
            s.transform.position = spawnPoint;

            StudentAgent agent = s.GetComponent<StudentAgent>();
            if (agent == null)
            {
                agent = s.AddComponent<StudentAgent>();
            }

            Vector3 door = classroomDoors[Random.Range(0, classroomDoors.Length)];
            Vector3 leave = leavePoints[Random.Range(0, leavePoints.Length)];
            float stay = averageStayTime * Random.Range(0.85f, 1.25f);

            agent.Initialize(
                this,
                spawnPoint,
                door,
                leave,
                stay,
                studentSpeed
            );

            if (!activeStudents.Contains(agent))
            {
                activeStudents.Add(agent);
            }
        }

        nextSpawnTime = Time.time + 1.5f;
    }

    private void Update()
    {
        if (isPaused) return;

        float dynamicInterval = Mathf.Clamp(averageStayTime / Mathf.Max(1, targetStudents), 1.2f, 3.5f);

        if (activeStudents.Count < targetStudents && Time.time >= nextSpawnTime)
        {
            SpawnStudent();
            nextSpawnTime = Time.time + dynamicInterval;
        }
    }

    public void SpawnStudent()
    {
        Vector3 spawn = perimeterSpawnPoints[Random.Range(0, perimeterSpawnPoints.Length)];
        Vector3 door = classroomDoors[Random.Range(0, classroomDoors.Length)];
        Vector3 leave = leavePoints[Random.Range(0, leavePoints.Length)];
        float stay = averageStayTime * Random.Range(0.85f, 1.25f);

        GameObject studentObj = null;

        if (studentTemplates.Count > 0)
        {
            GameObject template = studentTemplates[Random.Range(0, studentTemplates.Count)];
            studentObj = Instantiate(template, spawn, Quaternion.identity, studentContainer);
            studentObj.SetActive(true);

            // Remapear huesos del SkinnedMeshRenderer al clon para evitar que se vuelva invisible
            RemapBonesForClone(studentObj);
        }
        else
        {
            studentObj = CreateProceduralStudent(spawn);
            studentObj.transform.SetParent(studentContainer);
        }

        studentObj.name = $"Estudiante_{activeStudents.Count + 1}";

        StudentAgent agent = studentObj.GetComponent<StudentAgent>();
        if (agent == null)
        {
            agent = studentObj.AddComponent<StudentAgent>();
        }

        agent.Initialize(
            this,
            spawn,
            door,
            leave,
            stay,
            studentSpeed
        );

        activeStudents.Add(agent);
    }

    private void RemapBonesForClone(GameObject clone)
    {
        SkinnedMeshRenderer smr = clone.GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (smr == null) return;

        smr.updateWhenOffscreen = true;

        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        foreach (var t in clone.GetComponentsInChildren<Transform>(true))
        {
            boneMap[t.name] = t;
        }

        Transform[] oldBones = smr.bones;
        Transform[] newBones = new Transform[oldBones.Length];
        for (int i = 0; i < oldBones.Length; i++)
        {
            if (oldBones[i] != null && boneMap.TryGetValue(oldBones[i].name, out Transform mapped))
            {
                newBones[i] = mapped;
            }
            else
            {
                newBones[i] = oldBones[i];
            }
        }
        smr.bones = newBones;

        if (smr.rootBone != null && boneMap.TryGetValue(smr.rootBone.name, out Transform rootMapped))
        {
            smr.rootBone = rootMapped;
        }
    }

    private GameObject CreateProceduralStudent(Vector3 spawnPos)
    {
        GameObject root = new GameObject("Estudiante_Procedural");
        root.transform.position = spawnPos;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Cuerpo";
        body.transform.SetParent(root.transform);
        body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        body.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);

        GameObject backpack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backpack.name = "Morral";
        backpack.transform.SetParent(root.transform);
        backpack.transform.localPosition = new Vector3(0f, 1.05f, -0.28f);
        backpack.transform.localScale = new Vector3(0.35f, 0.45f, 0.2f);

        Color[] colors = new Color[]
        {
            new Color(1f, 0.85f, 0.1f),   // Amarillo Uniguajira
            new Color(0.85f, 0.15f, 0.2f), // Rojo Uniguajira
            new Color(0.15f, 0.5f, 0.9f),  // Azul
            new Color(0.2f, 0.75f, 0.3f),  // Verde
            new Color(0.95f, 0.95f, 0.95f) // Blanco
        };
        Color c = colors[Random.Range(0, colors.Length)];

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.color = c;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        body.GetComponent<Renderer>().material = mat;

        Material bpMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        bpMat.color = new Color(0.15f, 0.15f, 0.15f);
        if (bpMat.HasProperty("_BaseColor")) bpMat.SetColor("_BaseColor", bpMat.color);
        backpack.GetComponent<Renderer>().material = bpMat;

        Destroy(body.GetComponent<Collider>());
        Destroy(backpack.GetComponent<Collider>());

        return root;
    }

    public void OnStudentInsideStateChanged(StudentAgent agent, bool isInside)
    {
        RecalculateInsideCount();
    }

    public void OnStudentCompletedJourney(StudentAgent agent)
    {
        if (activeStudents.Contains(agent))
        {
            activeStudents.Remove(agent);
            totalExited++;
        }
        RecalculateInsideCount();

        // Si es uno de los originales, reutilizarlo reapareciéndolo en un punto perimétrico
        if (studentTemplates.Contains(agent.gameObject))
        {
            Vector3 newSpawn = perimeterSpawnPoints[Random.Range(0, perimeterSpawnPoints.Length)];
            Vector3 door = classroomDoors[Random.Range(0, classroomDoors.Length)];
            Vector3 leave = leavePoints[Random.Range(0, leavePoints.Length)];
            agent.Initialize(
                this,
                newSpawn,
                door,
                leave,
                averageStayTime * Random.Range(0.85f, 1.25f),
                studentSpeed
            );
            activeStudents.Add(agent);
        }
        else
        {
            Destroy(agent.gameObject);
        }
    }

    private void RecalculateInsideCount()
    {
        int count = 0;
        for (int i = 0; i < activeStudents.Count; i++)
        {
            if (activeStudents[i] != null && activeStudents[i].isInsideBuilding)
            {
                count++;
            }
        }
        studentsInside = count;
    }

    public void EvacuateAll()
    {
        foreach (var student in activeStudents)
        {
            if (student != null)
            {
                student.ForceLeaveNow();
            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
    }
}
