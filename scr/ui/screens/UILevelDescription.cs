using Godot;

public partial class UILevelDescription : Control
{
    [Export] public Godot.Collections.Array<LevelData> Levels = new();
    [Export] public NodePath ButtonsContainerPath;
    [Export] public NodePath DescriptionLabelPath;

    private Container _buttonsContainer;
    private Label _description;

    public override void _Ready()
    {
        _buttonsContainer =
            GetNode<Container>(ButtonsContainerPath);

        _description =
            GetNode<Label>(DescriptionLabelPath);

        RegisterHoverEvents();

        SetDescription(null);
    }

    private void RegisterHoverEvents()
    {
        for (int i = 0; i < Levels.Count; i++)
        {
            int index = i;

            if (_buttonsContainer.GetChild(i)
                is not Button button)
                continue;

            button.MouseEntered += () =>
            {
                SetDescription(Levels[index]);
            };

            button.FocusEntered += () =>
            {
                SetDescription(Levels[index]);
            };
        }
    }

    private void SetDescription(LevelData level)
    {
        if (level == null)
        {
            _description.Text =
                "Select a level.";
            return;
        }

        _description.Text =
            level.Description;
    }
}