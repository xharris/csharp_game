using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

namespace Blanke
{
  public class ID {
    private static List<string> Names;
    private static List<int> Count;
    public int Id;
    public string Name;

    public static void Load(Engine engine)
    {
      UserData.RegisterType<ID>();
      UserData.RegisterType<ID.Signature>();
    }

    static ID()
    {
      Names = new List<string>();
      Count = new List<int>();
    }

    public ID(string name)
    {
      Name = name;
      Id = GetId(name);
    }

    // copy constructor
    private ID(ID other)
    {
      Name = other.Name;
      Id = other.Id;
    }

    public static int GetId(string name)
    {
      int idx = Names.IndexOf(name);
      if (idx == -1)
      {
        Names.Add(name);
        Count.Add(0);
        idx = Names.Count()-1;
      }
      return Count[idx]++;
    }

    public ID Copy()
    {
      return new ID(this);
    }

    public bool Equals(ID other)
    {
      return Name == other.Name && (other.Id == 0 || Id == 0 || Id == other.Id);
    }

    public bool Equals(string other)
    {
      return (Name+"#"+Id).StartsWith(other);
    }

    public override string ToString()
    {
      return Name+"#"+Id;
    }

    // SIGNATURE
    
    public class Signature {
      private List<string> IDList;
      private int Count;
      
      public Signature()
      {
        IDList = new List<string>();
      }

      public void Add(ID id)
      {
        int idx = IDList.IndexOf(id.Name);
        if (idx == -1)
        {
          IDList.Add(id.Name);
          Count = IDList.Count();
        }
      }

      public bool Remove(ID id)
      {
        return IDList.Remove(id.Name);
      }

      /// <summary>checks if other contains all ids of this signature</summary>
      public bool Validate(Signature other)
      {
        return IDList.Intersect(other.IDList).Count() == Count;
      }

      public int Size()
      {
        return Count;
      }

      public List<string>.Enumerator GetEnumerator()
      {
        return IDList.GetEnumerator();
      }

      public override string ToString()
      {
        return string.Join(",", IDList);
      }
    }
  }
}