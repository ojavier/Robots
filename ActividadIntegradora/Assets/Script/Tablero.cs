using UnityEngine;

public class Tablero : MonoBehaviour
{
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject binPrefab;
    public GameObject trashPrefab;
    public GameObject robotPrefab;
    private LeerArchivo mapLoader;

    void Start()
    {
        mapLoader = GetComponent<LeerArchivo>();
        if (mapLoader == null)
        {
            Debug.LogError("El componente LeerArchivo no está adjunto al GameObject.");
            return;
        }

        if (mapLoader.officeMap == null || mapLoader.officeMap.Length == 0)
        {
            Debug.LogError("officeMap no está inicializado correctamente.");
            return;
        }

        GenerateMap();
        SpawnRobots();
    }

    void GenerateMap()
    {
        float cellSpacing = 3.0f; // Espaciado entre las celdas
        for (int y = 0; y < mapLoader.height; y++)
        {
            for (int x = 0; x < mapLoader.width; x++)
            {
                Vector3 position = new Vector3(x * cellSpacing, 0, -y * cellSpacing);
                Instantiate(floorPrefab, position, Quaternion.identity);

                char cell = mapLoader.officeMap[y, x];
                if (cell == 'X')
                {
                    Instantiate(wallPrefab, position, Quaternion.identity);
                }
                else if (cell == 'P')
                {
                    Instantiate(binPrefab, position + new Vector3(0, 0, 0), Quaternion.identity); // Y se establece en 0
                }
                else if (char.IsDigit(cell) && cell != '0')
                {
                    int trashCount = int.Parse(cell.ToString());
                    for (int i = 0; i < trashCount; i++)
                    {
                        Vector3 trashPosition = position + new Vector3(Random.Range(-0.4f, 0.4f), -10.37f, Random.Range(-0.4f, 0.4f));
                        Instantiate(trashPrefab, trashPosition, Quaternion.identity);
                    }
                }
            }
        }
    }

    void SpawnRobots()
    {
        for (int y = 0; y < mapLoader.height; y++)
        {
            for (int x = 0; x < mapLoader.width; x++)
            {
                if (mapLoader.officeMap[y, x] == 'S')
                {
                    Vector3 position = new Vector3(x * 3.0f, robotPrefab.transform.localScale.y / 2, -y * 3.0f);
                    Instantiate(robotPrefab, position, Quaternion.identity);
                }
            }
        }
    }
}
