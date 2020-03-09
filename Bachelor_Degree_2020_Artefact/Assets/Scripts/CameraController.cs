using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform = null;
    [SerializeField] private float sensitivity = 1.0f;
    [SerializeField, Range(0.0f, 89.0f)] private float maxXDegrees = 89.0f;

    private Vector3 firstPersonOffset = new Vector3(0.0f, 0.5f, 0.0f);
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        
    }

    void LateUpdate()
    {
        //Quaternion newRotation = UpdateRotation();

        transform.rotation = UpdateRotation();
        transform.position = playerTransform.position + firstPersonOffset;

    }


    private Quaternion UpdateRotation()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;

        rotationX = Mathf.Clamp(rotationX, -maxXDegrees, maxXDegrees);

        return Quaternion.Euler(rotationX, rotationY, 0.0f);

    }
}
