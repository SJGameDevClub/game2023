
using Godot;

public static class Utils {

    public static int ui_open = 0;
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
