using Godot;
using System;

public partial class MainMenu : CanvasLayer {

    // public static PackedScene test_level = ResourceLoader.Load<PackedScene>("res://Scenes/test.tscn");

    public override void _Ready() {
        ++Utils.ui_open;
    }

    public void play() {
        --Utils.ui_open;
        Global.global.load(ResourceLoader.Load<Level>("res://TestData/test.tres"));
    }

    public void save_menu() {
        SaveScreen.openSaveMenu(this);
    }

    public void settings() {
        Settings.openSettings(this);
    }

    public void quit() {
        Utils.Quit(this);
    }
}
