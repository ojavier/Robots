using UnityEngine;

public class AjustarCamara : MonoBehaviour
{
    // Definición de objetos
    public Tablero tablero;
    public Camera camara;

    /// Este método es llamado al iniciar el programa y verificq que los valores han sido asignados a los objetos
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

    // Ajusta la vista según las dimenciones del tablero, desde posición y rotación
    void AjustarVista()
    {
        int width = tablero.mapLoader.width;
        int height = tablero.mapLoader.height;

        Vector3 centroTablero = new Vector3((width - 1) * 1.5f, 0, -(height - 1) * 1.5f);

        float maxDimension = Mathf.Max(width, height);
        float alturaCamara = maxDimension * 1.5f;

        camara.transform.position = new Vector3(centroTablero.x, alturaCamara, centroTablero.z);
        camara.transform.rotation = Quaternion.Euler(90, 0, 0);

        if (camara.orthographic)
        {
            camara.orthographicSize = maxDimension * 1.5f;
        }
    }
}
