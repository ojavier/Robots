using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Peticion : MonoBehaviour
{
    [System.Serializable]
    public class GridData
    {
        public int step;
        public int[][] grid;
    }

    public GameObject robotPrefab; // Prefab del robot
    private GameObject[] robots; // Array de robots
    private int gridSizeX = 3; // Ancho del grid (ajustar según sea necesario)
    private int gridSizeY = 3; // Alto del grid (ajustar según sea necesario)

    void Start()
    {
        // Iniciar la corrutina para recibir los datos
        StartCoroutine(ReceiveData());
    }

    IEnumerator ReceiveData()
    {
        string url = "http://localhost:8585";

        // Crear una solicitud GET
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            // Enviar la solicitud y esperar la respuesta
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Verificar la respuesta del servidor
                Debug.Log("Respuesta del servidor: " + www.downloadHandler.text);

                // Parsear la respuesta JSON si es válida
                try
                {
                    GridData response = JsonUtility.FromJson<GridData>(www.downloadHandler.text);
                    Debug.Log("Datos recibidos: paso = " + response.step);

                    // Actualizar la matriz y los robots
                    UpdateRobots(response.grid);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error al parsear JSON: " + e.Message);
                }
            }
        }
    }

    void UpdateRobots(int[][] grid)
    {
        // Inicializar robots si no se han creado aún
        if (robots == null)
        {
            robots = new GameObject[gridSizeX * gridSizeY];
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    int index = y * gridSizeX + x;
                    if (grid[y][x] == 8) // Suponiendo que 8 representa un robot
                    {
                        robots[index] = Instantiate(robotPrefab, new Vector3(x, 0, y), Quaternion.identity);
                    }
                }
            }
        }

        // Actualizar posiciones de robots
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                int index = y * gridSizeX + x;
                if (grid[y][x] == 8) // Suponiendo que 8 representa un robot
                {
                    if (robots[index] != null)
                    {
                        robots[index].transform.position = new Vector3(x, 0, y);
                    }
                    else
                    {
                        robots[index] = Instantiate(robotPrefab, new Vector3(x, 0, y), Quaternion.identity);
                    }
                }
            }
        }
    }
}
