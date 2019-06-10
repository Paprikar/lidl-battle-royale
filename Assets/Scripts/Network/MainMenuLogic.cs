using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

namespace LidlBattleRoyale
{
    public class MainMenuLogic : MonoBehaviourPunCallbacks
    {
        [SerializeField] byte m_MaxPlayersPerRoom = 4;
        [SerializeField] GameObject m_ControlLayer;
        [SerializeField] GameObject m_ProgressLayer;

        string m_GameVersion = "1";
        bool m_isConnecting = false;


        public void Connect()
        {
            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            m_isConnecting = true;

            m_ProgressLayer.SetActive(true);
            m_ControlLayer.SetActive(false);

            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.GameVersion = m_GameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }


        void Awake()
        {
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
        }


        void Start()
        {
            m_ProgressLayer.SetActive(false);
            m_ControlLayer.SetActive(true);
        }


        public override void OnConnectedToMaster()
        {
            if (m_isConnecting)
            {
                PhotonNetwork.JoinRandomRoom();
            }
        }


        public override void OnDisconnected(DisconnectCause cause)
        {
            m_ProgressLayer.SetActive(false);
            m_ControlLayer.SetActive(true);
        }


        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = m_MaxPlayersPerRoom });
        }


        public override void OnJoinedRoom()
        {
            m_isConnecting = false;

            PhotonNetwork.LoadLevel("SampleScene");
        }
    }
}
