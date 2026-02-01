using UnityEngine;

[ExecuteAlways] // Makes this run in the Editor and Play mode
public class ManualBaseMapOffset : MonoBehaviour
{
    [Header("Assign your Material here")]
    public Material material; // Drag your material here

    [Header("Texture Offset (U, V)")]
    public Vector2 offset;    // Editable in Inspector

    void Update()
    {
        if (material != null)
        {
            material.SetTextureOffset("_BaseMap", offset);
        }
    }
}