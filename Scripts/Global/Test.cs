using Godot;
using System;

public partial class Test : Node {

    public static bool run = false;

    public override void _Ready() {
        run = true;
    }

    public static void LoadPlayerInv(Inventory inventory) {
        if (!run) {return;}
        Inventory.Load(inventory, ResourceLoader.Load<InventoryResource>("res://TestData/player_inventory.tres"));
    }

    public static void StartEnemyRespawn(BaseEnemy enemy) {
        if (!run) {return;}
        BaseEnemy _enemy = (BaseEnemy) enemy.Duplicate();
        Node parent = enemy.GetParent();
        Vector2 pos = enemy.respawnPoint;
        _enemy.health = 10;
        SceneTreeTimer t = parent.GetTree().CreateTimer(2, false, true);
        t.Timeout += () => {
            parent.AddChild(_enemy);
            _enemy.Owner = parent;
            _enemy.GlobalPosition = pos;
            _enemy.respawnPoint = pos;
        };
    }
}
