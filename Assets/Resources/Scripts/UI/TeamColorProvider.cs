using UnityEngine;
using System.Collections.Generic;
using MPLobbyExample.Networking;


namespace MPLobbyExample.UI
{
    /// <summary>
    /// Color provider for team based games
    /// </summary>
    public class TeamColorProvider : IColorProvider
    {
        //Available colors
        private List<Color> _colors = new List<Color>() { Color.red, Color.blue };

        private List<MPNetworkPlayer> _players = new List<MPNetworkPlayer>();

        private int _lastUsedColorIndex = -1;

        //Get the available color
        public Color ServerGetColor(MPNetworkPlayer player)
        {
            int playerIndex = _players.IndexOf(player);
            if (playerIndex == -1)
            {
                _players.Add(player);
                playerIndex = _players.Count - 1;
            }

            Color playerColor = player.Color;
            int index = _colors.IndexOf(playerColor);
            if (index == -1)
            {
                //Ensure that the first two tanks aren't both the same colours
                index = Random.Range(0, _colors.Count);
                while (index == _lastUsedColorIndex)
                {
                    index = Random.Range(0, _colors.Count);
                }
                _lastUsedColorIndex = index;
            }
            else
            {
                index++;
            }


            if (index == _colors.Count)
            {
                index = 0;
            }

            Color newColor = _colors[index];
            if (CanUseColor(newColor, playerIndex))
            {
                return newColor;
            }

            return playerColor;
        }

        public void SetupColors(List<Color> colors)
        {
            this._colors = colors;
        }

        //Ensures that there are least two teams
        private bool CanUseColor(Color newColor, int playerIndex)
        {
            if (_players.Count == 1)
            {
                return true;
            }

            for (int i = 0; i < _players.Count; i++)
            {
                if (i != playerIndex)
                {
                    if (_players[i].Color != newColor)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Reset()
        {
            _players.Clear();
            _lastUsedColorIndex = -1;
        }
    }
}