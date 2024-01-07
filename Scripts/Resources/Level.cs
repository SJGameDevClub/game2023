using Godot;
using System;

[GlobalClass]
public partial class Level : Resource {

    [Export]
    public float player_health = 100f;

    [Export]
    public Inventory player_inventory = new(new() {null, null, null, null, null});
    
    [Export]
    public PackedScene saved_scene = new();

    [Export]
    public double time_scale = 1d;

    public long timestamp = DateTime.UtcNow.Ticks;

    public byte[] screenshot = Array.Empty<byte>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path">the path to the file without an extension</param>
    public void write(string path) {
        ResourceSaver.Save(this, path + ".res", ResourceSaver.SaverFlags.Compress);
        ResourceSaver.Save(new LevelMeta() {
            screenshot = screenshot,
            timestamp = timestamp
        }, path + ".meta.tres");
        
    }

}
