using UnityEngine;

public class Cookies : MonoBehaviour
{
    #region Variables

    // PRIVATE COMPONENTS
 
    private SkinnedMeshRenderer meshRenderer;

    // PRIVATE VARIABLES
    Color currentColor;

    #endregion

    private MeshRenderer renderer;

    #region MonoBehaviour Callbacks

    private void Start()
    {
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        if (meshRenderer != null)
            currentColor = meshRenderer.material.color;
        else
        {
            renderer = GetComponentInChildren<MeshRenderer>();
            currentColor = renderer.material.color;
        }
    }

    #endregion

    #region Other Methods

    public Color GetColor()
    {
        return currentColor;
    }

    #endregion
}
