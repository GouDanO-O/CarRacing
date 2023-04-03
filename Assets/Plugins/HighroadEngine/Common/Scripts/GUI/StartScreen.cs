using UnityEngine;
using System.Collections;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    /// Simple class to allow the player to select a scene on the start screen
    /// </summary>
    public class StartScreen : MonoBehaviour
    {
        [Header("SingleMapLobby")]
        /// the name of the basic racing game
        public string SingleMapLobbySceneName;

        [Header("TeamMapLobby")]
        /// the name of the aphex scene / online version
        public string TeamMapLobbySceneName;

        public virtual void OnSingleMapLobbyGameClick()
        {
            RemoveBackgroundGame();
            LoadingSceneManager.LoadScene(SingleMapLobbySceneName);
            RaceManager.whitchGameMod = false;
        }

        public virtual void OnTeamMapLobbyGameClick()
        {
            RemoveBackgroundGame();
            LoadingSceneManager.LoadScene(TeamMapLobbySceneName);
            RaceManager.whitchGameMod = true;
        }

        protected virtual void RemoveBackgroundGame()
        {
            // We need to remove LocalLobby since it's a persistent object
            Destroy(LocalLobbyManager.Instance.gameObject);
        }
    }
}