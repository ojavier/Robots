using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Peticion : MonoBehaviour
{
    [System.Serializable]
    public class PositionData
    {
        public int x;
        public int y;
        public int z;
    }

    private void Start()
    {
        StartCoroutine(ReceiveData());
    }

    IEnumerator ReceiveData()
    {
        string url = "http://localhost:8585";

        while (true)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(www.error);
                }
                else if (www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("HTTP Error: " + www.error);
                }
                else
                {
                    Debug.Log("JSON received: " + www.downloadHandler.text); // Agrega este mensaje de depuración
                    try
                    {
                        PositionData position = JsonUtility.FromJson<PositionData>(www.downloadHandler.text);
                        Debug.Log("Position received: " + position.x + ", " + position.y + ", " + position.z);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Error parsing JSON: " + e.Message);
                    }
                }
            }

            yield return new WaitForSeconds(1.0f);
        }
    }
}
