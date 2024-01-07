using Godot;
using System;
using System.Collections.Generic;

public partial class SaveScreen : CanvasLayer {

    private PackedScene _save_slot = ResourceLoader.Load<PackedScene>("res://Nodes/UI/Components/Save Slot.tscn");
    private static PackedScene _save_menu = ResourceLoader.Load<PackedScene>("res://Nodes/UI/Save Screen.tscn");

    private GridContainer container;
    private RichTextLabel page;
    private List<SaveSlot> slots = new();
    private CanvasLayer prev_ui = null;

    private int cur_page = 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.container = this.GetNode<GridContainer>("%SlotContainer");
        this.page = this.GetNode<RichTextLabel>("%Page");
        for (int i = 0; i < 10; ++i) {
            var slot = this._save_slot.Instantiate<SaveSlot>();
            this.container.AddChild(slot);
            slot.Owner = this.container;
            slot.setId(i);
            slots.Add(slot);
        }
    }

    public override void _UnhandledInput(InputEvent @event) {
        if (@event is not InputEventKey key || !key.Pressed || key.Keycode != Key.Escape) {
            return;
        }

        on_close();
    }

    public void on_close() {
        this.Visible = false;
        Utils.ui_open--;
        if (this.prev_ui != null) {
            this.prev_ui.Visible = true;
            this.prev_ui = null;
        }
        this.QueueFree();
    }

    public void setPage(int page) {
        if (page <= 0) {
            return;
        }
        this.page.Text = "" + page;
        this.cur_page = page;
        for (int i = 0; i < this.slots.Count; ++i) {
            this.slots[i].setId(i + (this.slots.Count * (page - 1)));
        }
    }

    public void pageLeft() {
        this.setPage(cur_page-1);
    }

    public void pageRight(){
        this.setPage(cur_page+1);
    }

    public static void openSaveMenu(CanvasLayer item = null) {
        SaveScreen screen = _save_menu.Instantiate<SaveScreen>();
        item.GetTree().Root.AddChild(screen);
        screen.Owner = item.GetTree().Root;
        screen.prev_ui = item;
        if (item != null) {
            item.Visible = false;
        }
        ++Utils.ui_open;
    }
}
