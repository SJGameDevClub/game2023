using Godot;
using System;

public partial class HUD : CanvasLayer {

    public static HUD hud {get; private set;}
    private TextureProgressBar health_bar;
    public InventoryGui player_inventory;
    public Slot held_item_container;
    public RichTextLabel result;
    public Panel result_panel;
    public LineEdit command;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        if (held_item_container == null) {
            held_item_container = ResourceLoader.Load<PackedScene>("res://Nodes/UI/Components/Slot.tscn").Instantiate<Slot>();
            held_item_container.Name = "Hover Item";
            held_item_container.is_hover = true;
            held_item_container.Visible = false;
            this.AddChild(held_item_container);
            held_item_container.Owner = this;
        }
        this.health_bar = this.GetNode<TextureProgressBar>("%HealthBar");
        this.player_inventory = this.GetNode<InventoryGui>("%Inventory");
        this.result = this.GetNode<RichTextLabel>("%Result");
        this.command = this.GetNode<LineEdit>("%Command");
        this.result_panel = (Panel) this.result.GetParent();
        hud = this;
    }

    double timer = 3;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        this.health_bar.Value = PlayerInfo.Health;
        if (Utils.ui_open == 0) {
            this.Visible = true;
        } else {
            this.Visible = false;
            return;
        }
        if (this.is_hovering || this.is_typing) {
            this.result_panel.Modulate = new (1,1,1,1);
            this.timer = 3;
            this.is_typing = false;
        }
        if (this.timer <= 0) {
            Color color = this.result_panel.Modulate;
            this.result_panel.Modulate = new (color.R, color.G, color.B, color.A - (1 * (float) delta));
        }
        // GD.PrintS(this.timer, this.timer - delta);
        this.timer -= delta;
    }

    public void on_command(string raw_command) {
        string[] raw_input = raw_command.Split(' ');
        this.is_typing = false;
        string command = raw_input[0];
        string[] args = raw_input[1..];
        this.result.AppendText(raw_command + "\n");
        this.command.Text = "";
        this.command.ReleaseFocus();
        Utils.text_focused = false;
        try {
            switch (command) {
                case "set_health": {
                    PlayerInfo.Health = int.Parse(args[0]);
                    this.result.AppendText($"Health set to {args[0]}");
                    break;
                }
                default: {
                    this.result.PushColor(Color.Color8(194, 19, 19));
                    this.result.AppendText("No such command");
                    this.result.Pop();
                    break;
                }
            }
        } catch (IndexOutOfRangeException) {
            this.result.PushColor(Color.Color8(194, 19, 19));
            this.result.AppendText("Not enough arguments");
            this.result.Pop();
            this.command.Text = raw_command;
        } catch (Exception e) {
            this.result.PushColor(Color.Color8(194, 19, 19));
            this.result.AppendText(e.Message + "\n" + e.StackTrace + "\n");
            this.result.Pop();
        }
    }

    bool is_hovering = false;
    bool is_typing = false;
    public void result_hover(bool is_hover) {
        this.is_hovering = is_hover;
    }

    public void cmd_change(string _, bool is_hover) {
        Utils.text_focused = this.is_typing = is_hover;
    }

    public void set_editing(bool focused) {
        Utils.text_focused = focused;
    }
}
