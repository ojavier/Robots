using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Peticion : MonoBehaviour
{
    [System.Serializable]
    public class StepData
    {
        public int data;
    }

    [System.Serializable]
    public class RobotPositionData
    {
        public List<List<float>> data;
    }

    private int stepValue;
    public GameObject[] robots = new GameObject[5];

    private void Start()
    {
        StartCoroutine(ReceiveSteps());
    }

    IEnumerator fetchData()
    {
        Debug.Log("holaenfetch" + stepValue.ToString());
        for (int i = 0; i < stepValue; i++)
        {
            yield return StartCoroutine(ReceiveData(i));
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator ReceiveData(int numStep)
    {
        string url = "http://localhost:8585/default/" + numStep.ToString();
        Debug.Log(url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || 
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                string jsonString = www.downloadHandler.text;
                Debug.Log("datos" + jsonString);

                RobotPositionData positionData = JsonUtility.FromJson<RobotPositionData>(jsonString);
                List<Vector3> vectorList = new List<Vector3>();

                foreach (var position in positionData.data)
                {
                    if (position.Count == 3)
                    {
                        Vector3 vector = new Vector3(position[0], position[1], position[2]);
                        vectorList.Add(vector);
                    }
                }

                Vector3[] vectorArray = vectorList.ToArray();
                for (int i = 0; i < robots.Length; i++)
                {
                    if (i < vectorArray.Length)
                    {
                        robots[i].transform.position = vectorArray[i];
                    }
                }
            }
        }
    }

    IEnumerator ReceiveSteps()
    {
        string url = "http://localhost:8585/steps";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || 
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                string dataString = www.downloadHandler.text;
                Debug.Log("datossteps" + dataString);

                StepData stepData = JsonUtility.FromJson<StepData>(dataString);
                stepValue = stepData.data;

                Debug.Log("Step value: " + stepValue);
            }
        }
        StartCoroutine(fetchData());
    }
}
