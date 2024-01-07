using Godot;
using System;

public partial class HUD : CanvasLayer {

    public static HUD hud {get; private set;}
    private TextureProgressBar health_bar;
    public InventoryGui player_inventory;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.health_bar = this.GetNode<TextureProgressBar>("%HealthBar");
        this.player_inventory = this.GetNode<InventoryGui>("%Inventory");
        hud = this;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        this.health_bar.Value = PlayerInfo.Health;
        if (Utils.ui_open == 0) {
            this.Visible = true;
        } else {
            this.Visible = false;
        }
    }
}
