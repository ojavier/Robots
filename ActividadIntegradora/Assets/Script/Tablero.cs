using System.Collections.Generic;
using UnityEngine;

public class Tablero : MonoBehaviour
{
    public GameObject floorPrefab;
    public GameObject binPrefab;
    public GameObject trashPrefab1;
    public GameObject trashPrefab2;
    public GameObject trashPrefab3;
    public GameObject MoreTrash;
    public GameObject robotPrefab;
    public LeerArchivo mapLoader;

    public GameObject[] obstaclePrefabs; // Arreglo de objetos que se colocarán como obstáculos

    private int robotsToSpawn = 5; // Número de robots a crear
    private int robotsSpawned = 0; // Número de robots ya creados

    void Start()
    {
        if (mapLoader == null)
        {
            mapLoader = GetComponent<LeerArchivo>();
            if (mapLoader == null)
            {
                Debug.LogError("El componente LeerArchivo no está adjunto al GameObject.");
                return;
            }
        }
        GenerateMap();
    }

    void GenerateMap()
    {
        float cellSpacing = 3.0f;

        // Usar una semilla aleatoria para la generación de números aleatorios
        Random.InitState((int)System.DateTime.Now.Ticks);

        for (int y = 0; y < mapLoader.height; y++)
        {
            for (int x = 0; x < mapLoader.width; x++)
            {
                Vector3 position = new Vector3(x * cellSpacing, 0, -y * cellSpacing);
                Instantiate(floorPrefab, position, Quaternion.identity);

                char cell = mapLoader.officeMap[y, x];
                if (cell == 'X')
                {
                    PlaceObstacles(position); // Colocar obstáculos en lugar de la pared si es necesario
                }
                else if (cell == 'P')
                {
                    Instantiate(binPrefab, position, Quaternion.identity); // Y se establece en 0
                }
                else if (char.IsDigit(cell) && cell != '0')
                {
                    // Asignar el prefab correspondiente y el desplazamiento en Y según la cantidad de basura
                    (GameObject trashPrefab, float yOffset) = GetTrashPrefab(int.Parse(cell.ToString()));
                    Instantiate(trashPrefab, position + new Vector3(0, yOffset, 0), Quaternion.identity);
                }
                else if (cell == 'S' && robotsSpawned < robotsToSpawn)
                {
                    Instantiate(robotPrefab, position + new Vector3(0, robotPrefab.transform.localScale.y / 2, 0), Quaternion.identity);
                    robotsSpawned++;
                }
                else
                {
                    // Cualquier otra celda se ignora o se puede agregar un manejo adicional si es necesario
                }
            }
        }
    }

    void PlaceObstacles(Vector3 position)
    {
        // Crea una lista para almacenar las posiciones donde se colocarán los obstáculos
        List<Vector3> obstaclePositions = new List<Vector3>();

        Vector3 obstaclePosition = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
        obstaclePositions.Add(obstaclePosition);

        // Selecciona un prefab aleatorio de la lista de obstáculos
        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

        // Instancia el prefab en la posición calculada
        Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);

        // Dibujar un gizmo en la posición del obstáculo
        Debug.DrawLine(obstaclePosition, obstaclePosition + Vector3.up * 2, Color.red, 5.0f);
    }

    (GameObject, float) GetTrashPrefab(int trashCount)
    {
        switch (trashCount)
        {
            case 1:
                return (trashPrefab1, 0.49f);
            case 2:
                return (trashPrefab2, 2.3f);
            case 3:
                return (trashPrefab3, 0.1f);
            default:
                return (MoreTrash, -17.2f);
        }
    }
}
