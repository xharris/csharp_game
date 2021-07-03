using MonoGame;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MoonSharp.Interpreter;

using XnaColor = Microsoft.Xna.Framework.Color;

namespace Blanke 

{
  public class Graphics 
  {
    public static ECS.ComponentTemplate Circle;

    public static void Load(Engine e)
    {
      // Circle
      Circle = new ECS.ComponentTemplate(e.Ecs, "Circle",
        new ECS.ComponentProp(e, "line", Color.DefaultFg),
        new ECS.ComponentProp(e, "r", 1),
        new ECS.ComponentProp(e, "sides", 32),
        new ECS.ComponentProp(e, "fill", Color.none)
      );
      ECS.System CircleSystem = new ECS.System(e.Ecs, Circle);
      CircleSystem.DrawFn = delegate(ECS.Entity ent, ECS.Component[] components)
      {
        ECS.Component circle = ent.Get(Circle);
        float r = circle.Get<float>("r");
        XnaColor fill = circle.Get<XnaColor>("fill");
        XnaColor line = circle.Get<XnaColor>("line");
        float thickness = circle.Get<float>("thickness");
        
        if (fill != Color.none)
          e._sb.DrawCircle(new Vector2(0, 0), r, circle.Get<int>("sides"), fill, r);
        if (thickness > 0 && line != Color.none)
          e._sb.DrawCircle(new Vector2(0, 0), r, circle.Get<int>("sides"), line, thickness);
      };      

      UserData.RegisterType<Graphics>();
      e.MainScript.Globals["graphics"] = typeof(Graphics);
    }
  }
}