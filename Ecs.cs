using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;


namespace Blanke
{
  public delegate void DUpdate(ECS.Entity e, float dt, params ECS.Component[] components);
  public delegate void DDraw(ECS.Entity e, params ECS.Component[] components);
  public delegate void DUpdateAll(ECS.Entity[] e, float dt);
  public delegate void DDrawAll(ECS.Entity[] e);

  public class ECS {
    private List<System> systems;
    // { uuid:Component[] } TODO unused for now. use with systems that only deal with one component
    private Dictionary<int, List<Component>> components;
    private Dictionary<string, ComponentTemplate> templates;

    public ECS()
    {
      systems = new List<System>();
      components = new Dictionary<int, List<Component>>();
      templates = new Dictionary<string, ComponentTemplate>();
    }

    public void UpdateAll(GameTime gt)
    {
      float dt = (float)gt.ElapsedGameTime.TotalSeconds;

      foreach (System sys in systems)
      {
        if (sys.Size() > 0)
          sys.Update(dt);
      }
    }

    public void DrawAll()
    {
      foreach (System sys in systems)
      {
        if (sys.Size() > 0)
          sys.Draw();
      }
    }

    public System system(Table t)
    {
      ComponentTemplate[] templates = new ComponentTemplate[t.Keys.Count()];
      int i = 0;
      foreach (DynValue v in t.Values)
      {
        templates[i++] = v.ToObject<ComponentTemplate>();
      }
      return new System(this, templates);
    }
    
    public ComponentTemplate component(Dictionary<string, DynValue> props)
    {
      List<ComponentProp> proplist = new List<ComponentProp>();
      foreach (KeyValuePair<string, DynValue> entry in props)
      {
        proplist.Add(new ComponentProp(entry.Key, entry.Value));
      }
      return new ComponentTemplate(this, proplist.ToArray());
    }

    public Entity entity(params Component[] components)
    {
      Entity e = new Entity(this);
      foreach (Component c in components)
      {
        e.Add(c);
      }
      return e;
    }

