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

    private void Awake()
    {
        rotationX = transform.eulerAngles.x;
        rotationY = transform.eulerAngles.y;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        //Quaternion newRotation = UpdateRotation();

        transform.rotation = UpdateRotation();
        transform.position = playerTransform.position + firstPersonOffset;

    }

    public void SetRotation(float rotX, float rotY)
    {
        rotationX = rotX;
        rotationY = rotY;

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0.0f);
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

    [ContextMenu("Move to player position")]
    private void MoveToPlayerPosition()
    {
        if (playerTransform != null)
        {
            transform.position = playerTransform.position + firstPersonOffset;
        }
    }
}