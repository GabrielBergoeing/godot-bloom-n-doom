using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class UIMatchResults : Control
{
    private UIService UI => UIService.Instance;

    [ExportGroup("Winner UI")]
    [Export] private TextureRect _portrait;
    [Export] private Label _scoreLabel;
    [Export] private Label _playerLabel;

    [ExportGroup("Buttons")]
    [Export] private TextureButton _characterSelectButton;
    [Export] private TextureButton _stageSelectButton;
    [Export] private TextureButton _mainMenuButton;

    [Export] private Label _waitingLabel;

    private bool _initialized;

    private const int ACTION_CHARACTER_SELECT = 0;
    private const int ACTION_STAGE_SELECT = 1;
    private const int ACTION_MAIN_MENU = 2;

    public override void _Ready()
    {
        Visible = false;
        if (MatchManager.Instance != null)
            MatchManager.Instance.MatchEnded += OnMatchEnded;
    }

    public override void _ExitTree()
    {
        if (UI.Network.IsOnline)
            UI.Network.Lobby.OnResultAction -= OnResultActionReceived;
    }

    private void OnMatchEnded()
    {
        ShowResults(MatchManager.Instance.Results);
    }

    public void ShowResults(List<ScoreResult> results)
    {
        if (_initialized) return;
        _initialized = true;

        if (results == null || results.Count == 0)
        {
            GD.PushError("[UIMatchResults] Empty results.");
            return;
        }

        var sorted = results.OrderByDescending(r => r.Score).ToList();
        ScoreResult winner = sorted[0];

        if (winner.PlayerIndex < 0 || winner.PlayerIndex >= UI.Game.LobbyPlayers.Count)
        {
            GD.PushError($"[UIMatchResults] Invalid winner index {winner.PlayerIndex}");
            return;
        }

        LobbyPlayerData player = UI.Game.LobbyPlayers[winner.PlayerIndex];
        CharacterData character = player.SelectedCharacter;

        if (_portrait != null) _portrait.Texture = character?.Illustration;
        if (_scoreLabel != null) _scoreLabel.Text = $"{winner.Score} pts";
        if (_playerLabel != null) _playerLabel.Text = $"Player {winner.PlayerIndex + 1}";

        Visible = true;
        SetupOnlineState();
    }

    private void SetupOnlineState()
    {
        if (!UI.Network.IsOnline)
        {
            if (_waitingLabel != null) _waitingLabel.Visible = false;
            RegisterButtons();
            _stageSelectButton?.GrabFocus();
            return;
        }

        if (UI.Network.Lobby.IsHost)
        {
            if (_waitingLabel != null) _waitingLabel.Visible = false;
            RegisterButtons();
            _stageSelectButton?.GrabFocus();
        }
        else
        {
            // Peer: disable buttons, show waiting label
            SetButtonsDisabled(true);
            _characterSelectButton?.ReleaseFocus();
            _stageSelectButton?.ReleaseFocus();
            _mainMenuButton?.ReleaseFocus();

            if (_waitingLabel != null)
            {
                _waitingLabel.Visible = true;
                _waitingLabel.Text = "Waiting for host...";
            }

            UI.Network.Lobby.OnResultAction += OnResultActionReceived;
        }
    }

    private void RegisterButtons()
    {
        if (_characterSelectButton != null)
            _characterSelectButton.Pressed += GoToCharacterSelect;
        if (_stageSelectButton != null)
            _stageSelectButton.Pressed += GoToStageSelect;
        if (_mainMenuButton != null)
            _mainMenuButton.Pressed += GoToMainMenu;
    }

    private void SetButtonsDisabled(bool disabled)
    {
        if (_characterSelectButton != null) _characterSelectButton.Disabled = disabled;
        if (_stageSelectButton != null) _stageSelectButton.Disabled = disabled;
        if (_mainMenuButton != null) _mainMenuButton.Disabled = disabled;
    }

    private void OnResultActionReceived(int action)
    {
        GD.Print($"[UIMatchResults] Peer received result action: {action}");
        if (action == ACTION_MAIN_MENU && UI.Network.IsOnline)
            UI.Network.Lobby.LeaveLobby();
        switch (action)
        {
            case ACTION_CHARACTER_SELECT: ExecuteCharacterSelect(); break;
            case ACTION_STAGE_SELECT: ExecuteStageSelect(); break;
            case ACTION_MAIN_MENU: ExecuteMainMenu(); break;
        }
    }

    private void GoToCharacterSelect()
    {
        if (UI.Network.IsOnline)
            UI.Network.Lobby.BroadcastResultAction(ACTION_CHARACTER_SELECT);
        ExecuteCharacterSelect();
    }

    private void GoToStageSelect()
    {
        if (UI.Network.IsOnline)
            UI.Network.Lobby.BroadcastResultAction(ACTION_STAGE_SELECT);
        ExecuteStageSelect();
    }

    private void GoToMainMenu()
    {
        if (UI.Network.IsOnline)
            UI.Network.Lobby.BroadcastResultAction(ACTION_MAIN_MENU);
        ExecuteMainMenu();
    }

    private void ExecuteCharacterSelect()
    {
        UI.SFX.PlayOnConfirm();
        AudioManager.Instance.StartBGM(UI.Paths.MainMenuBGM);
        SceneManager.Instance.ChangeScene(UI.Paths.LobbyMenuScene);
    }

    private void ExecuteStageSelect()
    {
        UI.SFX.PlayOnConfirm();
        AudioManager.Instance.StartBGM(UI.Paths.MainMenuBGM);
        SceneManager.Instance.ChangeScene(UI.Paths.LevelSelectScene);
    }

    private void ExecuteMainMenu()
    {
        UI.SFX.PlayOnConfirm();
        AudioManager.Instance.StartBGM(UI.Paths.MainMenuBGM);
        SceneManager.Instance.ChangeScene(UI.Paths.MainMenuScene);
    }
}