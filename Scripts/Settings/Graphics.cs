using Godot;
using System;
using System.IO;

//TODO: Add AA Options
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public partial class Graphics : Resource {
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public static Graphics graphics {get; protected set;}

    private static Vector2I defaultSize = new Vector2I(1920, 1080);

    public enum WindowMode {
        Windowed = 0,
        WindowedBorderless,
        Fullscreen,
        FullscreenBorderless
    }

    [Export]
    public DisplayServer.VSyncMode vSync = DisplayServer.VSyncMode.Disabled;

    [Export]
    public WindowMode window_mode = WindowMode.Windowed;

    [Export]
    public Vector2I display_size = defaultSize;

    [Export]
    public Vector2I last_win_size {
        get => _last_win_size == Vector2I.Zero ? this.display_size : _last_win_size;
        set => _last_win_size = value;
    }
    private Vector2I _last_win_size;
    
    [Export]
    private Vector2I _win_position;
    public Vector2I win_position {
        get => _win_position;
        set {
            this._win_position = value;
            defaultPos = false;
        }
    }

    [Export]
    private bool defaultPos = true;

    public static Graphics load(bool use = true) {
        if (Engine.IsEditorHint()) {
            return new Graphics();
        }
        
        Graphics _graphics;
        
        if (ResourceLoader.Exists("user://settings/graphics.tres")) {
            _graphics = ResourceLoader.Load<Graphics>("user://settings/graphics.tres");
        } else {
            _graphics = new Graphics();
        }
        
        if (use) {
            setGraphics(_graphics);
            DisplayServer.WindowSetSize(_graphics.last_win_size);
        }

        return _graphics;
    }

    public static void save(Graphics graphics) {
        string dir = ProjectSettings.GlobalizePath("user://settings");
        if (!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }
        // ProjectSettings.Singleton.SetSetting()
        ResourceSaver.Save(graphics, "user://settings/graphics.tres");
    }

    public static void setDisplayMode(WindowMode mode) {
        switch (mode) {
            case WindowMode.Windowed: {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
                break;
            }
            case WindowMode.WindowedBorderless: {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, true);
                break;
            }
            case WindowMode.Fullscreen: {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
                break;
            }
            case WindowMode.FullscreenBorderless: {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, true);
                break;
            }
        }
    }

    public static void setGraphics(Graphics graphics) {
        DisplayServer.WindowSetSize(graphics.display_size);
        DisplayServer.WindowSetVsyncMode(graphics.vSync);
        /* GD.PrintS(
            graphics.defaultPos,
            "\n",
            DisplayServer.WindowGetSize() <= graphics.display_size,
            "\n",
            graphics.display_size,
            "\n",
            DisplayServer.WindowGetSize(),
            "\n",
            DisplayServer.WindowGetSizeWithDecorations(),
            "\n",
            DisplayServer.ScreenGetSize() / 2 - ((DisplayServer.ScreenGetSize() <= graphics.display_size ? DisplayServer.WindowGetSize() : DisplayServer.WindowGetSizeWithDecorations()) / 2),
            "\n",
            DisplayServer.ScreenGetSize() / 2,
            "\n",
            ( DisplayServer.ScreenGetSize() <= graphics.display_size ? DisplayServer.WindowGetSize() : DisplayServer.WindowGetSizeWithDecorations()) / 2
        ); */
        DisplayServer.WindowSetPosition(graphics.win_position);
        if (graphics.defaultPos || DisplayServer.WindowGetPosition() <= Vector2I.Zero) {
            graphics._win_position = ( DisplayServer.ScreenGetSize() / 2 ) - ( (DisplayServer.ScreenGetSize() <= graphics.display_size ? DisplayServer.WindowGetSize() : DisplayServer.WindowGetSizeWithDecorations()) / 2 );
            DisplayServer.WindowSetPosition(graphics.win_position);
        }
        setDisplayMode(graphics.window_mode);
        Graphics.graphics = graphics;
    }

    public static bool operator ==(Graphics a, Graphics b) {
        if (ReferenceEquals(a, b)) {
            return true;
        }
        
        if ((a is null && b is not null) || (a is not null && b is null)) {
            return false;
        }
        
        if (a is null && b is null) {
            return true;
        }
        return (a.display_size == b.display_size) && (a.vSync == b.vSync) && ( a.window_mode == b.window_mode );
    }

    public static bool operator !=(Graphics a, Graphics b) {
        if ((a is null && b is not null) || (b is null && a is not null)) {
            return false;
        }
        if (a is null) {
            return true;
        }
        return ( a.display_size != b.display_size ) && ( a.vSync != b.vSync ) && (a.window_mode != b.window_mode);
    }

    static Graphics() {
        load();
    }

    public override bool Equals(object obj) {
        if (obj is not Graphics _graphics) {
            return false;
        }

        return this == _graphics;
    }

}
