using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 1.0f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private KeyCode editorSprintKey = KeyCode.LeftShift;
    [SerializeField] private float editorSprintMult = 3.0f;
    

    [SerializeField] private Vector3 inputVector = Vector3.zero;
    private CharacterController characterController;
    private Transform mainCameraTransform;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        mainCameraTransform = Camera.main.transform;
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (characterController.isGrounded)
        {
            inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));

            //Vector3 topPoint = transform.position + characterController.center + Vector3.up * (characterController.height / 2 - characterController.radius) * 0.95f;
            //Vector3 bottomPoint = transform.position + characterController.center - Vector3.up * (characterController.height / 2 - characterController.radius) * 0.95f;

            //Physics.CapsuleCast(topPoint, bottomPoint, characterController.radius * 0.95f, Vector3.down, out RaycastHit raycastHit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore);

            //Debug.DrawLine(Vector3.zero, raycastHit.normal, Color.red, 1.0f);
            ////Debug.Log(raycastHit.normal);

            //inputVector = Vector3.ProjectOnPlane(mainCameraTransform.rotation * inputVector, raycastHit.normal).normalized * inputVector.magnitude;
            inputVector = Vector3.ProjectOnPlane(mainCameraTransform.rotation * inputVector, Vector3.up).normalized * inputVector.magnitude;

            inputVector = Vector3.ClampMagnitude(inputVector, 1.0f) * movementSpeed;

            if(Application.isEditor && Input.GetKey(editorSprintKey))
            {
                inputVector *= editorSprintMult;
            }

            

        }
        inputVector += new Vector3(0.0f, gravity, 0.0f) * Time.deltaTime;

        characterController.Move(inputVector * Time.deltaTime);

        Debug.DrawLine(transform.position, transform.position + (inputVector * 2.0f), Color.red);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Debug.Log("wow.");
    }
}
