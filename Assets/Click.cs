using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
// This script is attached to a GameObject that can be clicked to power on/off a virtual machine (VM).
public class Click : MonoBehaviour
{
    // Variables that hold information about the VM, the GameObject representing the VM, the current power state of the VM, and its brightness
    public string vmID; // ID of the associated virtual machine
    public GameObject cuboid;
    public string state;
    public float shade;
    // When the button is clicked, it will trigger the ButtonClicked coroutine
    private void OnMouseDown()
    {
        StartCoroutine(ButtonClicked());
    }
    // Coroutine that will send a request to power on/off the VM when it's clicked and change its colour
    private IEnumerator ButtonClicked()
    {
        // Create authorization header
        string auth = "andreastric" + ":" + "Andreas4301!";
        string authEncoded = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(auth));
        string authHeader = string.Format("Basic {0}", authEncoded);

        // Determine whether the virtual machine should be powered on or off
        string power;
        if (state == "poweredOn") power = "off";
        else power = "on";

        string url = "http://127.0.0.1:8697/api/vms/" + vmID + "/power";
        // Create PUT request
        UnityWebRequest request = UnityWebRequest.Put(url, "@-");
        request.method = UnityWebRequest.kHttpVerbPUT;
        request.SetRequestHeader("Authorization", authHeader);
        request.SetRequestHeader("Content-Type", "application/vnd.vmware.vmw.rest-v1+json");
        request.SetRequestHeader("Accept", "application/vnd.vmware.vmw.rest-v1+json");
        // Set request body to desired power state
        string requestBody = power;
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        // Send request
        yield return request.SendWebRequest();

        // Extract response
        byte[] responseBody = request.downloadHandler.data;
        string responseString = Encoding.UTF8.GetString(responseBody);
        // Check if request was successful
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else{
            // Update VM colour and power state
            if(power=="off"){
                cuboid.GetComponent<Renderer>().material.color =  new Color(shade, 0f, 0f);
                state="poweredOff";
            }
            else{
                cuboid.GetComponent<Renderer>().material.color = new Color(0f, shade, 0f);
                state="poweredOn";
            }
        }
        yield break;
    }
}
