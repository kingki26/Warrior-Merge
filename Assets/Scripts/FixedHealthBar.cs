using UnityEngine;

public class HealthBarFollow : MonoBehaviour
{
    private Quaternion fixedRotation;

    private void Start()
    {
        fixedRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.rotation = fixedRotation;
    }
}