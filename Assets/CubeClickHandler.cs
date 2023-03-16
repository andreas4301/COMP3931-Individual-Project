using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VMPowerOperation
{
    public string[] @string;
    public string stringEnum;
}

public enum VMPowerOperationType {
    On,
    Off,
    Shutdown,
    Suspend
}


public class CubeClickHandler : MonoBehaviour
{
    public string vmID; // ID of the associated virtual machine

    private void OnMouseDown()
    {
        StartCoroutine(ButtonClicked());
    }

    private IEnumerator ButtonClicked()
    {
        // Create a VMPowerOperation object with the "on" operation
VMPowerOperation powerOperation = new VMPowerOperation();
powerOperation.@string = new string[] { "on" };
powerOperation.stringEnum = VMPowerOperationType.On.ToString("G");

// Serialize the VMPowerOperation object to JSON
string jsonContent = JsonUtility.ToJson(powerOperation);
        string auth = "andreastric" + ":" + "Andreas4301!";
        string authEncoded = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(auth));
        string authHeader = string.Format("Basic {0}", authEncoded);
        string url = "http://127.0.0.1:8697/api/vms/" + vmID + "/power";
        UnityWebRequest request2 = UnityWebRequest.Put(url, jsonContent);
        request2.method = UnityWebRequest.kHttpVerbPUT;
        request2.SetRequestHeader("Authorization", authHeader);
        request2.SetRequestHeader("Content-Type", "application/vnd.vmware.vmw.rest-v1+json");
request2.SetRequestHeader("Accept", "application/vnd.vmware.vmw.rest-v1+json");
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
        //yield break;
    }
}
