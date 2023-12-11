using Godot;
using System;

public partial class Global : Node {

    public override void _Notification(int what) {
        base._Notification(what);
        if (what == (int) NotificationWMCloseRequest) {
            GD.PrintS("Saving");
            Graphics.graphics.win_position = DisplayServer.WindowGetPositionWithDecorations();
            Graphics.save(Graphics.graphics);
        }
    }
}
