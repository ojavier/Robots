using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SendJsonToServer : MonoBehaviour
{
    [System.Serializable]
    public class GridData
    {
        public int step;
        public int[][] grid;
    }

    void Start()
    {
        // Datos a enviar
        GridData data = new GridData
        {
            step = 1,
            grid = new int[][]
            {
                new int[] { 0, 0, 8 },
                new int[] { 0, 3, 0 },
                new int[] { 2, 0, 0 }
            }
        };

        // Convertir los datos a JSON
        string jsonData = JsonUtility.ToJson(data);

        // Iniciar la corrutina para enviar los datos
        StartCoroutine(SendData(jsonData));
    }

    IEnumerator SendData(string jsonData)
    {
        string url = "http://localhost:8585";

        // Crear una solicitud POST
        using (UnityWebRequest www = UnityWebRequest.Post(url, ""))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // Enviar la solicitud y esperar la respuesta
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Respuesta del servidor: " + www.downloadHandler.text);
            }
        }
    }
}
