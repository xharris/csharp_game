using MoonSharp.Interpreter;

namespace Blanke 
{
  public class GameSetting
  {
    private Engine engine;
    public int width
    {
      get { return engine.GDManager.PreferredBackBufferWidth; }
    }
    public int height
    {
      get { return engine.GDManager.PreferredBackBufferHeight; }
    }

    public static void Load(Engine engine)
    {
      UserData.RegisterType<GameSetting>();

      engine.MainScript.Globals["game"] = new GameSetting(engine);
    }

    public GameSetting(Engine engine)
    {
      this.engine = engine;
    }
  }
}