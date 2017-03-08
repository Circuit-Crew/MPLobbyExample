using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Circuit.UI
{
    [RequireComponent(typeof(Button))]

    /// <summary>
    /// Tracks opening order of menus and modals to fire the correct back button delegate when Android back button/Esc key is pressed.
    /// </summary>
    public class BackButton : MonoBehaviour
    {
        private static List<BackButton> _buttonStack = new List<BackButton>();

        protected Button m_BackButton;

        protected virtual void OnEnable()
        {
            m_BackButton = GetComponent<Button>();

            _buttonStack.Add(this);
        }

        protected virtual void OnDisable()
        {
            // Pop this button
            _buttonStack.Remove(this);
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Make sure we're on top of the stack and are enabled and interactive
                if (_buttonStack.Count > 0 &&
                    _buttonStack[_buttonStack.Count - 1] == this &&
                    m_BackButton.interactable &&
                    m_BackButton.enabled)
                {
                    OnBackPressed();
                }
            }
        }

        //We use the back button logic on this screen to ensure clean closure.
        protected virtual void OnBackPressed()
        {
            m_BackButton.onClick.Invoke();
        }
    }
}