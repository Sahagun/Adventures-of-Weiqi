using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public RawImage image;  // Assign in Inspector
        public float depthFactor = 1f; // Determines how much the layer moves
        [HideInInspector] public Vector2 originalUVOffset; // Stores the initial UV offset
    }

    public List<ParallaxLayer> layers = new List<ParallaxLayer>(); // List of layers
    public float parallaxIntensity = 50f;  // Adjust sensitivity
    public float smoothSpeed = 5f;         // Speed of movement smoothing
    public float maxOffsetX = 0.2f;        // Max horizontal UV movement
    public float maxOffsetY = 0.2f;        // Max vertical UV movement
    public float speedNormalization = 500f; // Adjust this based on your character speed
    public float speedMultiplier = 0.1f;    // Fine-tune the effect for fast movement

    private Vector2 _currentOffset;
    private Vector3 _previousCameraPosition;

    void Start ()
    {
        // Get the initial camera position
        _previousCameraPosition = Camera.main.transform.position;

        // Store initial UV positions for each layer
        foreach (var layer in layers)
        {
            if (layer.image != null)
            {
                layer.originalUVOffset = layer.image.uvRect.position;
            }
        }
    }

    void LateUpdate ()
    {
        // Get the current camera position
        Vector3 currentCameraPosition = Camera.main.transform.position;

        // Calculate the difference between the current and previous positions
        Vector3 cameraDelta = (currentCameraPosition - _previousCameraPosition) / speedNormalization;

        // Update previous camera position for the next frame
        _previousCameraPosition = currentCameraPosition;

        // Calculate the parallax movement based on normalized camera movement
        Vector2 parallaxMovement = new Vector2(cameraDelta.x,cameraDelta.y) * parallaxIntensity * speedMultiplier;

        // Smooth out the offset movement
        _currentOffset = Vector2.Lerp(_currentOffset,parallaxMovement,smoothSpeed * Time.deltaTime);

        foreach (var layer in layers)
        {
            if (layer.image != null)
            {
                // Calculate new UV offset
                Vector2 newUVOffset = layer.originalUVOffset + (_currentOffset * layer.depthFactor / parallaxIntensity);

                // Clamp the UV offset within separate horizontal and vertical limits
                newUVOffset.x = Mathf.Clamp(newUVOffset.x,layer.originalUVOffset.x - maxOffsetX,layer.originalUVOffset.x + maxOffsetX);
                newUVOffset.y = Mathf.Clamp(newUVOffset.y,layer.originalUVOffset.y - maxOffsetY,layer.originalUVOffset.y + maxOffsetY);

                // Apply to the RawImage
                layer.image.uvRect = new Rect(newUVOffset.x,newUVOffset.y,layer.image.uvRect.width,layer.image.uvRect.height);
            }
        }
    }
}
