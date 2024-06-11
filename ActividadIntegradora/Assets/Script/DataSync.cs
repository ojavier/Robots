using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DataSync : MonoBehaviour
{
    [System.Serializable]
    public class AgentData
    {
        public int id;
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    public class AgentsResponse
    {
        public List<AgentData> agents;
    }

    public GameObject agentPrefab;
    private Dictionary<int, GameObject> agents = new Dictionary<int, GameObject>();

    void Start()
    {
        StartCoroutine(UpdateAgents());
    }

    IEnumerator UpdateAgents()
    {
        while (true)
        {
            string url = "http://localhost:8585/step";

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.downloadHandler = new DownloadHandlerBuffer();

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    string jsonResponse = www.downloadHandler.text;
                    // Debug.Log("Respuesta del servidor: " + jsonResponse);

                    try
                    {
                        AgentsResponse response = JsonUtility.FromJson<AgentsResponse>(jsonResponse);
                        UpdateAgentPositions(response);
                    }
                    catch (System.ArgumentException e)
                    {
                        Debug.LogError("JSON parse error: " + e.Message);
                        Debug.LogError("Respuesta JSON: " + jsonResponse);
                    }
                }
            }

            yield return new WaitForSeconds(1.0f); // Ajusta el intervalo de actualización
        }
    }

    void UpdateAgentPositions(AgentsResponse response)
    {
        foreach (AgentData agent in response.agents)
        {
            if (!agents.ContainsKey(agent.id))
            {
                GameObject newAgent = Instantiate(agentPrefab, new Vector3(agent.x, agent.y, agent.z), Quaternion.identity);
                agents[agent.id] = newAgent;
            }
            else
            {
                GameObject existingAgent = agents[agent.id];
                existingAgent.transform.position = new Vector3(agent.x, agent.y, agent.z);
            }
        }
    }
}
