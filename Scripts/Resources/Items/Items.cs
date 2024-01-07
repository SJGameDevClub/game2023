
using Godot;

public static class Items {
    
    public readonly static Item Cherry = getItem("res://Items/Cherry.tres");
    public readonly static Item GreenGem = getItem("res://Items/GreenGem.tres");
    public readonly static Item BlueGem = getItem("res://Items/BlueGem.tres");

    private static Item getItem(string path) {
        return ResourceLoader.Load<Item>(path);
    }
}
