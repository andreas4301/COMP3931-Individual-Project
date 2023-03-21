using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CubeClickHandler : MonoBehaviour
{

    public string vmID; // ID of the associated virtual machine
    public GameObject cube;
    private void OnMouseDown()
    {
        StartCoroutine(ButtonClicked());
    }

    private IEnumerator ButtonClicked()
    {
        // Create a UnityWebRequest object with the correct headers and URL
        string url = "http://127.0.0.1:8697/api/vms/" + vmID + "/power";
        string auth = "andreastric" + ":" + "Andreas4301!";
        string authEncoded = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(auth));
        string authHeader = string.Format("Basic {0}", authEncoded);
        string power = "";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", authHeader);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // Parse the JSON response to get the power state
            string jsonResponse = request.downloadHandler.text;
            PowerState powerState = JsonUtility.FromJson<PowerState>(jsonResponse);
            string state = powerState.power_state;
            Debug.Log("Power State of VM " + vmID + ": " + state);

            // Change the color of the game object based on power state
            if (state == "poweredOn")
            {
                power = "off";
            }
            else
            {
                power = "on";
            }
        }
                    
        UnityWebRequest request2 = UnityWebRequest.Put(url, "@-");
        request2.method = UnityWebRequest.kHttpVerbPUT;
        request2.SetRequestHeader("Authorization", authHeader);
        request2.SetRequestHeader("Content-Type", "application/vnd.vmware.vmw.rest-v1+json");
        request2.SetRequestHeader("Accept", "application/vnd.vmware.vmw.rest-v1+json");

        // Set the request body
        string requestBody = power;
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
        request2.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request2.downloadHandler = new DownloadHandlerBuffer();
        // Send the request
        yield return request2.SendWebRequest();
        // Download the response body
        byte[] responseBody = request2.downloadHandler.data;

        // Deserialize the response manually
        string responseString = Encoding.UTF8.GetString(responseBody);
        Debug.Log(responseString);
        //VMPowerOperationResponse response = JsonUtility.FromJson<VMPowerOperationResponse>(responseString);

        Debug.Log(request2.result);
        if (request2.result == UnityWebRequest.Result.ConnectionError || request2.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request2.error);
        }
        else{
            if(power=="off") cube.GetComponent<Renderer>().material.color = new Color(6f/255f, 171f/255f, 4f/255f);
            else cube.GetComponent<Renderer>().material.color = Color.blue;
        }
        yield break;
    }
    [System.Serializable]
    private class PowerState
    {
        public string power_state;
    }
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
