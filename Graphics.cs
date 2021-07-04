using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MoonSharp.Interpreter;

using XnaColor = Microsoft.Xna.Framework.Color;
using CP = Blanke.ECS.ComponentProp;

namespace Blanke 

{
  public class Graphics 
  {
    public static ECS.ComponentTemplate Circle;

    public static void Load(Engine e)
    {
      // Circle
      Circle = new ECS.ComponentTemplate(e, "Circle",
        new CP(e, "line", Color.DefaultFg),
        new CP(e, "r", 1),
        new CP(e, "sides", 32),
        new CP(e, "fill", Color.none)
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
          e._sb.DrawCircle(ent.Transform.Translate2D, r, circle.Get<int>("sides"), fill, r, ent.Z);
        if (thickness > 0 && line != Color.none)
          e._sb.DrawCircle(ent.Transform.Translate2D, r, circle.Get<int>("sides"), line, thickness, ent.Z);
      };      

      UserData.RegisterType<Graphics>();
      e.MainScript.Globals["graphics"] = typeof(Graphics);
    }
  }
}