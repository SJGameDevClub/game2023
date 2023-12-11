using Godot;
using System;

public partial class KeybindResource : Resource {
    [Export]
    public Godot.Collections.Dictionary<string, Godot.Collections.Array<InputEvent>> vals = new();
}
