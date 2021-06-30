//using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using Guid = System.Guid;

namespace Blanke
{
  public class Engine : Game
  {    
    [System.STAThread]
    static void Main()
    {
      (new Engine()).Run();
    }

    public ECS Ecs;
    public static GraphicsDevice _gd;
    public static GraphicsDeviceManager _gdm;
    public static ContentManager _cm;
    public static Dictionary<string, Texture2D> _textures;
    SpriteBatch _sb;

    static Engine()
    {
      _textures = new Dictionary<string, Texture2D>();
    }

    public Engine()
    {
      Ecs = new ECS();

      ECS.ComponentTemplate Text = new ECS.ComponentTemplate(Ecs, "Text", 
        new ECS.ComponentPropGeneric<string>("value", "placeholder")
      );
      ECS.System TextSystem = new ECS.System(Ecs, Text);
      TextSystem.UpdateFn = delegate(ECS.Entity e, float dt, ECS.Component[] components)
      {
        ECS.Component text = components[0];
        System.Console.WriteLine("text:"+text.Get<string>("value")+" dt:"+dt.ToString());
      };

      ECS.Entity bob = new ECS.Entity(Ecs).Add(Text.Create().Set<string>("value", "wowie"));
      

      Engine._gdm = Engine._gdm ?? new GraphicsDeviceManager(this);
      Engine._cm = this.Content;
      Engine._gd = Engine._gd ?? this.GraphicsDevice;
      Content.RootDirectory = "Content";
      IsMouseVisible = true;
    }

    protected override void Update(GameTime gt)
    {
      Ecs.UpdateAll(gt);
      base.Update(gt);
    }

    protected override void Draw(GameTime gt)
    {
      _sb = _sb ?? new SpriteBatch(this.GraphicsDevice);
      Clear(Color.White);

      Ecs.DrawAll();
  
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

    public Texture2D AddTexture(string name)
    {
      _textures = _textures ?? new Dictionary<string, Texture2D>();
      if (!_textures.ContainsKey(name))
        _textures.Add(name, this.Content.Load<Texture2D>(name));
      return _textures[name];
    }

    public static Texture2D GetTexture(string name)
    {
      return _cm.Load<Texture2D>(name);// (Texture2D)_textures[name];
    }

    public static Guid NewGuid()
    {
      return Guid.NewGuid();
    }

    public void Clear(Color color)
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