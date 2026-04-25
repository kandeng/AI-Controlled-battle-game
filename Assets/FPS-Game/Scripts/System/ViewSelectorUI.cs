using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime UI overlay (top-right corner) showing a dropdown of all non-bot players.
/// Lets the user click to follow any player's Cinemachine camera.
///
/// Created automatically by PlayerCommandAPI at startup — no scene setup needed.
/// Visible only in UNITY_EDITOR and DEVELOPMENT_BUILD.
/// </summary>
public class ViewSelectorUI : MonoBehaviour
{
    public static ViewSelectorUI Instance { get; private set; }

    // -------------------------------------------------------
    // Internal state
    // -------------------------------------------------------

    private List<PlayerRoot> _players = new();
    private int _selectedIndex = 0;
    private bool _dropdownOpen = false;

    // GUI skin/layout constants
    private const float PanelWidth  = 220f;
    private const float RowHeight   = 28f;
    private const float PanelX      = 10f;   // offset from right edge
    private const float PanelY      = 10f;   // offset from top
    private GUIStyle _headerStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _selectedStyle;

    // -------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Rebuild the player list and update the highlight after a SET_VIEW command.
    /// </summary>
    public void RefreshSelection(PlayerRoot activePlayer)
    {
        RefreshPlayers();
        int idx = _players.IndexOf(activePlayer);
        if (idx >= 0) _selectedIndex = idx;
    }

    // -------------------------------------------------------
    // OnGUI
    // -------------------------------------------------------

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void OnGUI()
    {
        InitStyles();
        RefreshPlayers();
        if (_players.Count == 0) return;

        float x = Screen.width - PanelWidth - PanelX;
        float y = PanelY;

        // Header / toggle button
        string label = _players.Count > 0
            ? $"View: {_players[_selectedIndex].gameObject.name} ▾"
            : "View: — ▾";

        if (GUI.Button(new Rect(x, y, PanelWidth, RowHeight), label, _headerStyle))
            _dropdownOpen = !_dropdownOpen;

        // Dropdown rows
        if (_dropdownOpen)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                float rowY = y + RowHeight * (i + 1);
                GUIStyle style = (i == _selectedIndex) ? _selectedStyle : _buttonStyle;
                string name = _players[i].gameObject.name;

                if (GUI.Button(new Rect(x, rowY, PanelWidth, RowHeight), name, style))
                {
                    _selectedIndex = i;
                    _dropdownOpen  = false;
                    PlayerCommandAPI.Instance?.SetViewToPlayer(_players[i]);
                }
            }
        }
    }
#endif

    // -------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------

    void RefreshPlayers()
    {
        _players.Clear();
        foreach (var p in FindObjectsByType<PlayerRoot>(FindObjectsInactive.Exclude))
            if (!p.IsCharacterBot())
                _players.Add(p);

        // Keep selected index in range
        if (_selectedIndex >= _players.Count) _selectedIndex = 0;
    }

    void InitStyles()
    {
        if (_headerStyle != null) return;

        _headerStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle  = FontStyle.Bold,
            fontSize   = 12
        };
        _headerStyle.normal.background = MakeTex(1, 1, new Color(0.1f, 0.1f, 0.1f, 0.85f));
        _headerStyle.normal.textColor  = Color.white;

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize   = 12
        };
        _buttonStyle.normal.background = MakeTex(1, 1, new Color(0.15f, 0.15f, 0.15f, 0.85f));
        _buttonStyle.normal.textColor  = Color.white;

        _selectedStyle = new GUIStyle(_buttonStyle);
        _selectedStyle.normal.background = MakeTex(1, 1, new Color(0.2f, 0.5f, 0.9f, 0.9f));
    }

    static Texture2D MakeTex(int w, int h, Color col)
    {
        var pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        var t = new Texture2D(w, h);
        t.SetPixels(pix);
        t.Apply();
        return t;
    }
}
