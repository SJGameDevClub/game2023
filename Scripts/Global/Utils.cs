
using Godot;

public static class Utils {
    public static void ResetScene(this Node2D _this) {
        _this.GetTree().ReloadCurrentScene();
        PlayerInfo.Health = 100f;
    }
}
