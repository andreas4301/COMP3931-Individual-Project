using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CubeClickHandler : MonoBehaviour
{
    public string vmID; // ID of the associated virtual machine
    public string power;
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
        yield break;
    }
}
