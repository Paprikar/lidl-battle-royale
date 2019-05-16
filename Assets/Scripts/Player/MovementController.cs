using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float movementSpeed = 3.0f;
    public float jumpForce = 200.0f;
    public float mouseSensetivity = 5.0f;
    public Transform headObject;


    private Rigidbody rb;
    private float hMov;
    private float vMov;
    private float hRot;
    private float vRot;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
        Vector3 movement = new Vector3(hMov, 0f, vMov).normalized * movementSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);

        Vector2 rotating = new Vector2(hRot, vRot) * mouseSensetivity * Time.deltaTime;
        transform.rotation *= Quaternion.AngleAxis(rotating.x, headObject.up);
        headObject.rotation *= Quaternion.AngleAxis(-rotating.y, Vector3.right);

        if (Input.GetKeyDown("space"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Acceleration);
        }
    }
}
