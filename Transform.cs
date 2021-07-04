using Microsoft.Xna.Framework;
using System.Linq;
using MoonSharp.Interpreter;

/*
     y
    |_x
    \z
*/

namespace Blanke
{
  public class Transform
  {
    public Vector3 Scale = Vector3.One;
    public Vector3 Rotation = Vector3.Zero;
    public Vector3 Translate = Vector3.Zero;
    private Vector3 Origin = Vector3.Zero;
    public Matrix World = Matrix.Identity;
    public Matrix Local = Matrix.Identity;

    // 2D shorthand
    public Vector2 Translate2D { get { 
      // Vector3 t3d = Translate3D;
      // return new Vector2(t3d.X, t3d.Y);
      return Vector2.Transform(Vector2.Zero, World); 
    } }
    public float x { get { return Translate.X; } set { Translate.X = value; } }
    public float y { get { return Translate.Y; } set { Translate.Y = value; } }
    public float ox { get { return Origin.X; } set { Origin.X = value; } }
    public float oy { get { return Origin.Y; } set { Origin.Y = value; } }
    public float r { get { return Rotation.Z; } set { this.Rotation.Z = value; } }
    public float sx { get { return Scale.X; } set { Scale.X = value; } }
    public float sy { get { return Scale.Y; } set { Scale.Y = value; } }

    // 3D shorthand
    public Vector3 Translate3D { get { return Vector3.Transform(Vector3.Zero, World); } }
    public float z { get { return Translate.Z; } set { Translate.Z = value; } }
    public float oz { get { return Origin.Z; } set { Origin.Z = value; } }
    public float rx { get { return Rotation.X; } set { Rotation.X = value; } }
    public float ry { get { return Rotation.Y; } set { Rotation.Y = value; } }
    public float rz { get { return Rotation.Z; } set { Rotation.Z = value; } }
    public float sz { get { return Scale.Z; } set { Scale.Z = value; } }

    public static void Load(Engine e)
    {
      UserData.RegisterType<Transform>();
      UserData.RegisterType<Vector2>();
      UserData.RegisterType<Vector3>();
      UserData.RegisterType<Matrix>();
    }

    private Matrix CreateRotation()
    {
      // return Matrix.CreateFromYawPitchRoll(Rotation.Z, Rotation.Y, Rotation.X)
      return Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationY(Rotation.Y) * Matrix.CreateRotationZ(Rotation.Z);
    }
    
    public void UpdateLocal()
    { 
      // SRT
      Local = Matrix.CreateTranslation(-Origin)
        * Matrix.CreateScale(Scale)
        * CreateRotation()
        * Matrix.CreateTranslation(Translate + Origin);
    }

    public void UpdateWorld(Transform parent)
      => World = Local * parent.World;

    public void Update()
    {
      UpdateLocal();
      World = Local;
    }
    public void Update(Transform parent)
    {
      UpdateLocal();
      UpdateWorld(parent);
    }

    private float Min(params float[] v) => v.Min();
    private float Max(params float[] v) => v.Max();

    public Rectangle TransformRectangle(float x, float y, float w, float h)
    {
      Vector2 tl = Vector2.Transform(Vector2.Zero, World);
      Vector2 tr = Vector2.Transform(new Vector2(w, 0), World);
      Vector2 bl = Vector2.Transform(new Vector2(0, h), World);
      Vector2 br = Vector2.Transform(new Vector2(w, h), World);

      Vector2 min = new Vector2(Min(tl.X, tr.X, bl.X, br.X), Min(tl.Y, tr.Y, bl.Y, br.Y));
      Vector2 max = new Vector2(Max(tl.X, tr.X, bl.X, br.X), Max(tl.Y, tr.Y, bl.Y, br.Y));

      return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
    }

    public Rectangle TransformRectangle(Rectangle rect) 
      => TransformRectangle(rect.X, rect.Y, rect.Width, rect.Height);

    public Vector2 GetTranslate2D()
      => Vector2.Transform(Vector2.Zero, World);
  }
}