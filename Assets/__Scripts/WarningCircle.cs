using UnityEngine;

public class WarningCircle : MonoBehaviour
{
    public float duration = 1.5f;

    void Start()
    {
        Destroy(gameObject, duration);
    }
}