using Godot;
using System;
using System.IO;
using System.Threading.Tasks;

public partial class SaveSlot : PanelContainer {

    public int id = 0;
    public bool has_level {get; protected set;} = false;

    private TextureRect screenshot;
    private RichTextLabel label;
    private RichTextLabel time;
    private SaveScreen ui;

    public async void on_save() {
        var path = ProjectSettings.GlobalizePath("user://saves");
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        ui.Visible = false;
        var level = await Global.global.memsave();
        ui.Visible = true;
        setScreenshot(level.screenshot);
        time.Text = new DateTime(level.timestamp, DateTimeKind.Utc).ToLocalTime().ToString();
        level.write($"{path}/save{this.id}");
        this.has_level = true;
        GD.PrintS("Save");
    }

    public void on_load() {
        if (!has_level) {
            return;
        }
        ui.Call("on_close");
        Global.global.load(ResourceLoader.Load<Level>($"user://saves/save{this.id}.res", cacheMode: ResourceLoader.CacheMode.Ignore));
    }

    public void on_delete() {
        var path = $"{ProjectSettings.GlobalizePath("user://saves")}/save{this.id}";
        this.reset();
        if (!File.Exists(path + ".meta.tres")) {
            return;
        }
        File.Delete(path + ".meta.tres");
        File.Delete(path + ".res");
    }

    public void reset() {
        this.screenshot.Texture = null;
        this.has_level = false;
        this.time.Text = DateTime.UnixEpoch.ToString();
    }

    public void setId(int id) {
        this.id = id;
        this.label.Text = "Save " + id;
        if (!ResourceLoader.Exists($"user://saves/save{id}.meta.tres")) {
            this.reset();
            return;
        }

        this.has_level = true;
        LevelMeta level = ResourceLoader.Load<LevelMeta>($"user://saves/save{id}.meta.tres", cacheMode: ResourceLoader.CacheMode.Ignore);
        this.setScreenshot(level.screenshot);
        this.time.Text = new DateTime(level.timestamp, DateTimeKind.Utc).ToLocalTime().ToString();
        
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.screenshot = this.GetNode<TextureRect>("%Screenshot");
        this.label = this.GetNode<RichTextLabel>("%Label");
        this.time = this.GetNode<RichTextLabel>("%Time");
        this.time.Text = DateTime.UnixEpoch.ToString();
        ui = this.GetNode<SaveScreen>("../../../../..");
    }

    private void setScreenshot(byte[] data) {
        Image image = new Image();
        if (image.LoadWebpFromBuffer(data) != Error.Ok) {
            GD.PushError("Unable to load image");
            return;
        }

        this.screenshot.Texture = ImageTexture.CreateFromImage(image);
    }
}
