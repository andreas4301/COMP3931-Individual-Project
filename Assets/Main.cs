using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    // Define classes for JSON deserialization
    [Serializable]
    private class PowerState
    {
        public string power_state;
    }
    [Serializable]
    private class VM
    {
        public string id;
        public string path;
    }
    [Serializable]
    private class CPU
    {
        public int processors;
    }
    [Serializable]
    private class VMSettings
    {
        public string id;
        public CPU cpu;
        public int memory;
    }
    
    // Define variables to store VM and cube data
    private VM[] vms;
    private GameObject[] cuboids;
    public GameObject cubePrefab;

    // Coroutine to initialize VMs and adjust their appearance
    public IEnumerator Start()
    {
        // Define authentication header for API requests
        string auth = "andreastric" + ":" + "Andreas4301!";
        string authEncoded = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(auth));
        string authHeader = string.Format("Basic {0}", authEncoded);
        // Find the hypervisor game object and change its color
        GameObject hypervisor = GameObject.Find("Hypervisor");
        hypervisor.GetComponent<Renderer>().material.color = Color.white;
        Transform hypervisorTransform = hypervisor.GetComponent<Transform>();
        Vector3 currentScale = hypervisorTransform.localScale;
        // Send API request to get VM data
        string url = "http://127.0.0.1:8697/api/vms";
        string jsonResponse;
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", authHeader);
        yield return request.SendWebRequest();
        // Check if request was successful and parse JSON data to get VM data
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            jsonResponse = request.downloadHandler.text;
            vms = JsonHelper.FromJson<VM>(jsonResponse);
        }
        // Resize array of cuboids to match number of VMs and resize hypervisor game object
        int size = vms.Length;
        Array.Resize(ref cuboids, size);
        hypervisorTransform.localScale = new Vector3(currentScale.x + 8*size, currentScale.y, currentScale.z + 8*size);
        // Loop through each VM and create a cuboid with Click component
        for (int i = 0; i < size; i++)
        {
            GameObject newVM = Instantiate(cubePrefab, new Vector3(50-3*i, 0, 50-3*i), Quaternion.identity);
            cuboids[i] = newVM;
            Click clickHandler = cuboids[i].AddComponent<Click>();
            string vmID = vms[i].id;
            clickHandler.vmID = vmID;
            clickHandler.cuboid = cuboids[i];
            // Send API request to get power state data
            url = "http://127.0.0.1:8697/api/vms/" + vmID + "/power";
            request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", authHeader);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            jsonResponse = request.downloadHandler.text;
            PowerState powerState = JsonUtility.FromJson<PowerState>(jsonResponse);
            string power = powerState.power_state;
            // Send API request to get VM settings
            url = "http://127.0.0.1:8697/api/vms/" + vmID;
            request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", authHeader);
            yield return request.SendWebRequest();
            int memory, processors;
            // Check if request was successful and parse JSON data to get VM settings
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            jsonResponse = request.downloadHandler.text;
            VMSettings vmSettings = JsonUtility.FromJson<VMSettings>(jsonResponse);
            
            memory = vmSettings.memory;
            processors = vmSettings.cpu.processors;
            //Calls getBrightness() method to get brightness value
            float b = getBrightness(processors);
            //Adds values to Click component
            clickHandler.state = power;
            clickHandler.shade = b;
            //Calls editCuboid() method to adjust VM appearance
            editCuboid(i, power, memory, b);
            
        }
    }

    // Method to update size and colour of VM based on the power state, memory, and processor core count
    private void editCuboid(int i, string power, int memory, float shade){
        // Get current scale and position of VM
        Vector3 scale = cuboids[i].transform.localScale;
        Vector3 position = cuboids[i].transform.localPosition;
        // Update the height of VM based on the memory of the virtual machine
        // Linearly interpolated between a minimum of 0.5 and a maximum of 50 based on memory
        scale.y = Mathf.Lerp(0.5f, 50f, Mathf.InverseLerp(4, 128*1024,memory));

        // Update position of VM based on new height.
        position.y = 0.5f + scale.y / 2;

        // Apply the new scale and positio
        cuboids[i].transform.localScale = scale;
        cuboids[i].transform.localPosition = position;

        // Set colour of VM based on power state
        if (power == "poweredOn")
            // If it on, it will be green with a brightness level determined by the processor core count
            cuboids[i].GetComponent<Renderer>().material.color = new Color(0f, shade, 0f);
        else
            // If it off, it will be red with a brightness level determined by the processor core count
            cuboids[i].GetComponent<Renderer>().material.color =  new Color(shade,0f, 0f);
    }

    // Method used to determine brightness level based on number of processor cores
    private float getBrightness(int processorCount) {
        switch(processorCount) {
            case 1:
                return 1.0f;
            case 2:
                return 0.9f;
            case 3:
                return 0.8f;
            case 4:
                return 0.7f;
            case 6:
                return 0.6f;
            case 8:
                return 0.5f;
            case 12:
                return 0.4f;
            case 16:
                return 0.3f;
            case 24:
                return 0.2f;
            case 32:
                return 0.1f;
            default:
                return 0.0f;
        }
    }

    // The following piece of code is taken from a thread in Unity forums. More information can be found in Appendix B of the report.
    // This class provides a helper method for parsing JSON arrays using Unity's JsonUtility class
    private static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            // Adds a wrapper around the JSON array to allow it to be parsed by Unity's JsonUtility class
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        // This class provides a simple wrapper around the JSON array to allow it to be parsed by Unity's JsonUtility class.
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
