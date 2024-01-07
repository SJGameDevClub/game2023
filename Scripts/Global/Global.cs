using Godot;
using System;
using System.IO;
using System.Threading.Tasks;

public partial class Global : Node {

    public static Global global {get; protected set;}

    public override void _UnhandledInput(InputEvent @event) {
        base._UnhandledInput(@event);
        if (@event is not InputEventKey key || !key.Pressed) {
            return;
        }

        if (Test.run) {
            if (key.Keycode == Key.Bracketright) {
                this.load(ResourceLoader.Load<Level>("user://saves/save_.tres"));
            } else if (key.Keycode == Key.Bracketleft) {
                async void _save() {
                    var path = ProjectSettings.GlobalizePath("user://saves");
                    if (!Directory.Exists(path)) {
                        Directory.CreateDirectory(path);
                    }
                    ResourceSaver.Save(await this.memsave(), "user://saves/save_.tres");
                }
                _save();
            }
        }

    }

    public override void _Ready() {
        global = this;
    }

    public override void _Notification(int what) {
        base._Notification(what);
        if (what == (int) NotificationWMCloseRequest) {
            GD.PrintS("Saving Pos");
            Graphics.graphics.win_position = DisplayServer.WindowGetPosition();
            Graphics.save(Graphics.graphics);
        }
    }

    public async Task<Level> memsave() {
        GD.PrintS("Saving");
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        PackedScene scene = new();
        scene.Pack(this.GetTree().CurrentScene);
        Level level = new() {
            screenshot = this.GetViewport().GetTexture().GetImage().SaveWebpToBuffer(),
            player_health = PlayerInfo.Health,
            saved_scene = scene,
            player_inventory = InventoryGui.Save(HUD.hud.player_inventory),
            time_scale = Engine.TimeScale
        };

        return level;
    }

    public void load(Level level) {
        GD.Print("Loading");
        if (this.GetTree().ChangeSceneToPacked(level.saved_scene) != Error.Ok) {
            GD.PrintS("Error Loading Saved Level");
            return;
        }
        PlayerInfo.Health = level.player_health;
        Utils.ui_open = 0;
        PauseMenu.ensureClosed();
        Engine.TimeScale = level.time_scale;
        InventoryGui.Load(HUD.hud.player_inventory, level.player_inventory);
        this.GetTree().Paused = false;
    }

    /// <summary>
    /// an alias of memsave
    /// </summary>
    public async Task<Level> save() => await this.memsave();
}
