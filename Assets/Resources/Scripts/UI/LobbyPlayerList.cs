using MPLobbyExample.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace MPLobbyExample.UI
{
    /// <summary>
    /// Lobby player list.
    /// </summary>
    public class LobbyPlayerList : MonoBehaviour
    {
        public static LobbyPlayerList Instance = null;

        [SerializeField]
        protected RectTransform _playerListContentTransform;
        [SerializeField]
        protected GameObject _warningDirectPlayServer;

        private MPNetworkManager _netManager;

        protected virtual void Awake()
        {
            Instance = this;
        }

        //Subscribe to events on start
        protected virtual void Start()
        {
            _netManager = MPNetworkManager.singleton.gameObject.GetComponent<MPNetworkManager>();
            if (_netManager != null)
            {
                _netManager.playerJoined += PlayerJoined;
                _netManager.playerLeft += PlayerLeft;
                _netManager.serverPlayersReadied += PlayersReadied;
            }
        }

        //Unsubscribe to events on destroy
        protected virtual void OnDestroy()
        {
            if (_netManager != null)
            {
                _netManager.playerJoined -= PlayerJoined;
                _netManager.playerLeft -= PlayerLeft;
                _netManager.serverPlayersReadied -= PlayersReadied;
            }
        }

        //Used in direct play - display warning
        public void DisplayDirectServerWarning(bool enabled)
        {
            if (_warningDirectPlayServer != null)
                _warningDirectPlayServer.SetActive(enabled);
        }

        //Add lobby player to UI
        public void AddPlayer(LobbyPlayer player)
        {
            Debug.Log("Add player to list");
            player.transform.SetParent(_playerListContentTransform, false);
        }

        //Log player joining for tracing
        protected virtual void PlayerJoined(MPNetworkPlayer player)
        {
            Debug.LogFormat("Player joined {0}", player.name);
        }

        //Log player leaving for tracing
        protected virtual void PlayerLeft(MPNetworkPlayer player)
        {
            Debug.LogFormat("Player left {0}", player.name);
        }

        //When players are all ready progress
        protected virtual void PlayersReadied()
        {
            _netManager.ProgressToGameScene();
        }
    }
}
