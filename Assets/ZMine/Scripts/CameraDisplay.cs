
using UdonSharp;
using VRC.Udon;

using UnityEngine;
using VRC.SDKBase;

public class CameraDisplay : UdonSharpBehaviour
{
    [SerializeField] private Camera displayCamera;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Material displayMaterial;

    void Start()
    {
        if (displayCamera != null)
        {
            // Assign the render texture to the camera
            displayCamera.targetTexture = renderTexture;
            
            // Assign the render texture to the material
            if (displayMaterial != null)
            {
                displayMaterial.mainTexture = renderTexture;
            }
        }
    }
}