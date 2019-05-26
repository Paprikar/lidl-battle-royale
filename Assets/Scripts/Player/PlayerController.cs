using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour
{
    [HideInInspector] public Vector3 InputMovement { get { return m_InputMovement; } }
    [HideInInspector] public Vector2 InputRotation { get { return m_InputRotation; } }
    [HideInInspector] public bool Jump { get { return m_Jump; } }
    [HideInInspector] public bool Crouch { get { return false; } }


    [SerializeField] Text textWindow;

    MovementController mc;
    float hMov;
    float vMov;
    Vector3 m_InputMovement;
    Vector2 m_InputRotation;
    bool m_Jump;


    void Awake()
    {
        mc = GetComponent<MovementController>();
    }


    void Update()
    {
        textWindow.text = (mc.isGrounded ? "true" : "false");
    }


    void FixedUpdate()
    {
        Move();
    }


    void Move()
    {
        // Get movement inputs
        m_Jump = Input.GetKeyDown("space");

        if (mc.isGrounded)
        {
            hMov = InputManager(hMov, Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D), 5f, true);
            vMov = InputManager(vMov, Input.GetKey(KeyCode.S), Input.GetKey(KeyCode.W), 5f, true);

            m_InputMovement = Vector3.ClampMagnitude(new Vector3(hMov, 0f, vMov), 1.0f);

        }

        // Get rotation inputs
        m_InputRotation = new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));

        // Animation Controller
        mc.Move();
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
