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
    [Export] private CharacterDatabase CharacterDatabase;

    private bool _initialized;

    private const int ACTION_CHARACTER_SELECT = 0;
    private const int ACTION_STAGE_SELECT = 1;
    private const int ACTION_MAIN_MENU = 2;

    public override void _Ready()
    {
        Visible = false;
        if (MatchManager.Instance != null)
            MatchManager.Instance.MatchEnded += OnMatchEnded;

        if (UI.Network.IsOnline && !UI.Network.Lobby.IsHost)
            UI.Network.Lobby.OnMatchResults += OnRemoteResultsReceived;
    }

    public override void _ExitTree()
    {
        if (UI.Network.IsOnline)
        {
            UI.Network.Lobby.OnResultAction -= OnResultActionReceived;
            UI.Network.Lobby.OnMatchResults -= OnRemoteResultsReceived;
        }
    }

    private void OnMatchEnded()
    {
        ShowResults(
            MatchManager.Instance.Results,
            MatchManager.Instance.Winner
        );
    }

    public void ShowResults(IReadOnlyList<ScoreResult> results, ScoreResult winner)
    {
        if (_initialized) return;

        _initialized = true;
        if (winner == null)
        {
            ShowTie();
            return;
        }

        CharacterData character = CharacterDatabase?.GetCharacter(winner.CharacterIndex);
        _portrait.Texture = character?.Illustration;
        _scoreLabel.Text = $"{winner.Score} pts";
        _playerLabel.Text = winner.PlayerName;

        if (UI.Network.IsOnline && UI.Network.Lobby.IsHost)
        {
            UI.Network.Lobby.BroadcastMatchResults(
                new MatchResultsPacket
                {
                    WinnerPlayerIndex = winner.PlayerId,
                    WinnerScore = winner.Score,
                    WinnerUsername = winner.PlayerName,
                    WinnerCharacterIndex = winner.CharacterIndex
                });
        }

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

    private void OnRemoteResultsReceived(MatchResultsPacket packet)
    {
        CharacterData character = CharacterDatabase?.GetCharacter(packet.WinnerCharacterIndex);
        if (_portrait != null) _portrait.Texture = character?.Illustration;
        if (_scoreLabel != null) _scoreLabel.Text = $"{packet.WinnerScore} pts";
        if (_playerLabel != null) _playerLabel.Text = packet.WinnerUsername;

        Visible = true;
        SetupOnlineState();
    }

    private void ShowTie()
    {
        if (_portrait != null) _portrait.Texture = null;
        if (_scoreLabel != null) _scoreLabel.Text = "Tie!";
        if (_playerLabel != null) _playerLabel.Text = "No winner";
        Visible = true;
        SetupOnlineState();
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
        
        if (UI.Network.IsOnline)
            SteamMatchManager.Instance?.ResetMatchScene();
        
        SceneManager.Instance.ChangeScene(UI.Paths.LobbyMenuScene);
    }

    private void ExecuteStageSelect()
    {
        UI.SFX.PlayOnConfirm();
        AudioManager.Instance.StartBGM(UI.Paths.MainMenuBGM);
        
        if (UI.Network.IsOnline)
            SteamMatchManager.Instance?.ResetMatchScene();
        
        SceneManager.Instance.ChangeScene(UI.Paths.LevelSelectScene);
    }

    private void ExecuteMainMenu()
    {
        UI.SFX.PlayOnConfirm();
        AudioManager.Instance.StartBGM(UI.Paths.MainMenuBGM);
        
        if (UI.Network.IsOnline)
        {
            LobbyStateService.Instance.Clear();
            SteamMatchManager.Instance?.ResetMatchScene();
        }
        
        SceneManager.Instance.ChangeScene(UI.Paths.MainMenuScene);
    }
}