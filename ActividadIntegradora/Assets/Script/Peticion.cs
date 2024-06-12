using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Peticion : MonoBehaviour
{
    [System.Serializable]
    public class RobotData
    {
        public int id;
        public int x;
        public int y;
        public int movements;
    }

    [System.Serializable]
    public class ServerData
    {
        public int time_step;
        public RobotData[] robots;
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

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log("JSON received: " + jsonResponse);

                    try
                    {
                        // First, check if the JSON is not empty
                        if (!string.IsNullOrEmpty(jsonResponse))
                        {
                            // Try to parse the JSON
                            ServerData data = JsonUtility.FromJson<ServerData>(jsonResponse);

                            // Check if data and its fields are not null
                            if (data != null && data.robots != null)
                            {
                                Debug.Log("Parsed data successfully.");
                                // Iterate over the robots and log their data
                                foreach (RobotData robot in data.robots)
                                {
                                    Debug.Log($"Robot {robot.id} at ({robot.x}, {robot.y}) with {robot.movements} movements");
                                }
                            }
                            else
                            {
                                Debug.LogError("Parsed data or robots array is null.");
                            }
                        }
                        else
                        {
                            Debug.LogError("Received empty JSON response.");
                        }
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
