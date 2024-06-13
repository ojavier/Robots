using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Peticion : MonoBehaviour
{
    // Clase para deserializar el JSON de pasos
    [System.Serializable]
    public class StepData
    {
        public int data;
    }

    // Clase para deserializar el JSON de posiciones de los robots (x, y, z)
    [System.Serializable]
    public class RobotPositionData
    {
        public List<List<float>> data; 
    }

    // Definir arreglo de GameObjects, variable para pasos
    private int stepValue;
    public GameObject[] robots = new GameObject[5];

    // Este método es llamado al iniciar el programa iniciando la corutina para recibir el valor de los pasos
    private void Start()
    {
        StartCoroutine(ReceiveSteps());
    }

    // Corutina para conocer la secuencia para obtener los datos de posiciones de los robots
    IEnumerator fetchData()
    {
        Debug.Log("holaenfetch" + stepValue.ToString());
        for (int i = 0; i < stepValue; i++)
        {
            yield return StartCoroutine(ReceiveData(i));
            yield return new WaitForSeconds(0.3f);
        }
    }

    // Corutina para recibir los datos de una posición específica de los robots
    IEnumerator ReceiveData(int numStep)
    {
        string url = "http://localhost:8585/default/" + numStep.ToString();
        Debug.Log(url);

        // Envía la solicitud y espera la respuesta
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

                // Deserializa el JSON a un objeto RobotPositionData
                RobotPositionData positionData = JsonUtility.FromJson<RobotPositionData>(jsonString);
                List<Vector3> vectorList = new List<Vector3>();

                // Convierte las listas de floats en vectores y los añade a la lista de vectores
                foreach (var position in positionData.data)
                {
                    if (position.Count == 3)
                    {
                        Vector3 vector = new Vector3(position[0], position[1], position[2]);
                        vectorList.Add(vector);
                    }
                }

                Vector3[] vectorArray = vectorList.ToArray();

                // Actualiza la posición de los robots en la escena
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

    // Corutina para recibir el número total de pasos desde el servidor, la URL del servidor obtiene el número de pasos
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
        // Inicia la corutina para recibir datos de posiciones de los robots
        StartCoroutine(fetchData());
    }
}
