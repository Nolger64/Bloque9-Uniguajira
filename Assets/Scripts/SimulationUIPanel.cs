using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Panel de gestión visual interactivo para controlar la simulación de estudiantes en el Bloque 9.
/// Permite modificar en tiempo real el flujo de estudiantes, el tiempo de estancia y monitorear la ocupación.
/// Funciona de forma autónoma con OnGUI y se adapta a cualquier resolución.
/// </summary>
public class SimulationUIPanel : MonoBehaviour
{
    [Header("Configuración Visual")]
    public bool isMinimized = false;
    public KeyCode toggleKey = KeyCode.Tab;

    private CampusSimulationManager manager;
    private GUIStyle headerStyle;
    private GUIStyle bigNumberStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;
    private GUIStyle boxStyle;
    private GUIStyle subLabelStyle;
    private Texture2D panelBackground;
    private Texture2D buttonBackground;
    private Texture2D accentBackground;

    private void Start()
    {
        manager = CampusSimulationManager.Instance;
        if (manager == null)
        {
            manager = FindFirstObjectByType<CampusSimulationManager>();
        }
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.tabKey.wasPressedThisFrame)
        {
            isMinimized = !isMinimized;
        }
#else
        if (Input.GetKeyDown(toggleKey))
        {
            isMinimized = !isMinimized;
        }
#endif

        if (manager == null)
        {
            manager = CampusSimulationManager.Instance;
        }
    }

    private void InitStyles()
    {
        if (headerStyle != null) return;

        panelBackground = MakeColorTexture(2, 2, new Color(0.12f, 0.13f, 0.16f, 0.92f));
        buttonBackground = MakeColorTexture(2, 2, new Color(0.22f, 0.24f, 0.28f, 1f));
        accentBackground = MakeColorTexture(2, 2, new Color(0.85f, 0.2f, 0.25f, 1f));

        boxStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = panelBackground },
            padding = new RectOffset(16, 16, 16, 16)
        };

        headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, 0.88f, 0.2f) } // Amarillo Uniguajira
        };

        bigNumberStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.3f, 1f, 0.4f) } // Verde dinámico
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        subLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white, background = buttonBackground },
            hover = { textColor = new Color(1f, 0.88f, 0.2f) }
        };
    }

    private Texture2D MakeColorTexture(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void OnGUI()
    {
        InitStyles();

        if (manager == null) return;

        // Botón pequeño para minimizar / restaurar
        if (isMinimized)
        {
            if (GUI.Button(new Rect(15, 15, 140, 32), "📋 Abrir Panel", buttonStyle))
            {
                isMinimized = false;
            }
            return;
        }

        float panelWidth = 330f;
        float panelHeight = 440f;
        Rect panelRect = new Rect(15, 15, panelWidth, panelHeight);

        GUI.Box(panelRect, GUIContent.none, boxStyle);

        GUILayout.BeginArea(new Rect(25, 22, panelWidth - 20, panelHeight - 15));

        // Cabecera
        GUILayout.Label("🏛️ SIMULACIÓN BLOQUE 9", headerStyle);
        GUILayout.Label("Control de Flujo de Estudiantes", subLabelStyle);
        GUILayout.Space(8);

        // Métrica Principal: Estudiantes Dentro del Bloque
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("ESTUDIANTES DENTRO DEL BLOQUE", labelStyle);
        GUILayout.Label($"{manager.StudentsInside}", bigNumberStyle);
        GUILayout.EndVertical();

        GUILayout.Space(6);

        // Métricas Secundarias
        int outside = manager.ActiveStudentsCount - manager.StudentsInside;
        GUILayout.Label($"🚶 En tránsito / Andén: <b>{Mathf.Max(0, outside)}</b>", subLabelStyle);
        GUILayout.Label($"👥 Total activos en el campus: <b>{manager.ActiveStudentsCount}</b>", subLabelStyle);
        GUILayout.Label($"🎓 Han completado su ciclo y salido: <b>{manager.TotalExited}</b>", subLabelStyle);

        GUILayout.Space(10);
        GUILayout.Box(GUIContent.none, GUILayout.Height(1)); // Divisor
        GUILayout.Space(6);

        // Control 1: Cantidad de Estudiantes
        GUILayout.Label($"Capacidad / Flujo Objetivo: <b>{manager.TargetStudents} estudiantes</b>", labelStyle);
        int newTarget = (int)GUILayout.HorizontalSlider(manager.TargetStudents, 1, 35);
        if (newTarget != manager.TargetStudents)
        {
            manager.TargetStudents = newTarget;
        }

        GUILayout.Space(8);

        // Control 2: Tiempo Promedio en el Bloque
        GUILayout.Label($"Tiempo promedio adentro: <b>{manager.AverageStayTime:F1} segundos</b>", labelStyle);
        float newStay = GUILayout.HorizontalSlider(manager.AverageStayTime, 3f, 45f);
        if (Mathf.Abs(newStay - manager.AverageStayTime) > 0.1f)
        {
            manager.AverageStayTime = newStay;
        }

        GUILayout.Space(12);

        // Botones de Acción
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Entrar 1", buttonStyle, GUILayout.Height(28)))
        {
            manager.SpawnStudent();
        }

        if (GUILayout.Button("🚨 Salir Todos", buttonStyle, GUILayout.Height(28)))
        {
            manager.EvacuateAll();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
        string pauseLabel = manager.IsPaused ? "▶ Reanudar" : "⏸ Pausar";
        if (GUILayout.Button(pauseLabel, buttonStyle, GUILayout.Height(26)))
        {
            manager.TogglePause();
        }

        if (GUILayout.Button("Minimizar [Tab]", buttonStyle, GUILayout.Height(26)))
        {
            isMinimized = true;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        GUILayout.Label("<color=#888888>Cámaras: [1] Exterior | [2] Patio | [3] Aérea</color>", subLabelStyle);

        GUILayout.EndArea();
    }
}
