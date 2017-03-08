using System.Collections;
using System.Collections.Generic;
using MPLobbyExample.Networking;
using MPLobbyExample.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace MPLobbyExample.UI
{
    public class SinglePlayerPanel : MonoBehaviour
    {

        //Internal references to netManager and menu controller.
        private MPNetworkManager _netManager;
        private MainMenuUI _menuUi;

        protected virtual void OnEnable()
        {
            //Get fresh references to controllers
            _netManager = MPNetworkManager.singleton.gameObject.GetComponent<MPNetworkManager>();
            _menuUi = MainMenuUI.Instance;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        //Assigned to back button. Ends SP server session and leaves the SinglePlayer menu.
        public void OnBackClick()
        {
            _netManager.Disconnect();
            _menuUi.ShowDefaultPanel();
        }

        //Assigned to Start button. Loads up gamedata and begins the mission.
        public void OnStartClick()
        {
            _netManager.ProgressToGameScene();
        }
    }
}
