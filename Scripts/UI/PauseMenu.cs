using Godot;
using System;

public partial class PauseMenu : PanelContainer {

    private static PackedScene _main_menu = ResourceLoader.Load<PackedScene>("res://Scenes/Main Menu.tscn");
    private static PauseMenu pause_menu;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.Visible = false;
        pause_menu = this;
    }
    public override void _GuiInput(InputEvent @event) {
        if (@event is not InputEventKey key || !key.IsPressed() || key.PhysicalKeycode != Key.Escape) {
            return;
        }

        this.AcceptEvent();
        toggle();
    }

    public override void _UnhandledKeyInput(InputEvent @event) {
        if (@event is not InputEventKey key || !key.IsPressed() || key.PhysicalKeycode != Key.Escape) {
            return;
        }

        if ((Utils.ui_open == 1 && paused) || (Utils.ui_open == 0 && !paused)) {
            this.GetViewport().SetInputAsHandled();
            toggle();
        }
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
    private static bool paused = false;

    public static void toggle() {
        if (paused) {
            Engine.TimeScale = time_scale;
            time_scale = 1d;
            --Utils.ui_open;
        } else {
            ++Utils.ui_open;
            time_scale = Engine.TimeScale;
        }
        paused = !paused;
        pause_menu.Visible = !pause_menu.Visible;
        pause_menu.GetTree().Paused = paused;
    }
}
