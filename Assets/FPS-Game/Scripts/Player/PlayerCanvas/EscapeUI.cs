using System;
using UnityEngine;
using UnityEngine.UI;

public class EscapeUI : PlayerBehaviour
{
    public Button QuitGameButton;

    void Awake()
    {
        // Removed: Lobby check - always show quit button
        // if (!LobbyManager.Instance.IsLobbyHost()) QuitGameButton.gameObject.SetActive(false);

        QuitGameButton.onClick.AddListener(() =>
        {
            PlayerRoot.Events.InvokeOnQuitGame();
        });
    }
}