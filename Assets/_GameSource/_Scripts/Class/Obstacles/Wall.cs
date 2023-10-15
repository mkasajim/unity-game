using UnityEngine;

public class Wall : MonoBehaviour
{
    #region Variables

    [SerializeField] float damageValue;

    Rigidbody[] rigidbodies;

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(GetComponent<BoxCollider>());
            CanBreak();
            Destroy(gameObject, 3f);
        }
    }

    #endregion

    #region Other Methods

    void CanBreak()
    {
        HapticManager.Instance.Vibrate();
        foreach (Rigidbody rb in rigidbodies)
            rb.isKinematic = false;
        PlayerController.Instance.WrongColor(damageValue, false);
        PlayerController.Instance.UpdateCookieText();
    }

    #endregion
}
