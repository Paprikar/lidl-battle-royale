using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

namespace LidlBattleRoyale
{
    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputField : MonoBehaviour
    {
        const string m_PlayerNamePrefKey = "PlayerName";


        public void SetPlayerName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Player Name is null or empty");
                return;
            }

            PhotonNetwork.NickName = value;
            PlayerPrefs.SetString(m_PlayerNamePrefKey, value);
        }


        void Start()
        {
            string defaultName = string.Empty;
            InputField _inputField = GetComponent<InputField>();

            if (PlayerPrefs.HasKey(m_PlayerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(m_PlayerNamePrefKey);
                _inputField.text = defaultName;
            }

            PhotonNetwork.NickName = defaultName;
        }
    }
}
