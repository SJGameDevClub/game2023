using Godot;
using System;

public partial class Test : Node {

    public static bool run = false;

    public override void _Ready() {
        run = true;
    }

    public static void StartEnemyRespawn(BaseEnemy enemy) {
        if (!run) {return;}
        BaseEnemy _enemy = ResourceLoader.Load<PackedScene>(enemy.SceneFilePath).Instantiate<BaseEnemy>();
        Node parent = enemy.GetParent();
        Vector2 pos = enemy.respawn_point;
        _enemy.health = _enemy.max_health;
        SceneTreeTimer t = parent.GetTree().CreateTimer(2, false, true);
        t.Timeout += () => {
            parent.AddChild(_enemy);
            _enemy.Owner = parent;
            _enemy.GlobalPosition = pos;
            _enemy.respawn_point = pos;
        };
    }
}
