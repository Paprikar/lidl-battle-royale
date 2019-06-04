using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class MovementController : MonoBehaviour
{
    public bool isGrounded { get { return m_isGrounded; } }
    public Vector2 rotationSpeed { get { return m_RotationSpeed; } }

    [HideInInspector] public bool jumpKey { get { return m_JumpKey; } }
    [HideInInspector] public bool crouchKey { get { return m_CrouchKey; } }


    [SerializeField] float m_MoveSpeedMultiplier = 5f;
    [SerializeField] float m_MouseSensetivity = 15f;
    [SerializeField] float m_JumpSpeed = 4f;
    [SerializeField] bool m_UseMoveSnap = false;
    [SerializeField] float m_MoveAcceleration = 5f;
    [SerializeField] float m_MoveBrakingAcceleration = 8f;
    [SerializeField] float m_MoveIncreasedBrakingAcceleration = 10f;
    [SerializeField] float m_MoveExtrapolationFactor = 1.5f;
    [SerializeField] float m_AnimSpeedMultiplier = 1f;
    [SerializeField] float m_RunCycleLegOffset = 0.2f;
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
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    bool m_JumpKey;
    bool m_CrouchKey = false;
    bool m_isGrounded = true;
    bool m_isCrouching = false;
    Vector3 m_MovementVelocity;
    Vector3 m_GroundVelocity = Vector3.zero;
    Vector2 m_Rotation = Vector2.zero;
    Vector2 m_RotationSpeed = Vector2.zero;


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

        ApplyMoveAndRotation();

        if (m_isGrounded)
        {
            HandleGroundedMovement();
        }
        else
        {
            HandleAirborneMovement();
        }

        //ScaleCapsuleForCrouching();

        UpdateAnimator();
    }

    void GetInputs()
    {
        // Get movement inputs
        Vector3 inputMovement = Vector3.zero;

        if (m_isGrounded)
        {
            m_MovementVelocity = transform.InverseTransformDirection(m_Rigidbody.velocity - m_GroundVelocity); // Relative to ground velocity
            m_MovementVelocity.y = 0f;
            m_MovementVelocity = m_MovementVelocity / m_MoveSpeedMultiplier; // Normalization relative to maximum independent speed (running)

            inputMovement.x = InputManager(
                m_MovementVelocity.x,
                Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D),
                m_UseMoveSnap, m_MoveAcceleration, m_MoveBrakingAcceleration, m_MoveIncreasedBrakingAcceleration, m_MoveExtrapolationFactor);

            inputMovement.z = InputManager(
                m_MovementVelocity.z,
                Input.GetKey(KeyCode.S), Input.GetKey(KeyCode.W),
                m_UseMoveSnap, m_MoveAcceleration, m_MoveBrakingAcceleration, m_MoveIncreasedBrakingAcceleration, m_MoveExtrapolationFactor);

            Vector3 nextMovementVelocity = m_MovementVelocity + inputMovement;

            if (nextMovementVelocity.x < 1f + 1e-5 && nextMovementVelocity.z < 1f + 1e-5 && nextMovementVelocity.sqrMagnitude > 1f + 1e-5)
            {
                inputMovement = (nextMovementVelocity.normalized - m_MovementVelocity);
            }

            m_MovementVelocity = m_MovementVelocity + inputMovement; // New velocity (Units per second)
        }


        // Get rotation inputs
        Vector2 inputRotation = new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));
        Vector2 newRotation;

        // Y Rotation
        newRotation.y = transform.localEulerAngles.y + inputRotation.y * m_MouseSensetivity * Time.fixedDeltaTime;

        // X Rotation
        newRotation.x = Mathf.Clamp(m_Rotation.x - inputRotation.x * m_MouseSensetivity * Time.fixedDeltaTime, -70f, 60f);

        // Rotation speed (RPS)
        if (Time.fixedDeltaTime == 0)
        {
            m_RotationSpeed = Vector2.zero;
        }
        else
        {
            m_RotationSpeed = (newRotation - m_Rotation) / (Time.fixedDeltaTime * 360f);
            m_RotationSpeed.x = -m_RotationSpeed.x;
        }
        m_Rotation = newRotation;


        // Jump
        m_JumpKey = Input.GetKeyDown("space");


        // DEBUG
        m_PlayerController.textWindow.text = m_MovementVelocity.magnitude.ToString() + "\n"; // DEBUG

        if (Input.GetKeyDown(KeyCode.E)) // DEBUG
        {
            m_Rigidbody.velocity = m_CameraMountTransform.forward * 30f;
        }
    }


    float InputManager(
        float currentAxisValue,
        bool negativeKey, bool positiveKey,
        bool snap, float acceleration, float braking, float increasedBraking, float extrapolationFactor)
    {
        if (Input.GetKeyDown(KeyCode.Z) && !negativeKey && positiveKey) // DEBUG
        {

        }

        float deltaAxisValue = 0f;

        if (positiveKey == negativeKey) // Neutral input
        {
            if (currentAxisValue < -1e-5f)
            {
                if (currentAxisValue < -1f - 1e-5f)
                {
                    deltaAxisValue = Mathf.Min(currentAxisValue + braking * Time.fixedDeltaTime * extrapolationFactor, -1f);
                    deltaAxisValue = deltaAxisValue - currentAxisValue;
                }
                else
                {
                    deltaAxisValue = Mathf.Min(currentAxisValue + acceleration * Time.fixedDeltaTime * extrapolationFactor, 0f);
                    deltaAxisValue = deltaAxisValue - currentAxisValue;
                }
            }
            else if (currentAxisValue > 1e-5f)
            {
                if (currentAxisValue > 1f + 1e-5f)
                {
                    deltaAxisValue = Mathf.Max(currentAxisValue - braking * Time.fixedDeltaTime * extrapolationFactor, 1f);
                    deltaAxisValue = deltaAxisValue - currentAxisValue;
                }
                else
                {
                    deltaAxisValue = Mathf.Max(currentAxisValue - acceleration * Time.fixedDeltaTime * extrapolationFactor, 0f);
                    deltaAxisValue = deltaAxisValue - currentAxisValue;
                }
            }
        }
        else if (positiveKey)
        {
            if (currentAxisValue < -1e-5f)
            {
                if (currentAxisValue < -1f - 1e-5f)
                {
                    deltaAxisValue = Mathf.Min(currentAxisValue + increasedBraking * Time.fixedDeltaTime * extrapolationFactor, -1f);
                    deltaAxisValue = deltaAxisValue - currentAxisValue;
                }
                else
                {
                    if (snap)
                    {
                        deltaAxisValue = -currentAxisValue;
                    }
                    else
                    {
                        deltaAxisValue = acceleration * Time.fixedDeltaTime * extrapolationFactor;
                    }
                }
            }
            else
            {
                if (currentAxisValue > 1f + 1e-5f)
                {
                    deltaAxisValue = Mathf.Max(currentAxisValue - braking * Time.fixedDeltaTime * extrapolationFactor, 1f);
                    deltaAxisValue = deltaAxisValue - currentAxisValue;
                }
                else
                {
                    deltaAxisValue = Mathf.Min(currentAxisValue + acceleration * Time.fixedDeltaTime * extrapolationFactor, 1f);
                    deltaAxisValue = deltaAxisValue - currentAxisValue;
                }
            }
        }
        else if (negativeKey)
        {
            if (currentAxisValue > 1e-5f)
            {
                if (currentAxisValue > 1f + 1e-5f)
                {
                    deltaAxisValue = Mathf.Max(currentAxisValue - increasedBraking * Time.fixedDeltaTime * extrapolationFactor, 1f);
                    deltaAxisValue = deltaAxisValue - currentAxisValue;
                }
                else
                {
                    if (snap)
                    {
                        deltaAxisValue = -currentAxisValue;
                    }
                    else
                    {
                        deltaAxisValue = -acceleration * Time.fixedDeltaTime * extrapolationFactor;
                    }
                }
            }
            else
            {
                if (currentAxisValue < -1f - 1e-5f)
                {
                    deltaAxisValue = Mathf.Min(currentAxisValue + braking * Time.fixedDeltaTime * extrapolationFactor, -1f);
                    deltaAxisValue = deltaAxisValue - currentAxisValue;
                }
                else
                {
                    deltaAxisValue = Mathf.Max(currentAxisValue - acceleration * Time.fixedDeltaTime * extrapolationFactor, -1f);
                    deltaAxisValue = deltaAxisValue - currentAxisValue;
                }
            }
        }

        return deltaAxisValue;
    }


    void ApplyMoveAndRotation()
    {
        // Movement
        if (m_isGrounded)
        {
            Vector3 velocity = transform.TransformDirection(m_MovementVelocity) * m_MoveSpeedMultiplier;
            velocity.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = velocity + m_GroundVelocity;
        }


        // Rotation
        // Y Axis
        Vector3 newYRot = new Vector3
        (
            0f,
            m_Rotation.y,
            0f
        );

        transform.localEulerAngles = newYRot;

        // X Axis
        Vector3 newXRot = new Vector3
        (
            m_Rotation.x,
            m_CameraMountTransform.localEulerAngles.y,
            m_CameraMountTransform.localEulerAngles.z
        );

        m_CameraMountTransform.localEulerAngles = newXRot;
    }


    void ScaleCapsuleForCrouching()
    {
        if (m_isGrounded && m_JumpKey)
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


    void UpdateAnimator()
    {
        // update the animator parameters
        m_Animator.SetFloat("Move_X", m_MovementVelocity.x, 0.05f, Time.fixedDeltaTime);
        m_Animator.SetFloat("Move_Z", m_MovementVelocity.z, 0.05f, Time.fixedDeltaTime);
        m_Animator.SetFloat("Turn", m_RotationSpeed.y, 0.1f, Time.fixedDeltaTime);
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
        float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_MovementVelocity.z;
        if (m_isGrounded)
        {
            m_Animator.SetFloat("JumpLeg", jumpLeg);
        }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (m_isGrounded && m_MovementVelocity.magnitude > 0)
        {
            m_Animator.speed = m_AnimSpeedMultiplier;
        }
        else
        {
            // don't use that while airborne
            m_Animator.speed = 1;
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
            m_GroundCheckHeight = m_GroundCheckRaiseHeight;
        }
    }


    void CheckGroundStatus()
    {
        Collider[] collList = Physics.OverlapSphere
        (
            transform.position + Vector3.up * m_GroundCheckHeight,
            m_GroundCheckRadius,
            m_GroundLayerMask,
            QueryTriggerInteraction.Ignore
        );

        if (collList.Length > 0)
        {
            // Grounded status
            m_isGrounded = true;


            // AVG ground velocity
            m_GroundVelocity = Vector3.zero; // VelocityTracker -> Rigidbody -> 0

            for (int i = 0; i < collList.Length; i++)
            {
                VelocityTracker vt = collList[i].GetComponent<VelocityTracker>();
                if (vt != null)
                {
                    m_GroundVelocity += vt.velocity;
                }
                else
                {
                    Rigidbody rb = collList[i].GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        m_GroundVelocity += rb.velocity;
                    }
                }
            }

            m_GroundVelocity /= collList.Length;
        }
        else
        {
            // Grounded status
            m_isGrounded = false;
        }
    }
}
