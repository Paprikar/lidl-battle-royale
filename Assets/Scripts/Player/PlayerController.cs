using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;


namespace LidlBattleRoyale
{
    public class PlayerController : MonoBehaviourPunCallbacks
    {
        public bool FirstPersonView = true;

        [HideInInspector]
        public Text TextWindow;


        [SerializeField] RuntimeAnimatorController m_FirstPersonAnimatorController;
        [SerializeField] RuntimeAnimatorController m_ThirdPersonAnimatorController;

        MovementController m_MovementController;
        Animator m_AnimatorController;
        GameObject m_MenuLayer;


        void Awake()
        {
            m_MovementController = GetComponent<MovementController>();
            m_AnimatorController = GetComponent<Animator>();

            TextWindow = GameObject.Find("Canvas/Debug").GetComponent<Text>();
            m_MenuLayer = GameObject.Find("Canvas/Menu");
        }


        void Start()
        {
            ChangeView(FirstPersonView);

            m_MenuLayer.SetActive(false);
        }


        void Update()
        {
            if (photonView.IsMine)
            {
                TextWindow.text += (m_MovementController.isGrounded ? "true" : "false");

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    m_MenuLayer.SetActive(!m_MenuLayer.activeSelf);
                }
            }
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
}
