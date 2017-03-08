using System;
using System.Collections;
using System.Collections.Generic;
using MPLobbyExample.Networking;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

namespace MPLobbyExample.UI
{
    /// <summary>
    /// Represents a server in the server list
    /// </summary>
    public class ServerEntry : MonoBehaviour
    {
        [SerializeField]
        protected Text _serverInfoText;

        [SerializeField]
        protected Text _ModeText;

        [SerializeField]
        protected Text _slotInfo;

        [SerializeField]
        protected Button _joinButton;

        //The network manager
        protected MPNetworkManager _netManager;

        /// <summary>
        /// Use this for initialization
        /// </summary>
        [UsedImplicitly]
        void Start()
        {

        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        [UsedImplicitly]
        void Update()
        {

        }

        //Sets up the UI
        public void Populate(MatchInfoSnapshot match, Color c)
        {
            string name = match.name;

            string[] split = name.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            _serverInfoText.text = split[1].Replace(" ", string.Empty);
            _ModeText.text = split[0];

            _slotInfo.text = string.Format("{0}/{1}", match.currentSize, match.maxSize);

            NetworkID networkId = match.networkId;

            _joinButton.onClick.RemoveAllListeners();
            _joinButton.onClick.AddListener(() => JoinMatch(networkId));

            _joinButton.interactable = match.currentSize < match.maxSize;

            GetComponent<Image>().color = c;
        }

        //Load the network manager on enable
        protected virtual void OnEnable()
        {
            if (_netManager == null)
            {
                _netManager = MPNetworkManager.singleton.gameObject.GetComponent<MPNetworkManager>();
            }
        }

        //Fired when player clicks join
        private void JoinMatch(NetworkID networkId)
        {
            MainMenuUI menuUi = MainMenuUI.Instance;

            //menuUi.ShowConnectingModal(true);

            _netManager.JoinMatchmakingGame(networkId, (success, matchInfo) =>
            {
                //Failure flow
                if (!success)
                {
                    menuUi.ShowInfoPopup("Failed to join game.", null);
                }
                //Success flow
                else
                {
                    menuUi.HideInfoPopup();
                    menuUi.ShowInfoPopup("Entering lobby...");
                    _netManager.gameModeUpdated += menuUi.ShowLobbyPanelForConnection;
                }
            });
        }
    }
}