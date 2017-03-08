using System;
using JetBrains.Annotations;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using MPLobbyExample.UI;

namespace MPLobbyExample.Networking
{
    public class MPNetworkPlayer : NetworkBehaviour
    {
        public event Action<MPNetworkPlayer> SyncVarsChanged;
        // Server only event
        public event Action<MPNetworkPlayer> BecameReady;

        public event Action GameDetailsReady;


        [SerializeField]
        protected GameObject _avatarPrefab;
        [SerializeField]
        protected GameObject _lobbyPrefab;

        // Set by commands
        [SyncVar(hook = "OnMyName")]
        private string _playerName = "";
        [SyncVar(hook = "OnMyColor")]
        private Color _playerColor = Color.clear;
        [SyncVar(hook = "OnReadyChanged")]
        private bool _ready = false;

        // Set on the server only
        [SyncVar(hook = "OnHasInitialized")]
        private bool _initialized = false;

        private IColorProvider _colorProvider = null;
        private MPNetworkManager _netManager;

        /// <summary>
		/// Gets this player's name
		/// </summary>
		public string PlayerName
        {
            get { return _playerName; }
        }

        /// <summary>
        /// Gets this player's colour
        /// </summary>
        public Color Color
        {
            get { return _playerColor; }
        }

        /// <summary>
		/// Gets whether this player has marked themselves as ready in the lobby
		/// </summary>
		public bool Ready
        {
            get { return _ready; }
        }

        /// <summary>
		/// Gets the tank manager associated with this player
		/// </summary>
		public Avatar Avatar
        {
            get;
            set;
        }

        /// <summary>
		/// Gets the lobby object associated with this player
		/// </summary>
		public LobbyPlayer LobbyObject
        {
            get;
            private set;
        }

        void Awake()
        {
        }

        void Start()
        {
            //_nodeInfos.Callback = OnNodesChanged;

            if (_netManager == null)
            {
                _netManager = MPNetworkManager.singleton.gameObject.GetComponent<MPNetworkManager>();
            }
        }

        /// <summary>
		/// Set initial values
		/// </summary>
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            //GameManagerScript.Instance.LocalPlayer = this;
            _initialized = true;
        }

        /// <summary>
		/// Register us with the NetworkManager
		/// </summary>
		[Client]
        public override void OnStartClient()
        {
            DontDestroyOnLoad(this);

            if (_netManager == null)
            {
                _netManager = MPNetworkManager.singleton.gameObject.GetComponent<MPNetworkManager>();
            }

            base.OnStartClient();
            Debug.Log("Client Network Player start");

            _netManager.RegisterMPNetworkPlayer(this);
        }

        /// <summary>
		/// Deregister us with the manager
		/// </summary>
		public override void OnNetworkDestroy()
        {
            base.OnNetworkDestroy();
            Debug.Log("Client Network Player OnNetworkDestroy");

            if (LobbyObject != null)
            {
                Destroy(LobbyObject.gameObject);
            }

            if (_netManager != null)
            {
                _netManager.DeregisterMPNetworkPlayer(this);
            }
        }

        /// <summary>
		/// Clean up lobby object for us
		/// </summary>
		protected virtual void OnDestroy()
        {
            if (LobbyObject != null)
            {
                Destroy(LobbyObject.gameObject);
            }
        }

        /// <summary>
		/// Fired when we enter the game scene
		/// </summary>
		[Client]
        public void OnEnterGameScene()
        {
            if (hasAuthority)
            {
                CmdClientReadyInScene();
            }
        }

        /// <summary>
        /// Fired when we return to the lobby scene, or are first created in the lobby
        /// </summary>
        [Client]
        public void OnEnterLobbyScene()
        {
            Debug.Log("OnEnterLobbyScene");
            if (_initialized && LobbyObject == null)
            {
                CreateLobbyObject();
            }
        }

        [Server]
        public void ClearReady()
        {
            _ready = false;
        }

        /// <summary>
        /// Create our lobby object
        /// </summary>
        private void CreateLobbyObject()
        {
            LobbyObject = Instantiate(_lobbyPrefab).GetComponent<LobbyPlayer>();
            LobbyObject.Init(this);
        }

        [Server]
        private void LazyLoadColorProvider()
        {
            if (_colorProvider != null)
            {
                return;
            }

            _colorProvider = new TeamColorProvider();
        }

        [Server]
        private void SelectColour()
        {
            LazyLoadColorProvider();

            if (_colorProvider == null)
            {
                Debug.LogWarning("Could not find color provider");
                return;
            }

            Color newPlayerColor = _colorProvider.ServerGetColor(this);

            _playerColor = newPlayerColor;
        }

        [ClientRpc]
        public void RpcSetGameSettings(int mapIndex, int modeIndex)
        {
            if (GameDetailsReady != null && isLocalPlayer)
            {
                GameDetailsReady();
            }
        }

        [ClientRpc]
        public void RpcPrepareForLoad()
        {
            if (isLocalPlayer)
            {
                //// Show loading screen
                //LoadingModal loading = LoadingModal.s_Instance;

                //if (loading != null)
                //{
                //    loading.FadeIn();
                //}
            }
        }

        #region Commands

        /// <summary>
        /// Create our tank
        /// </summary>
        [Command]
        private void CmdClientReadyInScene()
        {
            Debug.Log("CmdClientReadyInScene");
            GameObject avatarGO = Instantiate(_avatarPrefab);
            NetworkServer.SpawnWithClientAuthority(avatarGO, connectionToClient);
            Avatar = avatarGO.GetComponent<Avatar>();
            //Avatar.SetPlayerId(playerId);
        }

        [Command]
        private void CmdSetInitialValues(int tankType, int decorationIndex, int decorationMaterial, string newName)
        {
            Debug.Log("CmdChangeTank");
            _playerName = newName;
            SelectColour();
            _initialized = true;
        }

        [Command]
        public void CmdChangeAvatar(int tankType)
        {
            Debug.Log("CmdChangeTank");
        }

        [Command]
        public void CmdColorChange()
        {
            Debug.Log("CmdColorChange");
            SelectColour();
        }

        [Command]
        public void CmdNameChanged(string name)
        {
            Debug.Log("CmdNameChanged");
            _playerName = name;
        }

        [Command]
        public void CmdSetReady()
        {
            Debug.Log("CmdSetReady");
            if (_netManager.hasSufficientPlayers)
            {
                _ready = true;

                if (BecameReady != null)
                {
                    BecameReady(this);
                }
            }
        }

        #endregion


        #region Syncvar callbacks

        private void OnMyName(string newName)
        {
            _playerName = newName;

            if (SyncVarsChanged != null)
            {
                SyncVarsChanged(this);
            }
        }

        private void OnMyColor(Color newColor)
        {
            _playerColor = newColor;

            if (SyncVarsChanged != null)
            {
                SyncVarsChanged(this);
            }
        }

        private void OnReadyChanged(bool value)
        {
            _ready = value;

            if (SyncVarsChanged != null)
            {
                SyncVarsChanged(this);
            }
        }

        private void OnHasInitialized(bool value)
        {
            if (!_initialized && value)
            {
                _initialized = value;
                CreateLobbyObject();

                //if (isServer && !m_Settings.isSinglePlayer)
                //{
                //    RpcSetGameSettings(m_Settings.mapIndex, m_Settings.modeIndex);
                //}
            }
        }

        #endregion
    }

}