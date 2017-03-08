using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace  MPLobbyExample.UI
{
    public class LobbyInfoPanel : MonoBehaviour
    {

        [SerializeField]
        protected Text _infoText;
        [SerializeField]
        protected Text _buttonText;
        [SerializeField]
        protected Button _singleButton;

        public void Display(string info, UnityEngine.Events.UnityAction buttonClbk, bool displayButton = true)
        {
            _infoText.text = info;

            _singleButton.gameObject.SetActive(displayButton);
            _singleButton.onClick.RemoveAllListeners();
            if (buttonClbk != null)
            {
                _singleButton.onClick.AddListener(buttonClbk);
            }

            _singleButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });

            gameObject.SetActive(true);
        }
    }
}