using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;


namespace LidlBattleRoyale
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager GameManagerInstance;


        [Tooltip("The prefab to use for representing the player")]
        [SerializeField] GameObject m_PlayerPrefab;


        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }


        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }


        void LoadArena()
        {
            PhotonNetwork.LoadLevel("SampleScene");
        }


        void Awake()
        {
            GameManagerInstance = this;
        }


        void Start()
        {
            if (PlayerManager.LocalPlayerInstance == null)
            {
                PhotonNetwork.Instantiate(m_PlayerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
            }
        }


        public override void OnPlayerEnteredRoom(Player other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LoadArena();
            }
        }


        public override void OnPlayerLeftRoom(Player other)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LoadArena();
            }
        }
    }
}
