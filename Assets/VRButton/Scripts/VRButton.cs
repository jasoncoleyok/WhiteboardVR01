using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

// Custom UnityEvents that we can see and set in the inspector
[System.Serializable]
public class VRButtonPressed : UnityEvent<SelectEnterEventArgs> { }

[System.Serializable]
public class VRButtonReleased : UnityEvent<SelectExitEventArgs> { }

public class VRButton : XRBaseInteractable
{
    // UnityEvents that will be triggered when the button is pressed and released
    public VRButtonPressed onButtonPressed;
    public VRButtonReleased onButtonReleased;

    private bool buttonPressed = false;
    private float threshold = 0.5f; // set this to the distance the button needs to be pressed

    private Vector3 initialPosition;

    // We use the Start method to save the initial position of the button
    void Start()
    {
        initialPosition = transform.position;
        Debug.Log("Button's initial position: " + initialPosition);
    }

    // The Update method checks the button's position each frame
    void Update()
    {
        // Continuously monitor the button's position during runtime
        Debug.Log("Button's current position: " + transform.position);

        // Check if the button has moved past the threshold
        if (!buttonPressed && Vector3.Distance(transform.position, initialPosition) > threshold)
        {
            buttonPressed = true;
            Debug.Log("Button pressed.");
            // We need to invoke the event when the button is pressed
            onButtonPressed.Invoke(new SelectEnterEventArgs());
        }

        // Check if the button has moved back past the threshold
        if (buttonPressed && Vector3.Distance(transform.position, initialPosition) < threshold)
        {
            buttonPressed = false;
            Debug.Log("Button released.");
            // We need to invoke the event when the button is released
            onButtonReleased.Invoke(new SelectExitEventArgs());
        }
    }
}