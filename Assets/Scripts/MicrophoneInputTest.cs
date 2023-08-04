using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneInputTest : MonoBehaviour
{
    void Start()
    {
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }
    }
}