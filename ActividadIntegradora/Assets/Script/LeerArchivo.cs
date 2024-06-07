using System.IO;
using UnityEngine;

public class LeerArchivo : MonoBehaviour
{
    public string fileName = "input1.txt";  // Solo el nombre del archivo, sin ruta
    public char[,] officeMap;
    public int width;
    public int height;
    public Vector3 binPosition;

    void Start()
    {
        string filePath = Path.Combine(Application.dataPath, fileName); // Obtiene la ruta completa al archivo
        if (File.Exists(filePath))
        {
            Leer(filePath);
        }
        else
        {
            Debug.LogError("El archivo no se encuentra en la ruta especificada: " + filePath);
        }
    }

    void Leer(string filePath)
    {
        Debug.Log("Cargando el mapa desde " + filePath);
        string[] lines = File.ReadAllLines(filePath);
        string[] dimensions = lines[0].Split(' ');
        height = int.Parse(dimensions[0]);
        width = int.Parse(dimensions[1]);

        officeMap = new char[height, width];

        for (int i = 1; i <= height; i++)
        {
            string[] row = lines[i].Split(' ');
            for (int j = 0; j < width; j++)
            {
                officeMap[i - 1, j] = row[j][0];
                if (officeMap[i - 1, j] == 'P')
                {
                    binPosition = new Vector3(j * 2.0f, 0, -(i - 1) * 2.0f);
                }
            }
        }

        Debug.Log("Mapa cargado correctamente. Dimensiones: " + height + "x" + width);
    }
}
