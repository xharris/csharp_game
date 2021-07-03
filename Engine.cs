//using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using MoonSharp.Interpreter;
using shortid;

using Math = System.Math;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Blanke
{
  public class Engine : Game
  {    
    [System.STAThread]
    static void Main(string[] args)
    {
      (new Engine(args)).Run();
    }

    public ECS Ecs;
    public Script MainScript;
    public XnaColor BackgroundColor;

    public GraphicsDeviceManager GDManager;
    public static Dictionary<string, Texture2D> _textures;
    public SpriteBatch _sb;

    static Engine()
    {
      _textures = new Dictionary<string, Texture2D>();
    }

    public Engine(string[] args)
    {
      MainScript = new Script();
      Ecs = new ECS();

      MoonSharpExtension.Load(this);
      Color.Load(this);
      ECS.Load(this);
      GameSetting.Load(this);
      ID.Load(this);
      Graphics.Load(this);

      // ECS.ComponentTemplate Text = new ECS.ComponentTemplate(Ecs, "Text", 
      //   new ECS.ComponentPropGeneric<string>("value", "placeholder")
      // );
      // ECS.System TextSystem = new ECS.System(Ecs, Text);
      // TextSystem.UpdateFn = delegate(ECS.Entity e, float dt, ECS.Component[] components)
      // {
      //   ECS.Component text = components[0];
      // };

      // ECS.Entity bob = new ECS.Entity(Ecs).Add(Text.Create().Set<string>("value", "wowie"));
    
      GDManager = new GraphicsDeviceManager((Game)this);
      Content.RootDirectory = "Content";
      IsMouseVisible = true;

      if (args.Length > 0 && File.Exists(args[0]))
      {
        // Stream sys_out = System.Console.OpenStandardOutput();
        // main_script.Options.DebugPrint = s => { Log.Debug(s); };
        // main_script.Options.Stderr = sys_out;
        MainScript.Globals["window"] = (System.Action<Dictionary<string, DynValue>>)ConfigWindow;
        MainScript.Globals["background"] = (System.Action<XnaColor>)SetBackgroundColor;

        try
        {
          DynValue res = MainScript.DoFile(args[0]);
        }
        catch (ScriptRuntimeException ex)
        {
          Log.Error(ex.DecoratedMessage);
        }
      }
    }

    protected override void Update(GameTime gt)
    {
      Ecs.UpdateAll(gt);
      base.Update(gt);
    }

    protected override void Draw(GameTime gt)
    {
      _sb = _sb ?? new SpriteBatch(this.GraphicsDevice);
      Clear(BackgroundColor);

      _sb.Begin();
      Ecs.DrawAll();
      _sb.End();
  
      // primitive = new Primitive(this.GraphicsDevice);
      // MouseState mouse = Mouse.GetState();
      // primitive.SetColor(Color.Red);
      // primitive.Begin();
      // primitive.Transform.Position = Blanke.Window.Size * 1 / 4;
      // primitive.Line(Blanke.Window.Size * 3 / 4);
      // primitive.Transform.Position = Blanke.Window.Size * 2 / 4;
      // primitive.Rect(new Vector2(30, 30));
      // primitive.Poly(
      //   new Vector2(Blanke.Window.Width / 2, Blanke.Window.Height / 3),
      //   Blanke.Window.Size * 3 / 4,
      //   new Vector2(Blanke.Window.Width / 4, Blanke.Window.Height * 3 / 4)
      // );
      // primitive.End();

      base.Draw(gt);
    }

    protected override void LoadContent()
    {
      AddTexture("bluerobot");
      base.LoadContent();
    }

    public void ConfigWindow(Dictionary<string, DynValue> opt)
    {
      // title
      if (opt.ContainsKey("title"))
      {
        Window.Title = opt["title"].CastToString();
      }
      // size
      bool size_changed = false;
      if (opt.ContainsKey("width"))
      {
        GDManager.PreferredBackBufferWidth = (int)opt["width"].CastToNumber();
        size_changed = true;
      }
      if (opt.ContainsKey("height"))
      {
        GDManager.PreferredBackBufferHeight = (int)opt["height"].CastToNumber();
        size_changed = true;
      }
      if (size_changed)
      {
        GDManager.ApplyChanges();
      }
      // position
      if (opt.ContainsKey("x") || opt.ContainsKey("y"))
      {
        Point p = new Point();
        if (opt.ContainsKey("x"))
          p.X = (int)opt["x"].CastToNumber();
        if (opt.ContainsKey("y"))
          p.Y = (int)opt["y"].CastToNumber();
        Window.Position = p;
      }
    }

    public void SetBackgroundColor(XnaColor c)
    {
      BackgroundColor = c;
    }

    public Texture2D AddTexture(string name)
    {
      _textures = _textures ?? new Dictionary<string, Texture2D>();
      if (!_textures.ContainsKey(name))
        _textures.Add(name, this.Content.Load<Texture2D>(name));
      return _textures[name];
    }

    public static shortid.Configuration.GenerationOptions shortid_options = new shortid.Configuration.GenerationOptions
    {
      UseNumbers = true,
      UseSpecialCharacters = false
    };
    public static string newGuid()
    {
      return ShortId.Generate(shortid_options);
    }

    public static Texture2D GetTexture(Engine engine, string name)
    {
      return engine.Content.Load<Texture2D>(name);// (Texture2D)_textures[name];
    }

    public void Clear(XnaColor color)
    {
      this.GraphicsDevice.Clear(color);
    }

    public static Vector2 RoundVector(Vector2 vector)
    {
      return new Vector2(
          (float)Math.Floor((decimal)(vector.X + 0.5)),
          (float)Math.Floor((decimal)(vector.Y + 0.5))
        );
    }
  }

  // Prebuilt Components
  // public class Transform : ComponentTemplate
  // {
  //   public Vector2 Position = Vector2.Zero;
  //   public float Scale = 1;
  //   public float Angle = 0;
  // }

  // public class DrawProps : Component
  // {
  //   public XnaRectangle? Crop;
  //   public Color Color = Color.White;
  //   public Vector2 Origin;
  //   public float Z;
  //   public DrawProps() : base() { }
  //   public void Align(string name, string alignment)
  //   {
  //     Texture2D texture = Engine.GetTexture(name);
  //     int X = 0;
  //     int Y = 0;
  //     if (texture != null)
  //     {
  //       if (alignment.Contains("center"))
  //       {
  //         X = texture.Width / 2;
  //         Y = texture.Height / 2;
  //       }
  //       if (alignment.Contains("left"))
  //         X = 0;
  //       if (alignment.Contains("right"))
  //         X = texture.Width;
  //       if (alignment.Contains("top"))
  //         Y = 0;
  //       if (alignment.Contains("bottom"))
  //         Y = texture.Height;
  //     }
  //     this.Origin.X = X;
  //     this.Origin.Y = Y;
  //   }
  // }
}