    public static void Load(Engine engine)
    {
      UserData.RegisterType<ECS>();
      UserData.RegisterType<Entity>();
      UserData.RegisterType<ComponentTemplate>();
      UserData.RegisterType<Component>();
      UserData.RegisterType<System>();
      UserData.RegisterType<Component>();

      Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(DUpdate), v => {
        return (DUpdate)((ECS.Entity e, float dt, ECS.Component[] components) => v.Function.Call(e, dt, components).ToObject<System>());
      });
      Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(DDraw), v => {
        return (DDraw)((ECS.Entity e, ECS.Component[] components) => v.Function.Call(e, components).ToObject<System>());
      });
      Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(DUpdateAll), v => {
        return (DUpdateAll)((ECS.Entity[] e, float dt) => v.Function.Call(e, dt).ToObject<System>());
      });
      Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(DDrawAll), v => {
        return (DDrawAll)((ECS.Entity[] e) => v.Function.Call(e).ToObject<System>());
      });

      engine.MainScript.Globals["ecs"] = engine.Ecs;
    }

    // SYSTEM
    public class System
    { 
      public DUpdateAll UpdateAllFn;
      public DDrawAll DrawAllFn;
      public DUpdate UpdateFn;
      public DDraw DrawFn;
      private List<Entity> Entities;
      public ID.Signature Signature;
      private ECS Parent;

      public System(ECS ecs, params ComponentTemplate[] templates) {
        Entities = new List<Entity>();
        Signature = new ID.Signature();
        foreach (ComponentTemplate template in templates)
        {
          // ID id = (ID)obj.GetType().GetProperty("Id", BindingFlags.Static).GetValue(null, null);
          Signature.Add(template.Id);
        }
        Parent = ecs;
        Parent.systems.Add(this);
      }

      public int Size()
      {
        return Entities.Count();
      }

      [MoonSharpHidden]
      public static void CheckAll(ECS ecs, Entity e)
      {
        foreach (System sys in ecs.systems)
        {
          sys.Check(e);
        }
      }
      
      /// <summary>check if entity belongs in system</summary>
      [MoonSharpHidden]
      public void Check(Entity e)
      {
        bool belongs = Signature.Validate(e.Signature);
        int idx = Entities.IndexOf(e);
        if (belongs && idx == -1)
          Entities.Add(e);
        if (!belongs && idx != -1)
          Entities.Remove(e);
      }

      /// <summary>iterate entities in system using UpdateFn</summary>
      [MoonSharpHidden]
      public void Update(float dt)
      {
        if (UpdateAllFn != null)
        {
          UpdateAllFn(Entities.ToArray(), dt);
        }
        else if (UpdateFn != null)
        {
          foreach (Entity ent in Entities)
          {
            Component[] components = new Component[Signature.Size()];
            int i = 0;
            foreach (string id in Signature)
            {
              components[i++] = ent[id];
            }
            UpdateFn(ent, dt, components);
          }
        }
      }

      /// <summary>iterate entities in system using DrawFn</summary>
      [MoonSharpHidden]
      public void Draw()
      {
        if (DrawAllFn != null)
        {
          DrawAllFn(Entities.ToArray());
        }
        else if (DrawFn != null)
        {
          foreach (Entity ent in Entities)
          {
            DrawFn(ent, ent.Components.ToArray());
          }
        }
      }
      
      // LUA 

      public System update(DUpdate fn)
      {
        UpdateFn = fn;
        return this;
      }
      public System draw(DDraw fn)
      {
        DrawFn = fn;
        return this;
      }
      public System updateAll(DUpdateAll fn)
      {
        UpdateAllFn = fn;
        return this;
      }
      public System drawAll(DDrawAll fn)
      {
        DrawAllFn = fn;
        return this;
      }
    }

    // COMPONENT
    public class ComponentProp {
      public string Key;
      public DynValue Value;

      public ComponentProp(string key, DynValue value)
        => (Key, Value) = (key, value);

      public ComponentProp(Engine engine, string key, object value)
        => (Key, Value) = (key, DynValue.FromObject(engine.MainScript, value));

      public bool Equals(ComponentProp other)
      {
        return Key == other.Key;
      }
    }
    // used to set default values for prop keys
    public class ComponentTemplate {
      public string Name;
      public List<ComponentProp> Props;
      public ID Id;
      private ECS Parent;

      public ComponentTemplate(ECS ecs, string name, params ComponentProp[] props)
      {
        Name = name;
        Props = new List<ComponentProp>();
        Id = new ID(Name);
        Parent = ecs;
        Parent.templates[Name] = this;
        foreach (ComponentProp prop in props)
        {
          Add(prop);
        }
      }

      public ComponentTemplate(ECS ecs, params ComponentProp[] props)
      {
        Name = Engine.newGuid();
        Props = new List<ComponentProp>();
        Id = new ID(Name);
        Parent = ecs;
        Parent.templates[Name] = this;
        foreach (ComponentProp prop in props)
        {
          Add(prop);
        }
      }

      public void Populate(Component c)
      {
        foreach (ComponentProp prop in Props)
        {
          c.Set(prop.Key, prop.Value);
        }
      }

      public ComponentTemplate Add(ComponentProp prop)
      {
        ComponentTemplate tprops = Parent.templates[Name];
        // replace if it already exists
        int idx = tprops.Props.IndexOf(prop);
        if (idx != -1)
          tprops.Props[idx] = prop;
        else 
          tprops.Props.Add(prop);
        return this;
      }

      public Component Create()
      {
        return new Component(Parent, Name);
      }

      public bool Equals(Component c)
      {
        return c.Id == Id;
      }

      [MoonSharpUserDataMetamethod("__call")]
      public static Component Call(ComponentTemplate t, Table props)
      {
        Component comp = new Component(t.Parent, t.Name, props);
        return comp;
      }

      public DynValue this[string idx]
      {
        get { return Props.Single(p => p.Key == idx).Value; }
      }
    }
    public class Component : IUserDataType
    {
      public ID Id;
      public Dictionary<string, ComponentProp> Props;
      public Entity Ent;
      public ECS Parent;

      public Component(ECS ecs, string name) 
      {
        Parent = ecs;
        Id = new ID(name);
        Props = new Dictionary<string, ComponentProp>();
        Parent.templates[name].Populate(this);
      }

      public Component(ECS ecs, string name, Table props)
      {
        Parent = ecs;
        Id = new ID(name);
        Props = new Dictionary<string, ComponentProp>();
        Parent.templates[name].Populate(this);
        if (props != null)
        {
          foreach (var entry in props.Pairs)
          {
            Set(entry.Key.String, entry.Value);
          }
        }
      }

      public bool Equals(Component other)
      {
        return Id == other.Id;
      }

      public DynValue Get(string k)
      {
        if (!Props.ContainsKey(k))
        {
          // Log.Error($"Key '{k}' not found.");
          return DynValue.Nil;
        }
        return ((ComponentProp)Props[k]).Value;
      }

      private DynValue Default(string k)
      {
        return Parent.templates[Id.Name][k];
      }

      public T Get<T>(string k)
      {
        DynValue dval = Get(k);
        T ret;
        if (dval == DynValue.Nil)
          ret = Default(k).ToObject<T>();
        else 
          ret = dval.ToObject<T>();
        return ret;
      }

      public Component Set(string k, DynValue v)
      {
        if (!Props.ContainsKey(k))
          Props.Add(k, new ComponentProp(k, v));
        else 
          ((ComponentProp)Props[k]).Value = v;
        return this;
      }

      public bool Has(string k)
      {
        return Props.ContainsKey(k);
      }

      public DynValue Index(Script script, DynValue k, bool isDirectIndexing)
      {
        string key = k.String;
        if (Has(key))
          return Get(key);
        IUserDataType u = this as IUserDataType;
        return key switch
        {
          "Id" => DynValue.FromObject(script, Id),
          "Entity" => DynValue.FromObject(script, Ent),
          _ => null
        };
      }

      public bool SetIndex(Script script, DynValue k, DynValue v, bool isDirectIndexing)
      {
        if (Has(k.String))
        {
          if (v == DynValue.Nil)
            Set(k.String, Default(k.String));
          else 
            Set(k.String, v);
          return true;
        }
        return false;
      }

      public DynValue MetaIndex(Script script, string metaname)
      {
        return null;
      }
    }

    // ENTITY
    public class Entity
    {
      public static int Count;
      public int Id;
      public ID.Signature Signature;
      public List<Component> Components;
      private ECS Parent;

      static Entity()
      {
        Count = 0;
      }

      public Entity(ECS ecs)
      {
        Id = Count++;
        Signature = new ID.Signature();
        Components = new List<Component>();
        Parent = ecs;
      }

      public Entity Add(Component c)
      {
        Signature.Add(c.Id);
        System.CheckAll(Parent, this);
        Components.Add(c);
        c.Ent = this;
        return this;
      }

      public Entity Remove(ComponentTemplate t)
      {
        if (Signature.Remove(t.Id))
        {
          System.CheckAll(Parent, this);
          Components.Remove(Components.Single(c => t.Equals(c)));
        }
        return this;
      }
 
      public bool Equals(Entity other)
      {
        return Id.Equals(other.Id);
      }

      public Component this[ComponentTemplate template]
      {
        get
        {
          return Components.Single(c => template.Id.Equals(c.Id));
        }
      }

      public Component this[string id]
      {
        get
        {
          return Components.Single(c => c.Id.Equals(id));
        }
      }
    }
  }
}