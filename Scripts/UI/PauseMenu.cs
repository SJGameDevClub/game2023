using Godot;
using System;

public partial class PauseMenu : CanvasLayer {

    private static PackedScene _main_menu = ResourceLoader.Load<PackedScene>("res://Scenes/Main Menu.tscn");
    private static PauseMenu pause_menu;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.Visible = false;
        pause_menu = this;
        
    }

    public override void _UnhandledKeyInput(InputEvent @event) {
        if (@event is not InputEventKey key || !key.IsPressed() || key.PhysicalKeycode != Key.Escape) {
            return;
        }
        
        if ((Utils.ui_open == 1 && pause_menu.Visible) || (Utils.ui_open == 0 && !pause_menu.Visible)) {
            this.GetViewport().SetInputAsHandled();
            toggle();
        }
    }

    public void save_menu() {
        SaveScreen.openSaveMenu(this);
    }

    public void settings() {
        Settings.openSettings(this);
    }

    public void main_menu() {
        toggle();
        this.GetTree().ChangeSceneToPacked(_main_menu);
    }

    public void quit() {
        Utils.Quit(this);
    }

    private static double time_scale = 1d;

    public static void toggle() {
        if (pause_menu.Visible) {
            Engine.TimeScale = time_scale;
            time_scale = 1d;
            --Utils.ui_open;
        } else {
            ++Utils.ui_open;
            time_scale = Engine.TimeScale;
        }
        pause_menu.Visible = !pause_menu.Visible;
        pause_menu.GetTree().Paused = pause_menu.Visible;
    }

    public static void ensureClosed() {
        pause_menu.Visible = false;
    }
}
