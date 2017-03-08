using UnityEngine;
using MPLobbyExample.Networking;

namespace MPLobbyExample.UI
{
    /// <summary>
    /// Lobby panel
    /// </summary>
    public class LobbyPanel : MonoBehaviour
    {
        private MainMenuUI _menuUi;
        private MPNetworkManager _netManager;

        protected virtual void Start()
        {
            _menuUi = MainMenuUI.Instance;
            _netManager = MPNetworkManager.singleton.gameObject.GetComponent<MPNetworkManager>();
        }

        public void OnBackClick()
        {
            Back();
        }

        private void Back()
        {
            _netManager.Disconnect();
            _menuUi.ShowDefaultPanel();
        }
    }
}