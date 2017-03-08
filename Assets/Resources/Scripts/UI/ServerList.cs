using System.Collections;
using System.Collections.Generic;
using MPLobbyExample.Networking;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

namespace MPLobbyExample.UI
{
    public class ServerList : MonoBehaviour
    {
        //Number of games per page
        [SerializeField]
        private int _pageSize = 6;

        //Editor configurable time
        [SerializeField]
        private float _listAutoRefreshTime = 60f;
        private float _nextRefreshTime;

        //Reference to paging buttons
        [SerializeField]
        protected Button _nextButton, _previousButton;

        [SerializeField]
        protected Text _pageNumber;

        [SerializeField]
        protected RectTransform _serverListRect;
        [SerializeField]
        protected GameObject _serverEntryPrefab;
        [SerializeField]
        protected GameObject _noServerFound;

        //Page tracking
        protected int _currentPage = 0;
        protected int _previousPage = 0;
        protected int _newPage = 0;

        static Color OddServerColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        static Color EvenServerColor = new Color(.94f, .94f, .94f, 1.0f);

        //Cached singletons
        private MPNetworkManager _networkManager;
        private MainMenuUI _menuUI;

        protected virtual void OnEnable()
        {
            if (_networkManager == null)
            {
                _networkManager = MPNetworkManager.singleton.gameObject.GetComponent<MPNetworkManager>();
            }

            if (_menuUI == null)
            {
                _menuUI = MainMenuUI.Instance;
            }

            //Reset pages
            _currentPage = 0;
            _previousPage = 0;

            ClearUi();

            //Disable NO SERVER FOUND error message
            _noServerFound.SetActive(false);

            _nextRefreshTime = Time.time;

            //Subscribe to network events
            if (_networkManager != null)
            {
                _networkManager.clientDisconnected += OnDisconnect;
                _networkManager.clientError += OnError;
                _networkManager.serverError += OnError;
                _networkManager.matchDropped += OnDrop;
            }
        }

        protected void ClearUi()
        {
            foreach (Transform t in _serverListRect)
            {
                Destroy(t.gameObject);
            }
        }

        protected virtual void OnDisable()
        {
            //Unsubscribe from network events
            if (_networkManager != null)
            {
                _networkManager.clientDisconnected -= OnDisconnect;
                _networkManager.clientError -= OnError;
                _networkManager.serverError -= OnError;
                _networkManager.matchDropped -= OnDrop;
            }
        }

        //Network event
        protected virtual void OnError(UnityEngine.Networking.NetworkConnection conn, int errorCode)
        {
            //if (_menuUI != null)
            //{
            //    _menuUI.ShowDefaultPanel();
            //    _menuUI.ShowInfoPopup("A connection error occurred", null);
            //}

            if (_networkManager != null)
            {
                _networkManager.Disconnect();
            }
        }

        //Network event
        protected virtual void OnDisconnect(UnityEngine.Networking.NetworkConnection conn)
        {
            //if (_menuUI != null)
            //{
            //    _menuUI.ShowDefaultPanel();
            //    _menuUI.ShowInfoPopup("Disconnected from server", null);
            //}

            if (_networkManager != null)
            {
                _networkManager.Disconnect();
            }
        }

        //Network event
        protected virtual void OnDrop()
        {
            //if (_menuUI != null)
            //{
            //    _menuUI.ShowDefaultPanel();
            //    _menuUI.ShowInfoPopup("Disconnected from server", null);
            //}

            if (_networkManager != null)
            {
                _networkManager.Disconnect();
            }
        }

        // Use this for initialization
        [UsedImplicitly]
        void Start()
        {

        }

        // Update is called once per frame
        [UsedImplicitly]
        void Update()
        {
            if (_nextRefreshTime <= Time.time)
            {
                RequestPage(_currentPage);

                _nextRefreshTime = Time.time + _listAutoRefreshTime;
            }
        }

        //Handle requests
        public void RequestPage(int page)
        {
            if (_networkManager != null && _networkManager.matchMaker != null)
            {
                _nextButton.interactable = false;
                _previousButton.interactable = false;

                Debug.Log("Requesting match list");
                _networkManager.matchMaker.ListMatches(page, _pageSize, string.Empty, false, 0, 0, OnGuiMatchList);
            }
        }

        //Callback for request
        public void OnGuiMatchList(bool flag, string extraInfo, List<MatchInfoSnapshot> response)
        {
            //If no response do nothing
            if (response == null)
            {
                return;
            }

            _nextButton.interactable = true;
            _previousButton.interactable = true;

            _previousPage = _currentPage;
            _currentPage = _newPage;

            //if nothing is returned
            if (response.Count == 0)
            {
                //current page is 0 then set enable NO SERVER FOUND message
                if (_currentPage == 0)
                {
                    _noServerFound.SetActive(true);
                    ClearUi();
                    _previousButton.interactable = false;
                    _nextButton.interactable = false;
                    _pageNumber.enabled = false;
                }

                _currentPage = _previousPage;

                return;
            }

            //Prev button should not be interactable for first (zeroth) page
            _previousButton.interactable = _currentPage > 0;
            //Next button should not be interactable if the current page is not full
            _nextButton.interactable = response.Count == _pageSize;

            _noServerFound.SetActive(false);

            //Handle page number
            _pageNumber.enabled = true;
            _pageNumber.text = (_currentPage + 1).ToString();

            //Clear all transforms
            foreach (Transform t in _serverListRect)
                Destroy(t.gameObject);

            //Instantiate UI gameObjects
            for (int i = 0; i < response.Count; ++i)
            {
                GameObject o = Instantiate(_serverEntryPrefab);

                o.GetComponent<ServerEntry>().Populate(response[i], (i % 2 == 0) ? OddServerColor : EvenServerColor);

                o.transform.SetParent(_serverListRect, false);
            }
        }

        //On click of back button
        public void OnBackClick()
        {
            _networkManager.Disconnect();
            _menuUI.ShowDefaultPanel();
        }

        //Called by button clicks
        public void ChangePage(int dir)
        {
            int newPage = Mathf.Max(0, _currentPage + dir);
            this._newPage = newPage;

            //if we have no server currently displayed, need we need to refresh page0 first instead of trying to fetch any other page
            if (_noServerFound.activeSelf)
                newPage = 0;

            RequestPage(newPage);
        }

        //We just set the autorefresh time to RIGHT NOW when this button is pushed, triggering all the refresh logic in the next Update tick.
        public void RefreshList()
        {
            _nextRefreshTime = Time.time;
        }
    }
}

