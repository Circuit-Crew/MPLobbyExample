using System.Collections;
using System.Collections.Generic;
using MPLobbyExample.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace MPLobbyExample.UI
{
    public class LobbyPlayer : MonoBehaviour
    {

        [SerializeField]
        protected Button _colorButton;
        [SerializeField]
        protected Image _colorTag;
        [SerializeField]
        protected InputField _nameInput;
        [SerializeField]
        protected Button _readyButton;
        [SerializeField]
        protected Transform _waitingLabel;
        [SerializeField]
        protected Transform _readyLabel;
        [SerializeField]
        protected Image _colorButtonImage;

        /// <summary>
        /// Associated NetworkPlayer object
        /// </summary>
        private MPNetworkPlayer _mpNetworkPlayer;

        private MPNetworkManager _netManager;

        public void Init(MPNetworkPlayer player)
        {
            Debug.LogFormat("Initializing lobby player - Ready {0}", player.Ready);
            this._mpNetworkPlayer = player;
            if (player != null)
            {
                player.SyncVarsChanged += OnNetworkPlayerSyncvarChanged;
            }

            _netManager = MPNetworkManager.singleton.gameObject.GetComponent<MPNetworkManager>(); //TanksNetworkManager.s_Instance;
            if (_netManager != null)
            {
                _netManager.playerJoined += PlayerJoined;
                _netManager.playerLeft += PlayerLeft;
            }

            _readyLabel.gameObject.SetActive(false);
            _waitingLabel.gameObject.SetActive(false);
            _readyButton.gameObject.SetActive(true);
            _readyButton.interactable = _netManager.hasSufficientPlayers;

            if (_netManager.gameType == NetworkGameType.SinglePlayer)
            {
                return;
            }

            MainMenuUI mainMenu = MainMenuUI.Instance;

            mainMenu.playerList.AddPlayer(this);
            mainMenu.playerList.DisplayDirectServerWarning(player.isServer && _netManager.matchMaker == null);

            if (player.hasAuthority)
            {
                SetupLocalPlayer();
            }
            else
            {
                SetupRemotePlayer();
            }

            UpdateValues();
        }

        public void RefreshJoinButton()
        {
            if (_mpNetworkPlayer.Ready)
            {
                // Toggle ready label
                _waitingLabel.gameObject.SetActive(false);
                _readyButton.gameObject.SetActive(false);
                _readyLabel.gameObject.SetActive(true);

                // Make everything else non-interactive
                _colorButton.interactable = false;
                _colorButtonImage.enabled = false;
                _nameInput.interactable = false;
                _nameInput.image.enabled = false;
            }
            else
            {
                // Toggle ready button
                if (_mpNetworkPlayer.hasAuthority)
                {
                    _readyButton.gameObject.SetActive(true);
                    _readyButton.interactable = _netManager.hasSufficientPlayers;
                }
                else
                {
                    _waitingLabel.gameObject.SetActive(true);
                }
                _readyLabel.gameObject.SetActive(false);

                _colorButton.interactable = _mpNetworkPlayer.hasAuthority;
                _colorButtonImage.enabled = _mpNetworkPlayer.hasAuthority;
                _nameInput.interactable = _mpNetworkPlayer.hasAuthority;
                _nameInput.image.enabled = _mpNetworkPlayer.hasAuthority;
            }
        }

        protected virtual void PlayerJoined(MPNetworkPlayer player)
        {
            RefreshJoinButton();
        }

        protected virtual void PlayerLeft(MPNetworkPlayer player)
        {
            RefreshJoinButton();
        }

        protected virtual void OnDestroy()
        {
            if (_mpNetworkPlayer != null)
            {
                _mpNetworkPlayer.SyncVarsChanged -= OnNetworkPlayerSyncvarChanged;
            }

            if (_netManager != null)
            {
                _netManager.playerJoined -= PlayerJoined;
                _netManager.playerLeft -= PlayerLeft;
            }
        }

        private void ChangeReadyButtonColor(Color c)
        {
            _readyButton.image.color = c;
        }

        private void UpdateValues()
        {
            _nameInput.text = _mpNetworkPlayer.PlayerName;
            _colorTag.color = _mpNetworkPlayer.Color;
            _colorButton.GetComponent<Image>().color = _mpNetworkPlayer.Color;

            RefreshJoinButton();
        }

        private void SetupRemotePlayer()
        {
            DeactivateInteractables();

            _readyButton.gameObject.SetActive(false);
            _waitingLabel.gameObject.SetActive(true);
        }

        private void SetupLocalPlayer()
        {
            _nameInput.interactable = true;
            _nameInput.image.enabled = true;

            //We only want in-lobby tank selection button to be clickable if the local player has more than one tank unlocked.
            //bool multipleTanksUnlocked = (TankLibrary.s_Instance.GetNumberOfUnlockedTanks() > 1);
            //m_TankSelectButton.interactable = multipleTanksUnlocked;

            //m_TankSelectButton.image.enabled = true;
            _colorButton.interactable = true;

            _readyButton.gameObject.SetActive(true);

            //we switch from simple name display to name input
            _nameInput.onEndEdit.RemoveAllListeners();
            _nameInput.onEndEdit.AddListener(OnNameChanged);

            _colorButton.onClick.RemoveAllListeners();
            _colorButton.onClick.AddListener(OnColorClicked);

            _readyButton.onClick.RemoveAllListeners();
            _readyButton.onClick.AddListener(OnReadyClicked);
        }

        private void OnNetworkPlayerSyncvarChanged(MPNetworkPlayer player)
        {
            // Update everything
            UpdateValues();
        }

        //===== UI Handler

        //Note that those handler use Command function, as we need to change the value on the server not locally
        //so that all client get the new value throught syncvar
        public void OnColorClicked()
        {
            _mpNetworkPlayer.CmdColorChange();
        }

        public void OnReadyClicked()
        {
            _mpNetworkPlayer.CmdSetReady();
            DeactivateInteractables();
        }

        public void OnNameChanged(string str)
        {
            _mpNetworkPlayer.CmdNameChanged(str);
        }

        private void DeactivateInteractables()
        {
            _nameInput.interactable = false;
            _nameInput.image.enabled = false;
            _colorButtonImage.enabled = false;
        }
    }
}