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
    private Vector3[,] targetPositions; // Array de posiciones objetivo para cada robot

    void Start()
    {
        // Inicializar las posiciones objetivo
        targetPositions = new Vector3[gridSizeX, gridSizeY];
        
        // Iniciar la corrutina para recibir los datos
        StartCoroutine(ReceiveData());
    }

    IEnumerator ReceiveData()
    {
        string url = "http://localhost:8585";

        while (true) // Bucle infinito para recibir datos continuamente
        {
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

            // Esperar un tiempo antes de la próxima solicitud
            yield return new WaitForSeconds(1.0f); // Ajusta el intervalo según sea necesario
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
                        targetPositions[x, y] = new Vector3(x, 0, y);
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
                        // Actualizar la posición objetivo
                        targetPositions[x, y] = new Vector3(x, 0, y);
                    }
                    else
                    {
                        robots[index] = Instantiate(robotPrefab, new Vector3(x, 0, y), Quaternion.identity);
                        targetPositions[x, y] = new Vector3(x, 0, y);
                    }
                }
            }
        }
    }

    void Update()
    {
        // Mover los robots hacia sus posiciones objetivo
        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                int index = y * gridSizeX + x;
                if (robots != null && robots[index] != null)
                {
                    Rigidbody rb = robots[index].GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 direction = (targetPositions[x, y] - robots[index].transform.position).normalized;
                        rb.MovePosition(robots[index].transform.position + direction * Time.deltaTime * 5f); // Ajusta la velocidad según sea necesario
                    }
                }
            }
        }
    }
}
