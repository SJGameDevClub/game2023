using Godot;
using System;

public partial class Settings : MarginContainer {
    protected static Settings settings;
    private static int last_tab = 0;
    
    private CanvasItem previous_item;
    private CanvasLayer previous_ui;
    private TabContainer tab_container;
    private Button save_button;
    private Button cancel_button;
    private int delays = 0;
    protected bool cancelled = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        settings = this;
        this.Visible = false;
        this.tab_container = this.GetNode<TabContainer>("%Tabs");
        this.save_button = this.GetNode<Button>("%Save");
        this.cancel_button = this.GetNode<Button>("%Cancel");
        this.tab_container.TabChanged += (id) => {
            last_tab = (int) id;
        };
        this.save_button.Pressed += () => {
            // this.GetTree().CreateTimer(3).Timeout += () => {
            //     this.Visible = true;
            // };

            this.PropagateCall("onSave", new() {Callable.From(delay)});
            if (!this.cancelled) {
                close();
            }
            this.cancelled = false;
        };
        // TODO: Confirm if modified
        this.cancel_button.Pressed += () => {
            this.PropagateCall("onCancel", new() {Callable.From(delay)});
            if (!this.cancelled) {
                close();
            }
            this.cancelled = false;
        };
        this.tab_container.CurrentTab = last_tab;
    }

    public override void _GuiInput(InputEvent @event) {
        if (@event is not InputEventKey key || !key.Pressed || key.PhysicalKeycode != Key.Escape) {
            return;
        }

        this.AcceptEvent();
        close();
        this.PropagateCall("onCancel");
    }

    private void close() {
        if (this.delays > 0) {
            return;
        }
        this.Visible = false;
        if (previous_ui != null) {
            previous_ui.Visible = true;
            previous_ui = null;
        } else if (previous_item != null) {
            previous_item.Visible = true;
            previous_item = null;
        }
        --Utils.ui_open;
    }

    public static void openSettings(CanvasItem item = null) {
        settings.Visible = true;
        settings.previous_item = item;
        item.Visible = false;
        ++Utils.ui_open;
    }

    public static void openSettings(CanvasLayer layer = null) {
        settings.Visible = true;
        settings.previous_ui = layer;
        layer.Visible = false;
        ++Utils.ui_open;
    }

    protected Callable delay() {
        ++this.delays;
        Action<bool> on_complete = onComplete;
        return Callable.From(on_complete);
    }

    protected void onComplete(bool cancel) {
        --this.delays;
        if (!cancel || cancelled) {
            close();
        } else {
            this.cancelled = cancel || this.cancelled;
        }
    }

    private Settings() {}

}
