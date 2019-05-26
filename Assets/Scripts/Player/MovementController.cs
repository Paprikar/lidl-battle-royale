using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class MovementController : MonoBehaviour
{
    public bool isGrounded { get { return m_isGrounded; } }


    [SerializeField] float m_MoveSpeedMultiplier = 5f;
    [SerializeField] float m_AnimSpeedMultiplier = 1f;
    [SerializeField] bool m_UseRootMotion = true;
    [SerializeField] float m_MouseSensetivity = 15f;
    [SerializeField] float m_JumpSpeed = 4f;
    //[SerializeField] Transform targetCameraTransform;
    [SerializeField] LayerMask m_GroundLayerMask = -1;
    [SerializeField] float m_GroundCheckHeight = 0.22f;
    [SerializeField] float m_GroundCheckRaiseHeight = 0.28f;
    [SerializeField] float m_GroundCheckRadius = 0.295f;
    [Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 1f;
    [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others

    Animator m_Animator;
    Rigidbody m_Rigidbody;
    CapsuleCollider m_Capsule;
    Transform m_CameraTransform;
    PlayerController m_PlayerController;
    float m_OrigGroundCheckHeight;
    const float k_Half = 0.5f;
    float m_TurnAmount;
    float m_CurrentXRot = 0f;
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    bool m_isGrounded = true;
    bool m_Crouching;


    void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_PlayerController = GetComponent<PlayerController>();
        m_CameraTransform = transform.Find("CameraMount");
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

        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        //move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        //move = Vector3.ProjectOnPlane(move, m_GroundNormal); // Const max horizontal speed
        m_TurnAmount = 0f;

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

        // send input and other state parameters to the animator
        UpdateAnimator();
    }


    void ScaleCapsuleForCrouching(bool crouch)
    {
        if (m_isGrounded && crouch)
        {
            if (m_Crouching) return;
            m_Capsule.height = m_Capsule.height / 2f;
            m_Capsule.center = m_Capsule.center / 2f;
            m_Crouching = true;
        }
        else
        {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
                return;
            }
            m_Capsule.height = m_CapsuleHeight;
            m_Capsule.center = m_CapsuleCenter;
            m_Crouching = false;
        }
    }


    void PreventStandingInLowHeadroom()
    {
        // prevent standing up in crouch-only zones
        if (!m_Crouching)
        {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
            }
        }
    }


    void UpdateAnimator()
    {
        // update the animator parameters
        m_Animator.SetFloat("Move_X", m_PlayerController.InputMovement.x, 0.05f, Time.deltaTime);
        m_Animator.SetFloat("Move_Z", m_PlayerController.InputMovement.z, 0.05f, Time.deltaTime);
        m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
        m_Animator.SetBool("Crouch", m_Crouching);
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
        float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_PlayerController.InputMovement.z;
        if (m_isGrounded)
        {
            m_Animator.SetFloat("JumpLeg", jumpLeg);
        }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (m_isGrounded && m_PlayerController.InputMovement.magnitude > 0)
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
        if (m_PlayerController.Jump && !m_PlayerController.Crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
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
            transform.localEulerAngles.y + m_PlayerController.InputRotation.y * m_MouseSensetivity * Time.fixedDeltaTime,
            0f
        );

        transform.localEulerAngles = newYRot;


        // X Rotation
        m_CurrentXRot = Mathf.Clamp(m_CurrentXRot - m_PlayerController.InputRotation.x * m_MouseSensetivity * Time.fixedDeltaTime, -70f, 60f);
        Vector3 newXRot = new Vector3
        (
            m_CurrentXRot,
            m_CameraTransform.localEulerAngles.y,
            m_CameraTransform.localEulerAngles.z
        );

        m_CameraTransform.localEulerAngles = newXRot;
    }


    void OnAnimatorMove()
    {
        if (m_isGrounded && Time.deltaTime > 0)
        {
            Vector3 v;
            if (m_UseRootMotion)
            {
                v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
            }
            else
            {
                v = transform.TransformDirection(m_PlayerController.InputMovement) * m_MoveSpeedMultiplier;
            }

            v.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = v;
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
