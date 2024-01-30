
using Godot;
using System;

public static class Utils {

    private static int _ui_open = 0;
    public static int ui_open {get => _ui_open; set => _ui_open = Math.Max(0, value);}
    public static bool text_focused = false;

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

    public static void EnterArea(Player player, Door door) {
        SceneTree tree = player.GetTree();
        PackedScene prev_scene = new();
        player.Owner = null;
        player.GetParent().RemoveChild(player);
        prev_scene.Pack(tree.CurrentScene);
        PackedScene new_scene = door.scene;
        Vector2 new_pos;
        Player dplayer = (Player) player.Duplicate((int) (Node.DuplicateFlags.Scripts | Node.DuplicateFlags.Signals | Node.DuplicateFlags.Groups));
        if (new_scene == null) {
            player.prev_door_scene = prev_scene;
            var prev_info = player.prev_scenes[player.prev_scenes.Count - 1];
            new_scene = prev_info.scene;
            new_pos = prev_info.pos;
            player.prev_scenes.RemoveAt(player.prev_scenes.Count - 1);
        } else {
            new_pos = Vector2.Zero;
            player.prev_scenes.Add((prev_scene, player.GlobalPosition));
        }
        tree.ChangeSceneToPacked(new_scene);
        Action enter = () => {};
        enter = () => {
            tree.CurrentScene.AddChild(player);
            player.Owner = tree.CurrentScene;
            player.GlobalPosition = new_pos;
            RenderingServer.FramePostDraw -= enter;
        };
        RenderingServer.FramePostDraw += enter;
        
    }
}
