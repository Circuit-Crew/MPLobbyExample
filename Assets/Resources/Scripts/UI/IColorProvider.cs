using MPLobbyExample.Networking;
using UnityEngine;

namespace MPLobbyExample.UI
{
    /// <summary>
    /// Interface to source colours for a given player or team from the server.
    /// </summary>
    public interface IColorProvider
    {
        Color ServerGetColor(MPNetworkPlayer player);
        void Reset();
    }
}