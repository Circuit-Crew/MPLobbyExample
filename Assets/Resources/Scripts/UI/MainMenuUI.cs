using System.Collections;
using System;
using System.Collections.Generic;
using MPLobbyExample.Networking;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace MPLobbyExample.UI
{
    //Page in menu that you wish to return to
    public enum MenuPage
    {
        Home,
        Lobby,
        SinglePlayer
    }

    /// <summary>
    /// Class that handles all of the main menu UI and the transitions.
    /// </summary>
    public class MainMenuUI : Singleton<MainMenuUI>
    {
        #region Static config

        public static MenuPage ReturnPage;

        #endregion

        #region Fields

        [SerializeField] protected CanvasGroup _defaultPanel;
        [SerializeField] protected CanvasGroup _createGamePanel;
        [SerializeField] protected CanvasGroup _lobbyPanel;
        [SerializeField] protected CanvasGroup _singlePlayerPanel;
        [SerializeField] protected CanvasGroup _serverListPanel;
        [SerializeField] protected LobbyInfoPanel _infoPanel;
        [SerializeField] protected LobbyPlayerList _playerList;

        [SerializeField] protected GameObject _quitButton;

        private CanvasGroup _currentPanel;

        private Action _waitTask;
        private bool _readyToFireTask;

        private MPNetworkManager _networkManager;

        #endregion

        public LobbyPlayerList playerList
        {
            get { return _playerList; }
        }

        #region Methods

        /// <summary>
        /// Use this for initialization
        /// </summary>
        [UsedImplicitly]
        void Start()
        {
            _networkManager = MPNetworkManager.singleton.gameObject.GetComponent<MPNetworkManager>();

            if (_quitButton != null)
            {
                _quitButton.SetActive(true);
            }
            else
            {
                Debug.LogError("Missing quitButton from MainMenuUI");
            }

            //Used to return to correct page on return to menu
            switch (ReturnPage)
            {
                case MenuPage.Home:
                default:
                    ShowDefaultPanel();
                    break;
                case MenuPage.Lobby:
                    ShowLobbyPanel();
                    break;
                case MenuPage.SinglePlayer:
                    ShowSingleplayerPanel();
                    break;
            }
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        [UsedImplicitly]
        void Update()
        {
            if (_readyToFireTask)
            {
                if (_waitTask != null)
                {
                    _waitTask();
                    _waitTask = null;
                }

                _readyToFireTask = false;
            }
        }

        //Convenience function for showing panels
        public void ShowPanel(CanvasGroup newPanel)
        {
            if (_currentPanel != null)
            {
                _currentPanel.gameObject.SetActive(false);
            }

            _currentPanel = newPanel;
            if (_currentPanel != null)
            {
                _currentPanel.gameObject.SetActive(true);
            }
        }

        public void ShowDefaultPanel()
        {
            ShowPanel(_defaultPanel);
        }

        public void ShowLobbyPanel()
        {
            ShowPanel(_lobbyPanel);
        }

        public void ShowLobbyPanelForConnection()
        {
            ShowPanel(_lobbyPanel);
            
            
            _networkManager.gameModeUpdated -= ShowLobbyPanelForConnection;
            HideInfoPopup();
        }

        public void ShowServerListPanel()
        {
            ShowPanel(_serverListPanel);
        }

        /// <summary>
        /// Shows the info popup with a callback
        /// </summary>
        /// <param name="label">Label.</param>
        /// <param name="callback">Callback.</param>
        public void ShowInfoPopup(string label, UnityAction callback)
        {
            if (_infoPanel != null)
            {
                _infoPanel.Display(label, callback, true);
            }
        }

        public void ShowInfoPopup(string label)
        {
            if (_infoPanel != null)
            {
                _infoPanel.Display(label, null, false);
            }
        }

        public void HideInfoPopup()
        {
            if (_infoPanel != null)
            {
                _infoPanel.gameObject.SetActive(false);
            }
        }

        private void GoToSingleplayerPanel()
        {
            ShowSingleplayerPanel();
            _networkManager.StartSinglePlayerMode(null);
        }

        private void ShowSingleplayerPanel()
        {
            ShowPanel(_singlePlayerPanel);
        }

        private void GoToFindGamePanel()
        {
            ShowServerListPanel();
            _networkManager.StartMatchingmakingClient();
        }

        private void GoToCreateGamePanel()
        {
            ShowPanel(_createGamePanel);
        }

        #endregion

        /// <summary>
        /// Join game button. Joins the first game available.
        /// </summary>
        [UsedImplicitly]
        public void OnJoinGameClicked()
        {
            Debug.Log("Starting Matchmaking Game");
            DoIfNetworkReady(GoToFindGamePanel);
        }

        /// <summary>
        /// Wait for network to disconnect before performing an action
        /// </summary>
        public void DoIfNetworkReady(Action task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            if (_networkManager.isNetworkActive)
            {
                _waitTask = task;
                _networkManager.clientStopped += OnClientStopped;
            }
            else
            {
                task();
            }
        }

        //Event listener
        private void OnClientStopped()
        {
            _networkManager.clientStopped -= OnClientStopped;
            _readyToFireTask = true;
        }

        #region Button Events
        
        /// <summary>
        /// Create game button. Launches game server.
        /// </summary>
        [UsedImplicitly]
        public void OnCreateGameClicked()
        {
            DoIfNetworkReady(GoToCreateGamePanel);
        }

        [UsedImplicitly]
        public void OnSinglePlayerClicked()
        {
            // Set network into SP mode
            DoIfNetworkReady(GoToSingleplayerPanel);
        }

        [UsedImplicitly]
        public void OnFindGameClicked()
        {
            // Set network into matchmaking search mode
            DoIfNetworkReady(GoToFindGamePanel);
        }

        [UsedImplicitly]
        public void OnQuitGameClicked()
        {
            Application.Quit();
        }

        #endregion
    }
}