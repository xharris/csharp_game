using System.Xml;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using DnetColor = System.Drawing.Color;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Blanke
{
  [MoonSharpUserData]
  public class ShadeList
  {
    private SortedList<int, XnaColor> Shades;

    public ShadeList()
    {
      Shades = new SortedList<int, XnaColor>();
    }

    public void Add(int shade, XnaColor color)
    {
      if (!Shades.ContainsKey(shade))
        Shades.Add(shade, color);
      else 
        Shades[shade] = color;
    }

    public XnaColor this[int shade]
    {
      get { return Shades[shade]; }
    }
    
    public static implicit operator XnaColor(ShadeList s) => s.Shades[s.Shades.Keys[s.Shades.Keys.Count/2]];
  }

  [MoonSharpUserData]
  public class Color
  {
    private static Dictionary<string, ShadeList> Colors;
    public static XnaColor DefaultFg;
    public static XnaColor DefaultBg;
    public static XnaColor none;

    static Color()
    {
      Colors = new Dictionary<string, ShadeList>();
    }

    public static void Load(Engine engine)
    {
      // Preload some colors
      XmlDocument doc = new XmlDocument();
      doc.Load("colors.xml");
      string failed = "";
      foreach (XmlNode node in doc.DocumentElement?.ChildNodes)
      {
        string[] name_parts = node.Attributes?["name"]?.Value.Split("_") ?? null;
        if (name_parts != null)
        {
          try {
            string color = string.Join("_", new System.ArraySegment<string>(name_parts, 1, name_parts.Length-2));
            int shade = int.Parse(name_parts[name_parts.Length-1]);
            Store(
              node.InnerText, 
              color, 
              shade
            );
          }
          catch (System.FormatException) {
            failed += node.Attributes?["name"]?.Value+" ";
          }
        }
      }  
      if (failed.Length > 0)
        Log.Warn($"Could not parse colors: {failed}");

      Store("#00000000", "none", 0);

      DefaultFg = Color.Get("black");
      DefaultBg = Color.Get("white");
      none = Color.Get("none");

      UserData.RegisterType<Color>();
      UserData.RegisterType<XnaColor>();
      engine.MainScript.Globals["color"] = (System.Func<string, int, XnaColor>)Get;
    }
    
    public static XnaColor Get(string name, int shade = -1)
    {
      if (shade < 0)
        return Colors[name];
      return Colors[name][shade];
    }

    private static void Store(DnetColor clr, string name, int shade)
    {
      XnaColor final_clr = new XnaColor(clr.R, clr.G, clr.B, clr.A);
      if (!Colors.ContainsKey(name))
        Colors.Add(name, new ShadeList());
      Colors[name].Add(shade, final_clr);
    }

    public static void Store(string hex, string name, int shade)
    {
      hex = hex.Replace("#", "");
      if (hex.Length <= 6)
        hex = "FF"+hex;
      int argb = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
      Store(DnetColor.FromArgb(argb), name, shade);
    }

    public ShadeList this[string color]
    {
      get { return Colors[color]; }
    }
  }
}