namespace Blanke
{
  public enum LogLevel {
    DEBUG,
    WARN,
    INFO,
    ERROR,
    NONE
  }

  public static class Log
  {
    public static LogLevel Level = LogLevel.DEBUG;
    
    public static void Debug(string text, params object[] objs)
    {
      if (Level <= LogLevel.DEBUG)
        System.Console.WriteLine("[DEBUG] {0}", text, objs);
    }
    public static void Info(string text, params object[] objs)
    {
      if (Level <= LogLevel.INFO)
        System.Console.WriteLine("[INFO] {0}", text, objs);
    }
    public static void Warn(string text, params object[] objs)
    {
      if (Level <= LogLevel.WARN)
        System.Console.WriteLine("[WARN] {0}", text, objs);
    }
    public static void Error(string text, params object[] objs)
    {
      if (Level <= LogLevel.ERROR)
        System.Console.WriteLine("[ERROR] {0}", text, objs);
    }
  }
}