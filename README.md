# Simulación Bloque 9 - Universidad de La Guajira

Simulación interactiva en 3D en **Unity 6 (URP)** del flujo, ingreso, permanencia y salida de estudiantes en el **Bloque 9 de la Universidad de La Guajira (Uniguajira)**.

---

## 🏛️ Características del Proyecto

- **Modelo Arquitectónico:**
  - Réplica del Bloque 9 de Uniguajira con colores institucionales (Amarillo y Rojo), 3 plantas, patio central, escaleras curvas, rampas y corredores de salones modulares.
  - Vía y andén exterior perimetral.
- **Simulación de Estudiantes (NPCs):**
  - Sistema de navegación inteligente multi-etapa:
    - *Spawn Perimétrico* $\rightarrow$ *Aproximación a la Plaza* $\rightarrow$ *Pórtico Principal* $\rightarrow$ *Patio Central* $\rightarrow$ *Corredores* $\rightarrow$ *Puertas de Salón* $\rightarrow$ *En Clase* $\rightarrow$ *Ruta Inversa de Salida*.
  - Animación procedimental cinemática humanoide de 15 huesos (movimiento real de piernas, rodillas, braceo y caderas).
  - Marcadores visuales de alta visibilidad para seguimiento a distancia.
  - Generación dinámica y variada con diferentes vestimentas y morrales.
- **Panel de Gestión en Tiempo Real (UI):**
  - Modificación del flujo / capacidad objetivo de estudiantes (1 a 40).
  - Modificación del tiempo promedio de estancia en el bloque (2 a 60 segundos).
  - Contador en vivo de estudiantes dentro del edificio y métricas de tránsito exterior.
  - Botones de acción rápida: *+ Entrar 1*, *🚨 Evacuar / Salir Todos*, *Pausar / Reanudar*.
- **Control de Cámara Panorámica:**
  - Tecla `[1]`: Vista frontal exterior (calle, andén y fachada).
  - Tecla `[2]`: Vista del patio central y corredores de aulas.
  - Tecla `[3]`: Vista aérea cenital de todo el campus.
  - Clic derecho para rotar, rueda para zoom y WASD para desplazamiento libre.

---

## 🚀 Requisitos

- **Unity 6** (6000.0+) con Universal Render Pipeline (URP).
- Paquetes requeridos:
  - `com.unity.inputsystem`
  - `com.unity.render-pipelines.universal`
  - `com.unity.ugui`
