using Godot;
using System;

public partial class ConfirmationBox : PanelContainer {

    [Signal]
    public delegate void on_selectEventHandler(bool confirm);

    public string _title = "";
    public RichTextLabel title;
    public RichTextLabel label;

    public override void _Ready() {
        this.title = this.GetNode<RichTextLabel>("%Title");
        this.label = this.GetNode<RichTextLabel>("%Text");
    }

    public override void _Process(double delta) {
        base._Process(delta);
        if (timer != null) {
            this.title.Text = $"{this._title} ({Math.Round(timer.TimeLeft, 1)})";
        }
    }

    public void selected(bool confirmed) {
        EmitSignal("on_select", confirmed);
        this.QueueFree();
        timer.Dispose();
        timer = null;
    }

    SceneTreeTimer timer;
    public void setTimeout(double seconds = 15) {
        if (timer != null) {
            GD.PushWarning("There is already a timer on this box");
            return;
        }
        timer = this.GetTree().CreateTimer(seconds, true, false, true);
        timer.Timeout += () => {
            selected(false);
        };
    }

    public void setText(string title, string text) {
        this._title = title;
        this.title.Text = title;
        this.label.Text = text;
    }
}
