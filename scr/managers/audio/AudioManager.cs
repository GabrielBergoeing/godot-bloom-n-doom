using Godot;
using System.Threading.Tasks;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    [Export] private AudioDatabase _database;
    [Export] private AudioStreamPlayer _bgmPlayer;

    private string _currentBgmGroup = "";
    private bool _bgmShouldPlay = false;

    public override void _Ready()
    {
        Instance = this;

        _database.Initialize();
        _bgmPlayer.Finished += OnBgmFinished;
    }

    private void OnBgmFinished()
    {
        if (_bgmShouldPlay && !string.IsNullOrEmpty(_currentBgmGroup))
            _ = NextBGM(_currentBgmGroup);
    }

    public void PlaySFX(string soundName, AudioStreamPlayer player)
    {
        var data = _database.Get(soundName);
        if (data == null)
        {
            GD.Print($"Missing sound: {soundName}");
            return;
        }

        var clip = data.GetRandomClip();
        if (clip == null)
            return;

        player.Stream = clip;
        player.PitchScale =
            (float)GD.RandRange(
                data.Pitch - 0.05f,
                data.Pitch + 0.05f);

        player.VolumeDb = LinearToDb(data.Volume);
        player.Play();
    }

    public async Task StartBGM(string musicGroup)
    {
        _bgmShouldPlay = true;
        if (musicGroup == _currentBgmGroup)
            return;

        await NextBGM(musicGroup);
    }

    public async Task NextBGM(string musicGroup)
    {
        _bgmShouldPlay = true;
        _currentBgmGroup = musicGroup;

        var data = _database.Get(musicGroup);
        if (data == null)
        {
            GD.Print($"Missing BGM group: {musicGroup}");
            return;
        }

        var clip = data.GetRandomClip();
        if (clip == null)
            return;

        if (_bgmPlayer.Playing)
            await FadeVolume(_bgmPlayer, -80f, 1f);

        _bgmPlayer.Stream = clip;
        _bgmPlayer.VolumeDb = -80f;
        _bgmPlayer.Play();

        await FadeVolume(
            _bgmPlayer,
            LinearToDb(data.Volume),
            1f
        );
    }

    public async Task StopBGM()
    {
        _bgmShouldPlay = false;
        await FadeVolume(_bgmPlayer, -80f, 1f);
        _bgmPlayer.Stop();
    }

    public void PlayOneShot(string soundName)
    {
        var data = _database.Get(soundName);
        var clip = data.GetRandomClip();

        AudioStreamPlayer player = new();

        AddChild(player);

        player.Stream = clip;
        player.Play();
        player.Finished += () => player.QueueFree();
    }

    private async Task FadeVolume(
        AudioStreamPlayer player,
        float targetDb,
        float duration)
    {
        Tween tween = CreateTween();
        tween.TweenProperty(
            player,
            "volume_db",
            targetDb,
            duration);

        await ToSignal(tween, Tween.SignalName.Finished);
    }

    private float LinearToDb(float linear)
    {
        if (linear <= 0f)
            return -80f;
        return Mathf.LinearToDb(linear);
    }
}