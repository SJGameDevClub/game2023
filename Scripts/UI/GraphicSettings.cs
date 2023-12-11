using Godot;
using System;
using System.Linq;

public partial class GraphicSettings : MarginContainer {
    
    protected Graphics graphics;
    protected OptionButton win_mode_selector;
    protected OptionButton resolution_selector;
    protected OptionButton vsync_selector;
    private static PackedScene confirm_box {get;} = ResourceLoader.Load<PackedScene>("res://Nodes/UI/Components/Confirm Box.tscn");

    protected static Vector2I[] resolutions {get;} = new Vector2I[4] {new (640, 360), new(854, 480), new(1280, 720), new(1920, 1080)};

    protected int changes = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.graphics = (Graphics) Graphics.graphics.Duplicate();
        this.win_mode_selector = this.GetNode<OptionButton>("%Win Mode");
        this.resolution_selector = this.GetNode<OptionButton>("%Resolution Mode");
        this.vsync_selector = this.GetNode<OptionButton>("%VSync Mode");
        reset();
    }

    public void windowModeSelected(int index) {
        ConfirmationBox box = confirm_box.Instantiate<ConfirmationBox>();
        Callable.From(() => {
            this.AddChild(box);
            box.Owner = this;
            MoveChild(box, -1);
            box.setText("Graphics", "Would you like stay in this mode");
            box.setTimeout();
        }).CallDeferred();

        var old = Graphics.graphics.window_mode;
        Graphics.graphics.window_mode = (Graphics.WindowMode) index;
        Graphics.setDisplayMode(this.graphics.window_mode);
        box.on_select += (confirmed) => {
            if (!confirmed) {
                Graphics.graphics.window_mode = old;
                Graphics.setDisplayMode(Graphics.graphics.window_mode);
                this.win_mode_selector.Selected = (int) old;
            }
        };
    }

    public void resolutionSelected(int index) {
        this.graphics.display_size = resolutions[index];
    }

    public void vsyncSelected(int index) {
        this.graphics.vSync = (DisplayServer.VSyncMode) index;
    }

    public void onSave(Callable delay) {
        if (this.graphics == Graphics.graphics) {
            return;
        }
        ConfirmationBox box = confirm_box.Instantiate<ConfirmationBox>();
        Callable.From(() => {
            Node parent = this.GetParent().GetParent();
            parent.AddChild(box);
            box.Owner = parent;
            parent.MoveChild(box, -1);
            box.setText("Graphics", "Would you like to keep these settings");
            box.setTimeout();
        }).CallDeferred();

        Callable complete = delay.Call().As<Callable>();
        var old = (Graphics) Graphics.graphics.Duplicate();
        box.on_select += (confirmed) => {
            if (!confirmed) {
                this.graphics = old;
                Graphics.setGraphics((Graphics) this.graphics.Duplicate());
                reset();
            } else {
                Graphics.save(this.graphics);
            }

            complete.Call(!confirmed);
        };
        Graphics.setGraphics((Graphics) this.graphics.Duplicate());
    }

    public void onCancel(Callable _delay) {
        this.graphics = (Graphics) Graphics.graphics.Duplicate();
        reset();
    }

    public void reset() {
        this.win_mode_selector.Selected = (int) this.graphics.window_mode;
        this.resolution_selector.Selected = resolutions.ToList().IndexOf(this.graphics.display_size);
        this.vsync_selector.Selected = (int) this.graphics.vSync;
    }

}
