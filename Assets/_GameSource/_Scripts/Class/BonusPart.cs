using UnityEngine;

public class BonusPart : MonoBehaviour
{
    #region Variables

    [SerializeField] int multipier;
    [SerializeField] Color myColor;

    MeshRenderer renderer;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boss"))
        {
            Destroy(GetComponent<BoxCollider>());
            Paint();
            Destroy(this);
        }
    }

    #endregion

    #region Other Methods

    void Paint()
    {
        renderer.material.color = myColor;
        GameManager.Instance.SetBonus(multipier);
    }

    #endregion
}
