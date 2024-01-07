using Godot;
using System;

public partial class LevelMeta : Resource {

    public static double VERSION {get;} = 0.01;

    [Export]
    public long timestamp = DateTime.UtcNow.Ticks;

    [Export]
    public byte[] screenshot = Array.Empty<byte>();

    [Export]
    public double version {get; private set;} = VERSION; // TODO: Check Version in SaveSlot

}
