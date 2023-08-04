using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whiteboard : MonoBehaviour
{
    // Declare a Texture2D object to represent the whiteboard's drawable surface
    public Texture2D texture;

    // Declare a Vector2 object to represent the dimensions of the whiteboard
    public Vector2 textureSize = new Vector2(x: 2048, y: 2048);

    void Start()
    {
        // Get the Renderer component attached to the GameObject this script is attached to
        var r = GetComponent<Renderer>();

        if (r == null)
        {
            // Debug log message to indicate that no Renderer component was found
            Debug.Log("No Renderer component found on this GameObject. A Renderer is needed for the whiteboard to function properly.");
        }
        else
        {
            // Create a new Texture2D with the specified dimensions
            texture = new Texture2D((int)textureSize.x, (int)textureSize.y);

            // Set this new texture as the main texture of the Renderer's material
            // This is what makes the whiteboard appear on the GameObject
            r.material.mainTexture = texture;

            // Debug log message to indicate that the whiteboard setup was successful
            Debug.Log("Whiteboard setup complete. The whiteboard is ready to be drawn on.");
        }
    }
}