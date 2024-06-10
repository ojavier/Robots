using UnityEngine;

public class AjustarCamara : MonoBehaviour
{
    public Tablero tablero;
    public Camera camara;

    void Start()
    {
        if (tablero == null)
        {
            Debug.LogError("El tablero no está asignado.");
            return;
        }

        if (camara == null)
        {
            Debug.LogError("La cámara no está asignada.");
            return;
        }

        if (tablero.mapLoader == null)
        {
            Debug.LogError("El mapLoader del tablero no está asignado.");
            return;
        }

        AjustarVista();
    }

    void AjustarVista()
    {
        // Dimensiones del tablero
        int width = tablero.mapLoader.width;
        int height = tablero.mapLoader.height;

        // Calcular el centro del tablero
        Vector3 centroTablero = new Vector3((width - 1) * 1.5f, 0, -(height - 1) * 1.5f);

        // Ajustar la posición de la cámara
        float maxDimension = Mathf.Max(width, height);
        float alturaCamara = maxDimension * 1.5f; // Ajustar este valor según sea necesario

        camara.transform.position = new Vector3(centroTablero.x, alturaCamara, centroTablero.z);
        camara.transform.rotation = Quaternion.Euler(90, 0, 0); // Apuntar la cámara directamente hacia abajo

        // Ajustar el tamaño de la cámara
        if (camara.orthographic)
        {
            camara.orthographicSize = maxDimension * 1.5f; // Ajustar el tamaño para cubrir todo el tablero
        }
    }
}
