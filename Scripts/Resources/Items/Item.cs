using Godot;
using System;

[Tool]
[GlobalClass]
public partial class Item : Resource {
    public static string id_base {get;} = "item.";
    public static int BASE_MAX_STACK_SIZE {get;} = 15;

    [ExportGroup("Identifiers")]
    [Export]
    public string name = "unknown";

    [Export(PropertyHint.MultilineText)]
    public string description = "unknown";

    protected string _id = "";

    [ExportGroup("Other")]
    [Export]
    public int max_stack_size = BASE_MAX_STACK_SIZE;

    public bool stackable => max_stack_size > 1;

    [Export(hintString: "id")]
    public string id {
        get => string.IsNullOrEmpty(this._id) ? id_base + this.name.ToLower().Replace(' ', '_') : _id;
        set => this._id = value;
    }

    [Export]
    public SpriteFrames frames = ResourceLoader.Load<SpriteFrames>("res://Assets/missing_frames.tres");

    private Item() {}

}
