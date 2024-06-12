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
                List<Vector3> vectorList = ParseVector3Array(jsonString);
        
                // Convertir la lista a un array
                Vector3[] vectorArray = vectorList.ToArray();
                
                // Imprimir los resultados
                // foreach (Vector3 vector in vectorArray)
                // {
                //     Debug.Log("vectores3 de los goodd" + vector);
                // }        
                for (int i=0; i<5; i++){
                    robots[i].gameObject.transform.position = vectorArray[i];
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

    List<Vector3> ParseVector3Array(string data)
    {
        List<Vector3> vectorList = new List<Vector3>();
        
        // Extraer la parte del string que contiene los datos
        int startIndex = data.IndexOf("[[") + 2;
        int endIndex = data.LastIndexOf("]]");
        string dataString = data.Substring(startIndex, endIndex - startIndex);

        // Eliminar corchetes adicionales
        dataString = dataString.Replace("[", "").Replace("]", "");

        // Imprimir dataString para verificar su formato
        // Debug.Log("Data string: " + dataString);
        
        // Separar los elementos por comas
        string[] vectorStrings = dataString.Split(new char[] {','}, System.StringSplitOptions.RemoveEmptyEntries);

        // Imprimir vectorStrings para verificar su contenido
        // foreach (string str in vectorStrings)
        // {
        //     Debug.Log("Vector string: " + str);
        // }

        // Crear el array de Vector3
        for (int i = 0; i < vectorStrings.Length; i += 3)
        {
            float x = float.Parse(vectorStrings[i]);
            float y = float.Parse(vectorStrings[i + 1]);
            float z = float.Parse(vectorStrings[i + 2]);
            vectorList.Add(new Vector3(x, y, z));
        }
        
        return vectorList;
    }

}
