using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebClient : MonoBehaviour
{
    IEnumerator SendData()
    {
        // Construye el objeto de posición
        Vector3 fakePos = new Vector3(3.44f, 0, -15.707f);
        string json = JsonUtility.ToJson(fakePos);

        // Configura la solicitud HTTP
        UnityWebRequest www = UnityWebRequest.PostWwwForm("http://localhost:8585", json);
        www.SetRequestHeader("Content-Type", "application/json");

        // Envía la solicitud y espera la respuesta
        yield return www.SendWebRequest();

        // Maneja la respuesta
        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Error: " + www.error);
        }
    }

    void Start()
    {
        StartCoroutine(SendData());
    }
}
