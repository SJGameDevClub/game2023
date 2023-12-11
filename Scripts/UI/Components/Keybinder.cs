using Godot;
using System;

public partial class Keybinder : HBoxContainer {

    [Signal]
    public delegate void on_clickEventHandler(int index, bool alt);
    [Signal]
    public delegate void on_resetEventHandler();
    [Signal]
    public delegate void on_clearEventHandler();
    public int index = -1;
    protected Button shortcut;
    protected Button shortcut_alt;
    protected RichTextLabel keybind_name;
    private string _name = "";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.shortcut = this.GetNode<Button>("Shortcut");
        this.shortcut_alt = this.GetNode<Button>("Shortcut Alt");
        this.keybind_name = this.GetNode<RichTextLabel>("Keybind Name");
        this.keybind_name.Text = this._name;
        this.shortcut.Pressed += () => {
            EmitSignal("on_click", this.index, false);
        };
        this.shortcut_alt.Pressed += () => {
            EmitSignal("on_click", this.index, true);
        };
        this.GetNode<Button>("Reset").Pressed += () => {
            EmitSignal("on_reset");
        };
        this.GetNode<Button>("Clear").Pressed += () => {
            EmitSignal("on_clear");
        };
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    public void setIndex(int i) {
        this.index = i;
    }
    
    public void setText(string text, bool alt) {
        (alt ? this.shortcut_alt : this.shortcut).Text = text;
    }

    public void setKeybindName(string name) {
        if (this.keybind_name == null) {
            this._name = name;
            return;
        }
        this.keybind_name.Text = name;
    }
}
