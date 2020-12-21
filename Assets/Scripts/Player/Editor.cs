using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Editor : MonoBehaviour
{
    [Header("First Person Camera")]
    [Tooltip("The first person camera transform")]
    [SerializeField] private Transform FPCameraTransform;
    [Tooltip("The look sensitivity or how fast the character can look")]
    [SerializeField] private float LookSensitivity;

    [Header("Movement")]
    [Tooltip("The max speed the player can move")]
    [SerializeField] private float MovementSpeed;

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    private Rigidbody rigidbody;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

    private float xRotation;
    private float walkInputValue;
    private float strafeInputValue;

    // Start is called before the first frame update
    void Start()
    {
        // Setup cursor for FPS
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        rigidbody = GetComponent<Rigidbody>();

        xRotation = FPCameraTransform.localEulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        HandleCameraLook();
        CapturePlayerMove();
    }

    private void FixedUpdate()
    {
        HandlePlayerMove();
    }

    void HandleCameraLook()
    {
        // Horizontal rotation, done on player
        {
            transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X") * LookSensitivity * Time.deltaTime, 0f), Space.Self);
        }

        // Vertical look, done on camera comp tranform
        {
            xRotation += Input.GetAxis("Mouse Y") * LookSensitivity * Time.deltaTime;

            xRotation = Mathf.Clamp(xRotation, -89f, 89f);

            FPCameraTransform.localEulerAngles = new Vector3(xRotation, 0f, 0f);
        }
    }

    void CapturePlayerMove()
    {
        walkInputValue = Input.GetAxis("Walk");
        strafeInputValue = Input.GetAxis("Strafe");
    }

    void HandlePlayerMove()
    {
        if (walkInputValue != 0 || strafeInputValue != 0)
        {
            Vector3 walkDirection = transform.forward * walkInputValue;
            Vector3 strafeDirection = transform.right * strafeInputValue;

            Vector3 direction = walkDirection + strafeDirection;
            direction.Normalize();
            rigidbody.AddForce(direction * MovementSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}
