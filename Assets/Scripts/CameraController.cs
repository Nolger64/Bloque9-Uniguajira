using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controlador de cámara panorámica interactivo adaptado para visualizar el Bloque 9 completo
/// y el tránsito de los estudiantes desde la calle hacia el interior.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Velocidades de Navegación")]
    public float moveSpeed = 22f;
    public float lookSpeed = 2.5f;
    public float zoomSpeed = 4f;

    [Header("Presets de Vista Optimizados para Bloque 9")]
    // Vista 1: Panorámica completa de la fachada principal y andén (donde inician y entran los estudiantes)
    public Vector3 exteriorPos = new Vector3(0f, 18f, -48f);
    public Vector3 exteriorRot = new Vector3(20f, 0f, 0f);

    // Vista 2: Patio central y corredores interiores (para ver las puertas de las aulas)
    public Vector3 patioPos = new Vector3(0f, 7f, -16f);
    public Vector3 patioRot = new Vector3(14f, 0f, 0f);

    // Vista 3: Vista aérea superior cenital (para observar todo el flujo aéreo del campus)
    public Vector3 aerialPos = new Vector3(0f, 42f, -10f);
    public Vector3 aerialRot = new Vector3(72f, 0f, 0f);

    private Vector3 targetPos;
    private Quaternion targetRot;
    private bool isTransitioning = false;

    private void Start()
    {
        // Posicionar inmediatamente en la vista panorámica frontal
        transform.position = exteriorPos;
        transform.eulerAngles = exteriorRot;
        targetPos = transform.position;
        targetRot = transform.rotation;

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1500f;
        }
    }

    private void Update()
    {
        HandlePresetShortcuts();

        if (isTransitioning)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 6f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 6f);
            if (Vector3.Distance(transform.position, targetPos) < 0.15f)
            {
                isTransitioning = false;
            }
            return;
        }

        HandleManualMovement();
        HandleRotation();
        HandleZoom();
    }

    private void HandlePresetShortcuts()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame)
        {
            SetViewPreset(exteriorPos, exteriorRot);
        }
        else if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame)
        {
            SetViewPreset(patioPos, patioRot);
        }
        else if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame)
        {
            SetViewPreset(aerialPos, aerialRot);
        }
#else
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SetViewPreset(exteriorPos, exteriorRot);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SetViewPreset(patioPos, patioRot);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SetViewPreset(aerialPos, aerialRot);
        }
#endif
    }

    public void SetViewPreset(Vector3 pos, Vector3 rot)
    {
        targetPos = pos;
        targetRot = Quaternion.Euler(rot);
        isTransitioning = true;
    }

    private void HandleManualMovement()
    {
        Vector3 move = Vector3.zero;

#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move += transform.forward;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move -= transform.forward;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move += transform.right;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move -= transform.right;
            if (kb.eKey.isPressed) move += Vector3.up;
            if (kb.qKey.isPressed) move -= Vector3.up;
        }

        move.y = 0;
        if (move.sqrMagnitude > 0.001f)
        {
            transform.position += move.normalized * moveSpeed * Time.deltaTime;
        }

        if (mouse != null && mouse.middleButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            transform.Translate(-delta.x * 0.03f, -delta.y * 0.03f, 0, Space.Self);
        }
#else
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        move = transform.right * h + transform.forward * v;
        move.y = 0;

        if (Input.GetKey(KeyCode.E)) move += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) move -= Vector3.up;

        if (move.sqrMagnitude > 0.001f)
        {
            transform.position += move.normalized * moveSpeed * Time.deltaTime;
        }

        if (Input.GetMouseButton(2))
        {
            float deltaX = -Input.GetAxis("Mouse X") * moveSpeed * 0.5f * Time.deltaTime;
            float deltaY = -Input.GetAxis("Mouse Y") * moveSpeed * 0.5f * Time.deltaTime;
            transform.Translate(deltaX, deltaY, 0, Space.Self);
        }
#endif
    }

    private void HandleRotation()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            float rotX = delta.x * lookSpeed * 0.1f;
            float rotY = -delta.y * lookSpeed * 0.1f;

            Vector3 currentEuler = transform.eulerAngles;
            float newPitch = currentEuler.x + rotY;
            if (newPitch > 180f) newPitch -= 360f;
            newPitch = Mathf.Clamp(newPitch, -10f, 85f);

            transform.rotation = Quaternion.Euler(newPitch, currentEuler.y + rotX, 0f);
        }
#else
        if (Input.GetMouseButton(1))
        {
            float rotX = Input.GetAxis("Mouse X") * lookSpeed;
            float rotY = -Input.GetAxis("Mouse Y") * lookSpeed;

            Vector3 currentEuler = transform.eulerAngles;
            float newPitch = currentEuler.x + rotY;
            if (newPitch > 180f) newPitch -= 360f;
            newPitch = Mathf.Clamp(newPitch, -10f, 85f);

            transform.rotation = Quaternion.Euler(newPitch, currentEuler.y + rotX, 0f);
        }
#endif
    }

    private void HandleZoom()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                transform.position += transform.forward * (Mathf.Sign(scroll) * zoomSpeed);
            }
        }
#else
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            transform.position += transform.forward * scroll * zoomSpeed * 10f;
        }
#endif
    }
}
