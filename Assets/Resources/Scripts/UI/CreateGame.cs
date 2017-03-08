using MPLobbyExample.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace MPLobbyExample.UI
{
    /// <summary>
    /// Governs the Create Game functionality in the main menu.
    /// </summary>
    public class CreateGame : MonoBehaviour
    {
        [SerializeField]
        //Internal reference to the InputField used to enter the server name.
        protected InputField _matchNameInput;

        //Cached references to other UI singletons.
        private MainMenuUI _menuUi;
        private MPNetworkManager _netManager;

        protected virtual void Start()
        {
            _menuUi = MainMenuUI.Instance;
            _netManager = MPNetworkManager.singleton.gameObject.GetComponent<MPNetworkManager>();
        }

        /// <summary>
        /// Back button method. Returns to main menu.
        /// </summary>
        public void OnBackClicked()
        {
            _menuUi.ShowDefaultPanel();
        }

        /// <summary>
        /// Create button method. Validates entered server name and launches game server.
        /// </summary>
        public void OnCreateClicked()
        {
            if (string.IsNullOrEmpty(_matchNameInput.text))
            {
                _menuUi.ShowInfoPopup("Server name cannot be empty!", null);
                return;
            }

            StartMatchmakingGame();
        }

        /// <summary>
        /// Populates game settings for broadcast to clients and attempts to start matchmaking server session.
        /// </summary>
        private void StartMatchmakingGame()
        {
            //_menuUi.ShowConnectingModal(false);

            Debug.Log(GetGameName());
            _netManager.StartMatchmakingGame(GetGameName(), (success, matchInfo) =>
            {
                if (!success)
                {
                    _menuUi.ShowInfoPopup("Failed to create game.", null);
                }
                else
                {
                    _menuUi.HideInfoPopup();
                    _menuUi.ShowLobbyPanel();
                }
            });
        }

        //Returns a formatted string containing server name and game mode information.
        private string GetGameName()
        {
            return string.Format("|{0}| {1}", "MPLobbyExampleMatch", _matchNameInput.text);
        }
    }
}