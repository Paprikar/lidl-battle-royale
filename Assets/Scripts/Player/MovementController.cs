using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementController : MonoBehaviour
{
    public float movementSpeed = 5.0f;
    public float mouseSensetivity = 20.0f;
    public float jumpForce = 4.0f;
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
        gc = transform.Find("GroundChecker").GetComponent<GroundChecker>();
    }


    void Start()
    {
        rb.freezeRotation = true;
        currentXRot = headTransform.localEulerAngles.x;
    }


    void Update()
    {
        textWindow.text = (gc.isGrounded ? "true" : "false") + " " + gc.collList.Count.ToString();
    }


    void FixedUpdate()
    {
        Move();
    }


    void Move()
    {
        // Movement
        if (gc.isGrounded)
        {
            hMov = InputManager(hMov, Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D), 5, true);
            vMov = InputManager(vMov, Input.GetKey(KeyCode.S), Input.GetKey(KeyCode.W), 5, true);

            Vector3 movement = Vector3.ClampMagnitude(new Vector3(hMov, 0f, vMov), 1.0f) * movementSpeed;

            // Jump
            if (Input.GetKeyDown("space"))
            {
                movement.y = rb.velocity.y + jumpForce;
            }
            else
            {
                movement.y = rb.velocity.y;
            }

            rb.velocity = transform.TransformDirection(movement);
        }


        // Get Inputs
        hRot = Input.GetAxisRaw("Mouse X");
        vRot = Input.GetAxisRaw("Mouse Y");


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
    }


    float InputManager(float currentAxisValue, bool negativeKey, bool positiveKey, float modifier, bool snap = true)
    {
        if (positiveKey == negativeKey)
        {
            if (currentAxisValue < 0f)
            {
                currentAxisValue = Mathf.Min(currentAxisValue + modifier * Time.fixedDeltaTime, 0f);
            }
            else if (currentAxisValue > 0f)
            {
                currentAxisValue = Mathf.Max(currentAxisValue - modifier * Time.fixedDeltaTime, 0f);
            }
        }
        else if (positiveKey)
        {
            if (snap && currentAxisValue < 0f)
            {
                currentAxisValue = 0f;
            }
            else
            {
                currentAxisValue = Mathf.Min(currentAxisValue + modifier * Time.fixedDeltaTime, 1f);
            }
        }
        else if (negativeKey)
        {
            if (snap && currentAxisValue > 0f)
            {
                currentAxisValue = 0f;
            }
            else
            {
                currentAxisValue = Mathf.Max(currentAxisValue - modifier * Time.fixedDeltaTime, -1f);
            }
        }

        return currentAxisValue;
    }
}
