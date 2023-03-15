using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class VMWareAPI : MonoBehaviour
{
    IEnumerator Start()
    {
        // Set up authentication headers
        string auth = "andreastric:Andreas4301!";
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
            Debug.Log(request.downloadHandler.text);
        }
    }
}