using System.Collections.Generic;
using UnityEngine;

public class Tablero : MonoBehaviour
{
    // Declarar objetos y variables
    public GameObject floorPrefab;
    public GameObject binPrefab;
    public GameObject trashPrefab1;
    public GameObject trashPrefab2;
    public GameObject trashPrefab3;
    public GameObject MoreTrash;
    public GameObject robotPrefab;
    public LeerArchivo mapLoader;

    public GameObject[] obstaclePrefabs;
    private int robotsToSpawn = 5;
    private int robotsSpawned = 0;

    // Este método es llamado al iniciar el programa
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

    // Función para generar el tablero usando mapLoader con los datos del archivo txt 
    void GenerateMap()
    {
        float cellSpacing = 3.0f;
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
                    PlaceObstacles(position);
                }
                else if (cell == 'P')
                {
                    Instantiate(binPrefab, position, Quaternion.identity);
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
            }
        }
    }

    // Función para colocar los obstáculos usando una lista y los coloca de forma aleatoria
    void PlaceObstacles(Vector3 position)
    {
        List<Vector3> obstaclePositions = new List<Vector3>();

        Vector3 obstaclePosition = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
        obstaclePositions.Add(obstaclePosition);

        GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);

        Debug.DrawLine(obstaclePosition, obstaclePosition + Vector3.up * 2, Color.red, 5.0f);
    }

    // Coloca pocisiones diferentes para cada prefab de diferentes basuras 
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
