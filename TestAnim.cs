using Godot;
using System;

public partial class TestAnim : TextureRect {

	[Export]
	public SpriteFrames anim;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	double index = 0;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		int count = anim.GetFrameCount("idle");
		if (index >= count) {
			index = 0;
		}
		this.Texture = anim.GetFrameTexture("idle", (int) Math.Floor(index));
		index += anim.GetAnimationSpeed("idle") * delta;
		GD.PrintS(this.Rotation);
	}
}
