using System.Collections.Generic;
using System.Linq;

namespace Blanke
{
  public class ID {
    private static List<string> Names;
    public int Id;

    static ID()
    {
      Names = new List<string>();
    }

    public ID(string name)
    {
      Id = GetId(name);
    }

    private ID(int id)
    {
      Id = id;
    }

    public static int GetId(string name)
    {
      int idx = Names.IndexOf(name);
      if (idx != -1)
        return idx;
      else 
      {
        Names.Add(name);
        return Names.Count() - 1;
      }
    }

    public ID Copy()
    {
      return new ID(Id);
    }

    public bool Equals(ID other)
    {
      return Id == other.Id;
    }

    // SIGNATURE
    
    public class Signature {
      private List<int> IDList;
      private int Count;
      
      public Signature()
      {
        IDList = new List<int>();
      }

      public void Add(ID id)
      {
        int idx = IDList.IndexOf(id.Id);
        if (idx == -1)
        {
          IDList.Add(id.Id);
          Count = IDList.Count();
        }
      }

      public bool Remove(ID id)
      {
        return IDList.Remove(id.Id);
      }

      /// <summary>checks if other contains all ids of this signature</summary>
      public bool Validate(Signature other)
      {
        return IDList.Union(other.IDList).Count() == Count;
      }

      public int Size()
      {
        return Count;
      }

      public List<int>.Enumerator GetEnumerator()
      {
        return IDList.GetEnumerator();
      }
    }
  }
}