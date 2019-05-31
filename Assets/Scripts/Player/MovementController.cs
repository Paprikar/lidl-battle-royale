using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class MovementController : MonoBehaviour
{
    public bool isGrounded { get { return m_isGrounded; } }

    [HideInInspector] public Vector3 InputMovement { get { return m_InputMovement; } }
    [HideInInspector] public Vector2 InputRotation { get { return m_InputRotation; } }
    [HideInInspector] public bool JumpKey { get { return m_JumpKey; } }
    [HideInInspector] public bool CrouchKey { get { return m_CrouchKey; } }


    [SerializeField] float m_MoveSpeedMultiplier = 5f;
    [SerializeField] float m_AnimSpeedMultiplier = 1f;
    [SerializeField] float m_RunCycleLegOffset = 0.2f;
    [SerializeField] bool m_UseRootMotion = true;
    [SerializeField] float m_MouseSensetivity = 15f;
    [SerializeField] float m_JumpSpeed = 4f;
    [SerializeField] LayerMask m_GroundLayerMask = -1;
    [SerializeField] float m_GroundCheckHeight = 0.22f;
    [SerializeField] float m_GroundCheckRaiseHeight = 0.28f;
    [SerializeField] float m_GroundCheckRadius = 0.295f;
    [SerializeField] [Range(1f, 4f)] float m_GravityMultiplier = 1f;

    Animator m_Animator;
    Rigidbody m_Rigidbody;
    CapsuleCollider m_Capsule;
    PlayerController m_PlayerController;
    Transform m_CameraMountTransform;
    float m_OrigGroundCheckHeight;
    const float k_Half = 0.5f;
    float m_TurnSpeed = 0f;
    float m_CurrentXRot = 0f;
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    bool m_JumpKey;
    bool m_CrouchKey;
    bool m_isGrounded = true;
    bool m_isCrouching = false;
    Vector3 m_InputMovement;
    Vector2 m_InputRotation;
    float horizontalMoveInput;
    float verticalMoveInput;


    void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_PlayerController = GetComponent<PlayerController>();
        m_CameraMountTransform = transform.Find("FirstPersonView/CameraMount");
    }


    void Start()
    {
        m_Rigidbody.freezeRotation = true;
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;
        m_OrigGroundCheckHeight = m_GroundCheckHeight;
    }


    public void Move()
    {
        CheckGroundStatus();

        GetInputs();

        m_TurnSpeed = m_InputRotation.y * m_MouseSensetivity / 360f;
        ApplyExtraTurnRotation();

        // control and velocity handling is different when grounded and airborne:
        if (m_isGrounded)
        {
            HandleGroundedMovement();
        }
        else
        {
            HandleAirborneMovement();
        }

        //ScaleCapsuleForCrouching(crouch);
        //PreventStandingInLowHeadroom();

        UpdateAnimator();
    }

    void GetInputs()
    {
        // Get movement inputs
        m_JumpKey = Input.GetKeyDown("space");

        //float horizontalMoveInput = transform.InverseTransformDirection(m_Rigidbody.velocity).x / m_MoveSpeedMultiplier;
        //float verticalMoveInput = transform.InverseTransformDirection(m_Rigidbody.velocity).z / m_MoveSpeedMultiplier;

        m_PlayerController.textWindow.text = verticalMoveInput.ToString() + "\n";

        if (m_isGrounded)
        {
            horizontalMoveInput = InputManager(horizontalMoveInput, Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D), true, 5f, 10f, 20f);
            verticalMoveInput = InputManager(verticalMoveInput, Input.GetKey(KeyCode.S), Input.GetKey(KeyCode.W), true, 5f, 10f, 20f);

            if (horizontalMoveInput <= 1f && verticalMoveInput <= 1f)
            {
                m_InputMovement = Vector3.ClampMagnitude(new Vector3(horizontalMoveInput, 0f, verticalMoveInput), 1.0f);
            }
        }


        // Get rotation inputs
        m_InputRotation = new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));
    }


    float InputManager(float currentAxisValue, bool negativeKey, bool positiveKey, bool snap, float modifier, float reduction, float increasedReduction)
    {
        if (positiveKey == negativeKey) // Neutral input
        {
            if (currentAxisValue < 0f)
            {
                if (currentAxisValue < -1f)
                {
                    currentAxisValue = Mathf.Min(currentAxisValue + reduction * Time.fixedDeltaTime, -1f);
                }
                else
                {
                    currentAxisValue = Mathf.Min(currentAxisValue + modifier * Time.fixedDeltaTime, 0f);
                }
            }
            else if (currentAxisValue > 0f)
            {
                if (currentAxisValue < -1f)
                {
                    currentAxisValue = Mathf.Max(currentAxisValue - reduction * Time.fixedDeltaTime, 1f);
                }
                else
                {
                    currentAxisValue = Mathf.Max(currentAxisValue - modifier * Time.fixedDeltaTime, 0f);
                }
            }
        }
        else if (positiveKey)
        {
            if (currentAxisValue < 0f)
            {
                if (currentAxisValue < -1f)
                {
                    currentAxisValue = Mathf.Min(currentAxisValue + increasedReduction * Time.fixedDeltaTime, -1f);
                }
                else
                {
                    if (snap)
                    {
                        currentAxisValue = 0f;
                    }
                    else
                    {
                        currentAxisValue = currentAxisValue + modifier * Time.fixedDeltaTime;
                    }
                }
            }
            else
            {
                if (currentAxisValue > 1f)
                {
                    currentAxisValue = Mathf.Max(currentAxisValue - reduction * Time.fixedDeltaTime, 1f);
                }
                else
                {
                    currentAxisValue = Mathf.Min(currentAxisValue + modifier * Time.fixedDeltaTime, 1f);
                }
            }
        }
        else if (negativeKey)
        {
            if (currentAxisValue > 0f)
            {
                if (currentAxisValue > 1f)
                {
                    currentAxisValue = Mathf.Max(currentAxisValue - increasedReduction * Time.fixedDeltaTime, 1f);
                }
                else
                {
                    if (snap)
                    {
                        currentAxisValue = 0f;
                    }
                    else
                    {
                        currentAxisValue = currentAxisValue - modifier * Time.fixedDeltaTime;
                    }
                }
            }
            else
            {
                if (currentAxisValue < -1f)
                {
                    currentAxisValue = Mathf.Min(currentAxisValue + reduction * Time.fixedDeltaTime, -1f);
                }
                else
                {
                    currentAxisValue = Mathf.Max(currentAxisValue - modifier * Time.fixedDeltaTime, -1f);
                }
            }
        }

        return currentAxisValue;
    }


    void ScaleCapsuleForCrouching(bool crouch)
    {
        if (m_isGrounded && crouch)
        {
            if (m_isCrouching) return;
            m_Capsule.height = m_Capsule.height / 2f;
            m_Capsule.center = m_Capsule.center / 2f;
            m_isCrouching = true;
        }
        else
        {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_isCrouching = true;
                return;
            }
            m_Capsule.height = m_CapsuleHeight;
            m_Capsule.center = m_CapsuleCenter;
            m_isCrouching = false;
        }
    }


    void PreventStandingInLowHeadroom()
    {
        // prevent standing up in crouch-only zones
        if (!m_isCrouching)
        {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_isCrouching = true;
            }
        }
    }


    void UpdateAnimator()
    {
        if (m_PlayerController.FirstPersonView)
        {

        }
        else
        {
            // update the animator parameters
            m_Animator.SetFloat("Move_X", m_InputMovement.x, 0.05f, Time.deltaTime);
            m_Animator.SetFloat("Move_Z", m_InputMovement.z, 0.05f, Time.deltaTime);
            m_Animator.SetFloat("Turn", m_TurnSpeed, 0.1f, Time.deltaTime);
            m_Animator.SetBool("Crouch", m_isCrouching);
            m_Animator.SetBool("OnGround", m_isGrounded);
            if (!m_isGrounded)
            {
                m_Animator.SetFloat("Move_Y", m_Rigidbody.velocity.y);
            }

            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle =
                Mathf.Repeat(
                    m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_InputMovement.z;
            if (m_isGrounded)
            {
                m_Animator.SetFloat("JumpLeg", jumpLeg);
            }

            // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
            // which affects the movement speed because of the root motion.
            if (m_isGrounded && m_InputMovement.magnitude > 0)
            {
                m_Animator.speed = m_AnimSpeedMultiplier;
            }
            else
            {
                // don't use that while airborne
                m_Animator.speed = 1;
            }
        }
    }


    void HandleAirborneMovement()
    {
        // Apply extra gravity from multiplier
        Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
        m_Rigidbody.AddForce(extraGravityForce);

        m_GroundCheckHeight = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckHeight : m_GroundCheckRaiseHeight;
    }


    void HandleGroundedMovement()
    {
        // Jump
        //if (m_JumpKey && !m_CrouchKey && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
        if (m_JumpKey && !m_CrouchKey && m_isGrounded)
        {
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_Rigidbody.velocity.y + m_JumpSpeed, m_Rigidbody.velocity.z);
            m_isGrounded = false;
            m_Animator.applyRootMotion = false;
            m_GroundCheckHeight = m_GroundCheckRaiseHeight;
        }
    }


    void ApplyExtraTurnRotation()
    {
        // Y Rotation
        Vector3 newYRot = new Vector3
        (
            0f,
            transform.localEulerAngles.y + m_InputRotation.y * m_MouseSensetivity * Time.fixedDeltaTime,
            0f
        );

        transform.localEulerAngles = newYRot;


        // X Rotation
        m_CurrentXRot = Mathf.Clamp(m_CurrentXRot - m_InputRotation.x * m_MouseSensetivity * Time.fixedDeltaTime, -70f, 60f);
        Vector3 newXRot = new Vector3
        (
            m_CurrentXRot,
            m_CameraMountTransform.localEulerAngles.y,
            m_CameraMountTransform.localEulerAngles.z
        );

        m_CameraMountTransform.localEulerAngles = newXRot;
    }


    void OnAnimatorMove()
    {
        if (m_isGrounded && Time.deltaTime > 0)
        {
            Vector3 v;
            if (m_UseRootMotion)
            {
                v = m_Animator.deltaPosition * m_MoveSpeedMultiplier / Time.deltaTime;
            }
            else
            {
                v = transform.TransformDirection(m_InputMovement) * m_MoveSpeedMultiplier;
            }

            v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
        }
    }


    void CheckGroundStatus()
    {
        if
        (
            Physics.CheckSphere
            (
                transform.position + Vector3.up * m_GroundCheckHeight,
                m_GroundCheckRadius,
                m_GroundLayerMask,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            m_isGrounded = true;
            m_Animator.applyRootMotion = true;
        }
        else
        {
            m_isGrounded = false;
            m_Animator.applyRootMotion = false;
        }
    }
}
