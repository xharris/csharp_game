using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using System;

namespace Blanke
{
  public class LuaIndexableObjectDataDescriptor  : StandardUserDataDescriptor
  {
      public LuaIndexableObjectDataDescriptor  (Type type, InteropAccessMode accessMode, string friendlyName = null) : base(type, accessMode, friendlyName)
      {
      }

      public override DynValue Index(Script script, object obj, DynValue index, bool isDirectIndexing)
      {
          // First attempt indexing in the normal way
          DynValue v = base.Index(script, obj, index, isDirectIndexing);

          // If the item wasn't found, attempt to look up the index in the indexable object's dictionary
          Log.Debug($"type is {obj.GetType()}");
          if((v == null) && (obj is LuaIndexableObject))
          {
              v = ((LuaIndexableObject)obj)[index];
          }

          return v;
      }

      public override bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isDirectIndexing)
      {
          bool b = base.SetIndex(script, obj, index, value, isDirectIndexing);

          // If the item wasn't found, set the item in the indexable object's dictionary
          Log.Debug($"type is {obj.GetType()}");
          if((!b) && (obj is LuaIndexableObject))
          {
              ((LuaIndexableObject)obj)[index] = value;
              b = true;
          }

          return b;
      }
  }

  public static class MoonSharpExtension
  {
    
    public static void Load(Engine engine)
    {
      
    }
  }

  public class LuaIndexableObject
  {
    public delegate object DIndex(MoonSharp.Interpreter.ScriptExecutionContext a, MoonSharp.Interpreter.CallbackArguments b, object c);
    public static void RegisterType<T>()
    {
      var descriptor = (StandardUserDataDescriptor)UserData.RegisterType<T>(); // new LuaIndexableObjectDataDescriptor(typeof(T), InteropAccessMode.Default, null));
      descriptor.RemoveMetaMember("__index");

      descriptor.AddMetaMember("__index", new ObjectCallbackMemberDescriptor("__index"));
    }

    private Dictionary<DynValue, DynValue> m_Index = new Dictionary<DynValue, DynValue>();

    public DynValue this[DynValue key]
    {
        get
        {
            DynValue d = null;
            try
            {
                d = m_Index[key];
            }
            catch (Exception)
            {}

            return d;
        }
        set { m_Index[key] = value; }
    }
  }
}