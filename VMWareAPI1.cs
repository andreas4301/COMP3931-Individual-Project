using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class VMWareAPI1 : MonoBehaviour
{
    public GameObject[] myGameObjects;
    
    IEnumerator Start()
    {
        // Set up authentication headers
        string username = "andreastric";
        string password = "Andreas4301!";
        string auth = username + ":" + password;
        string authEncoded = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(auth));
        string authHeader = string.Format("Basic {0}", authEncoded);

        // Set up request parameters
        string url = "http://127.0.0.1:8697/api/vms";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", authHeader);

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // Parse the JSON response
            string jsonResponse = request.downloadHandler.text;
            VM[] vms = JsonHelper.FromJson<VM>(jsonResponse);

            for (int i = 0; i < 2; i++)
            {
                string vmID = vms[i].id;

                // Get power state of the first VM
                url = "http://127.0.0.1:8697/api/vms/" + vmID + "/power";
                request = UnityWebRequest.Get(url);
                request.SetRequestHeader("Authorization", authHeader);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    // Parse the JSON response to get the power state
                    jsonResponse = request.downloadHandler.text;
                    PowerState powerState = JsonUtility.FromJson<PowerState>(jsonResponse);
                    string state = powerState.power_state;

                    Debug.Log("Power State of VM " + vmID + ": " + state);

                    // Change the color of the game object based on power state
                    if (state == "poweredOn")
                    {
                        myGameObjects[i].GetComponent<Renderer>().material.color = Color.blue;
                    }
                    else
                    {
                        myGameObjects[i].GetComponent<Renderer>().material.color =  new Color(6f/255f, 171f/255f, 4f/255f);
                    }
                    // Get settings of the first VM
                    url = "http://127.0.0.1:8697/api/vms/" + vmID;
                    request = UnityWebRequest.Get(url);
                    request.SetRequestHeader("Authorization", authHeader);

                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError(request.error);
                    }
                    else
                    {
                        // Parse the JSON response to get the VM settings
                        jsonResponse = request.downloadHandler.text;
                        VMSettings vmSettings = JsonUtility.FromJson<VMSettings>(jsonResponse);
                        int processors = vmSettings.cpu.processors;
                        int memory = vmSettings.memory;

                        Debug.Log("Processors of VM " + vmID + ": " + processors);
                        Debug.Log("Memory of VM " + vmID + ": " + memory);
                        SetGameObjectSize(i, memory);
                        
                    }
                }
            }
        }
    }

    // PowerState class to hold JSON data
    [System.Serializable]
    private class PowerState
    {
        public string power_state;
    }

    // VMSettings class to hold JSON data
    [System.Serializable]
    private class VMSettings
    {
        public string id;
        public CPU cpu;
        public int memory;
    }

    [System.Serializable]
    private class CPU
    {
        public int processors;
    }

    [System.Serializable]
    private class VM
    {
        public string id;
        public string path;
    }

    void SetGameObjectSize(int i, float memory)
    {
        Vector3 scale = myGameObjects[i].transform.localScale;
        scale.z = 8 * (memory / 8192f); // Scale between 0.1 and 0.5 based on memory (max 8192)
        myGameObjects[i].transform.localScale = scale;
    }

    // Helper class to parse JSON arrays
    private static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        private class Wrapper<T>
        {
            public T[] array;
        }
    }

}
