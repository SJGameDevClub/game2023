
using Godot;
using System;

public static class Utils {

    private static int _ui_open = 0;
    public static int ui_open {get => _ui_open; set => _ui_open = Math.Max(0, value);}

    public static void ResetScene(this Node2D _this) {
        _this.GetTree().ReloadCurrentScene();
        PlayerInfo.Health = 100f;
    }

    public static void Quit(Node node) {
        node.GetTree().Root.PropagateNotification((int) Node.NotificationWMCloseRequest);
        Callable.From(() => {
            node.GetTree().Quit();
        }).CallDeferred();
    }
}
