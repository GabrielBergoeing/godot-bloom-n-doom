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

    private bool _initialized;

    public override void _Ready()
    {
        Visible = false;
        RegisterButtons();

        if (MatchManager.Instance != null)
            MatchManager.Instance.MatchEnded += OnMatchEnded;
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

    private void OnMatchEnded()
    {
        ShowResults(MatchManager.Instance.Results);
    }

    public void ShowResults(List<ScoreResult> results)
    {
        if (_initialized)
            return;

        _initialized = true;
        if (results == null || results.Count == 0)
        {
            GD.PushError(
                "[UIMatchResults] Empty results."
            );

            return;
        }

        var sorted = results
            .OrderByDescending(r => r.Score)
            .ToList();

        ScoreResult winner = sorted[0];
        if (winner.PlayerIndex < 0 || winner.PlayerIndex >= UI.Game.LobbyPlayers.Count)
        {
            GD.PushError(
                $"[UIMatchResults] Invalid winner index {winner.PlayerIndex}"
            );

            return;
        }

        LobbyPlayerData player = UI.Game.LobbyPlayers[
                winner.PlayerIndex
            ];

        CharacterData character = player.SelectedCharacter;
        if (_portrait != null)
            _portrait.Texture = character.Illustration;

        if (_scoreLabel != null)
            _scoreLabel.Text = $"{winner.Score} pts";

        if (_playerLabel != null)
            _playerLabel.Text = $"Player {winner.PlayerIndex + 1}";

        Visible = true;
        _stageSelectButton?.GrabFocus();
    }

    private void GoToCharacterSelect()
    {
        UI.SFX.PlayOnConfirm();

        AudioManager.Instance.StartBGM(
            UI.Paths.MainMenuBGM
        );

        SceneManager.Instance.ChangeScene(
            UI.Paths.LobbyMenuScene
        );
    }

    private void GoToStageSelect()
    {
        UI.SFX.PlayOnConfirm();

        AudioManager.Instance.StartBGM(
            UI.Paths.MainMenuBGM
        );

        SceneManager.Instance.ChangeScene(
            UI.Paths.LevelSelectScene
        );
    }

    private void GoToMainMenu()
    {
        UI.SFX.PlayOnConfirm();

        AudioManager.Instance.StartBGM(
            UI.Paths.MainMenuBGM
        );

        SceneManager.Instance.ChangeScene(
            UI.Paths.MainMenuScene
        );
    }
}