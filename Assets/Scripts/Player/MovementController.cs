using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementController : MonoBehaviour
{
    public float movementSpeed = 5.0f;
    public float mouseSensetivity = 20.0f;
    public float jumpForce = 200.0f;
    public Transform headTransform;
    public Text textWindow;


    private Rigidbody rb;
    private float hMov;
    private float vMov;
    private float hRot;
    private float vRot;
    private float currentXRot;
    private GroundChecker gc;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        gc = transform.Find("GroundCheckerTrigger").GetComponent<GroundChecker>();
    }


    void Start()
    {
        rb.freezeRotation = true;
        currentXRot = headTransform.localEulerAngles.x;
    }


    void Update()
    {
        hMov = Input.GetAxis("Horizontal");
        vMov = Input.GetAxis("Vertical");

        hRot = Input.GetAxisRaw("Mouse X");
        vRot = Input.GetAxisRaw("Mouse Y");

        textWindow.text = (gc.isGrounded ? "true" : "false") + " " + gc.collList.Count.ToString();
    }


    void FixedUpdate()
    {
        Move();
    }


    void Move()
    {
        // Movement
        Vector3 movement = Vector3.ClampMagnitude(new Vector3(hMov, 0f, vMov), 1.0f) * movementSpeed * Time.fixedDeltaTime;

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


        // Jumps
        if (Input.GetKeyDown("space") && gc.isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }
}
