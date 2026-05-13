using Godot;
using System;

public partial class UISFX
{
    private UIService UI => UIService.Instance;
    private AudioStreamPlayer _player;

    private string _confirm;
    private string _toggle;
    private string _hover;

    public UISFX(
        AudioStreamPlayer player,
        string confirm,
        string toggle,
        string hover)
    {
        _player = player;
        _confirm = confirm;
        _toggle = toggle;
        _hover = hover;
    }

    public void PlayOnConfirm() => UI.Audio.PlaySFX(_confirm, _player);
    public void PlayOnToggle() => UI.Audio.PlaySFX(_toggle, _player);
    public void PlayOnHover() => UI.Audio.PlaySFX(_hover, _player);
}
