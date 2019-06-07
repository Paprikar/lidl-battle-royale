using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;

using Photon.Pun;


namespace LidlBattleRoyale
{
    public class PlayerManager : MonoBehaviourPunCallbacks
    {
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;


        void Awake()
        {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {
                PlayerManager.LocalPlayerInstance = gameObject;
            }
        }
    }
}
