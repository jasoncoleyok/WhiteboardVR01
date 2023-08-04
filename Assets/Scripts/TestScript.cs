using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Add a public field for the ConvaiNPC instance
    public ConvaiNPC convaiNPC;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            // Use the ConvaiNPC instance to call InvokeOnButtonPressed
            convaiNPC.InvokeOnButtonPressed();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // Use the ConvaiNPC instance to call InvokeOnButtonReleased
            convaiNPC.InvokeOnButtonReleased();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            convaiNPC.InvokeOnButtonPressed();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            convaiNPC.InvokeOnButtonReleased();
        }
    }
}