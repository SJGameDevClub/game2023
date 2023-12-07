using Godot;
using System;

public partial class HUD : CanvasLayer {
    private TextureProgressBar health_bar;
    public Inventory player_inventory;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.health_bar = this.GetNode<TextureProgressBar>("%HealthBar");
        this.player_inventory = this.GetNode<Inventory>("%Inventory");
        Test.LoadPlayerInv(this.player_inventory);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        this.health_bar.Value = PlayerInfo.Health;
    }
}
