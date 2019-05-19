using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float movementSpeed = 5.0f;
    public float mouseSensetivity = 20.0f;
    public float jumpForce = 200.0f;
    public Transform headTransform;


    private Rigidbody rb;
    private float hMov;
    private float vMov;
    private float hRot;
    private float vRot;
    private float currentXRot;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Start()
    {
        rb.freezeRotation = true;
        currentXRot = headTransform.localEulerAngles.x;
    }


    void Update()
    {
        hMov = Input.GetAxisRaw("Horizontal");
        vMov = Input.GetAxisRaw("Vertical");

        hRot = Input.GetAxisRaw("Mouse X");
        vRot = Input.GetAxisRaw("Mouse Y");
    }


    void FixedUpdate()
    {
        Move();
    }


    void Move()
    {
        // Movement
        Vector3 movement = new Vector3(hMov, 0f, vMov).normalized * movementSpeed * Time.fixedDeltaTime;

        rb.MovePosition(transform.position + transform.TransformDirection(movement));


        // Y Rotation
        Vector3 newYRot = new Vector3
        (
            0f,
            transform.localEulerAngles.y + hRot * mouseSensetivity * Time.fixedDeltaTime,
            0f
        );

        transform.localEulerAngles = newYRot;


        // X Rotation
        currentXRot = Mathf.Clamp(currentXRot - vRot * mouseSensetivity * Time.fixedDeltaTime, -70f, 60f);
        Vector3 newXRot = new Vector3
        (
            currentXRot,
            headTransform.localEulerAngles.y,
            headTransform.localEulerAngles.z
        );

        headTransform.localEulerAngles = newXRot;


/*         // Jumps
        if (Input.GetKeyDown("space"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        } */
    }
}
