using Godot;
using System;

public partial class HUD : CanvasLayer {
    private TextureProgressBar healthBar;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.healthBar = this.GetNode<TextureProgressBar>("%HealthBar");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        this.healthBar.Value = PlayerInfo.Health;
    }
}
