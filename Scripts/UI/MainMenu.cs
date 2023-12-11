using Godot;
using System;

public partial class MainMenu : CanvasLayer {

    public override void _Ready() {
        ++Utils.ui_open;
    }

    public void play() {
        --Utils.ui_open;
        GD.Print(this.GetTree().ChangeSceneToFile("res://Scenes/test.tscn"));
    }

    public void settings() {
        Settings.openSettings(this);
    }

    public void quit() {
        Utils.Quit(this);
    }
}
