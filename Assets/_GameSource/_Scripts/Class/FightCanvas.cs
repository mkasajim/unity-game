using UnityEngine;

public class FightCanvas : MonoBehaviour
{
    #region Variables

    Camera camera;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        camera = Camera.main;
    }

    private void Update()
    {
        transform.LookAt(camera.transform);
    }

    #endregion
}
