using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour
{
    public bool FirstPersonView = true;
    public Text textWindow;


    [SerializeField] RuntimeAnimatorController m_FirstPersonAnimatorController;
    [SerializeField] RuntimeAnimatorController m_ThirdPersonAnimatorController;

    MovementController m_MovementController;
    Animator m_AnimatorController;


    void Awake()
    {
        m_MovementController = GetComponent<MovementController>();
        m_AnimatorController = GetComponent<Animator>();
    }


    void Start()
    {
        ChangeView(FirstPersonView);
    }


    void Update()
    {
        textWindow.text += (m_MovementController.isGrounded ? "true" : "false");
    }


    void FixedUpdate()
    {
        m_MovementController.Move();
    }


    public void ChangeView(bool firstPersonView)
    {
        FirstPersonView = firstPersonView;

        transform.Find("FirstPersonView").gameObject.SetActive(FirstPersonView);
        transform.Find("ThirdPersonView").gameObject.SetActive(!FirstPersonView);

        m_AnimatorController.runtimeAnimatorController = FirstPersonView ?
            m_FirstPersonAnimatorController : m_ThirdPersonAnimatorController;
    }
}
