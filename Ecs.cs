using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;


namespace Blanke
{
  public delegate void DAdd(ECS.Entity e, params ECS.Component[] components);
  public delegate void DRemove(ECS.Entity e, params ECS.Component[] components);
  public delegate void DUpdate(ECS.Entity e, float dt, params ECS.Component[] components);
  public delegate void DDraw(ECS.Entity e, params ECS.Component[] components);
  public delegate void DUpdateAll(ECS.Entity[] e, float dt);
  // public delegate void DDrawAll(ECS.Entity[] e);

  public class ECS {
    private List<System> systems;
    // { uuid:Component[] } TODO unused for now. use with systems that only deal with one component
    private Dictionary<int, List<Component>> components;
    private Dictionary<string, ComponentTemplate> templates;
    public Entity root;
    public Table config;

    public ECS(Engine e)
    {
      systems = new List<System>();
      components = new Dictionary<int, List<Component>>();
      templates = new Dictionary<string, ComponentTemplate>();
      root = new Entity(this);
      root.Name = "Root";
      config = new Table(e.MainScript);
      config["order"] = new List<string>();
    }

    private string Tree(Entity e, int level)
    {
      string out_str = "";
      for (int l = 0; l < level; l++)
        out_str += "\t";
      out_str += e.ToString() + "\n";
      foreach (Entity child in e.Children)
      {
        out_str += Tree(child, level+1);
      }
      return out_str;
    }

    ///<summary>get scene graph tree</summary>
    public string Tree()
    {
      return Tree(root, 0);
    }

    public void Update(GameTime gt)
    {
      float dt = (float)gt.ElapsedGameTime.TotalSeconds;

      foreach (System sys in systems)
      {
        if (sys.Size() > 0)
          sys.Update(dt);
      }
    }

    public void Draw()
    {
      root.Render();
      // foreach (System sys in systems)
      // {
      //   if (sys.Size() > 0)
      //     sys.Draw();
      // }
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
      Entity e = new Entity(this, components);
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

      
      Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(DAdd), v => {
        return (DAdd)((ECS.Entity e, ECS.Component[] components) => v.Function.Call(e, components).ToObject<System>());
      });
      Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(DRemove), v => {
        return (DRemove)((ECS.Entity e, ECS.Component[] components) => v.Function.Call(e, components).ToObject<System>());
      });
      Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(DUpdate), v => {
        return (DUpdate)((ECS.Entity e, float dt, ECS.Component[] components) => v.Function.Call(e, dt, components).ToObject<System>());
      });
      Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(DDraw), v => {
        return (DDraw)((ECS.Entity e, ECS.Component[] components) => v.Function.Call(e, components).ToObject<System>());
      });
      Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(DUpdateAll), v => {
        return (DUpdateAll)((ECS.Entity[] e, float dt) => v.Function.Call(e, dt).ToObject<System>());
      });
      // Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Function, typeof(DDrawAll), v => {
      //   return (DDrawAll)((ECS.Entity[] e) => v.Function.Call(e).ToObject<System>());
      // });

      engine.MainScript.Globals["ecs"] = engine.Ecs;
    }

    // SYSTEM
    public class System
    { 
      public DAdd AddFn;
      public DRemove RemoveFn;
      public DUpdateAll UpdateAllFn;
      // public DDrawAll DrawAllFn;
      public DUpdate UpdateFn;
      public DDraw DrawFn;
      private List<Entity> Entities;
      public ID.Signature Signature;
      private ECS Parent;
      public int Order = 0;

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
        {
          Add(e);
        }
        if (!belongs && idx != -1)
        {
          Remove(e);
        }
      }

      [MoonSharpHidden]
      public void Add(Entity e)
      {
        Entities.Add(e);
        e.AddRenderSystem(this);
        if (AddFn != null)
          AddFn(e);
      }

      [MoonSharpHidden]
      public void Remove(Entity e)
      {
        Entities.Remove(e);
        e.RemoveRenderSystem(this);
        if (RemoveFn != null)
          RemoveFn(e);
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
            UpdateFn(ent, dt, ent.Get(Signature));
          }
        }
      }

      /// <summary>iterate entities in system using DrawFn</summary>
      [MoonSharpHidden]
      public void Draw(Entity e)
      {
        if (DrawFn != null)
          DrawFn(e, e.Get(Signature));
        // if (DrawAllFn != null)
        // {
        //   DrawAllFn(Entities.ToArray());
        // }
        // else if (DrawFn != null)
        // {
        //   foreach (Entity ent in Entities)
        //   {
        //     DrawFn(ent, ent.Components.ToArray());
        //   }
        // }
      }
      
      // LUA 

      public System add(DAdd fn)
      {
        AddFn = fn;
        return this;
      }
      public System remove(DRemove fn)
      {
        RemoveFn = fn;
        return this;
      }
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
      public System order(int o)
      {
        Order = o;
        return this;
      }
      public System order(string oname)
      {
        DynValue order_key = DynValue.NewString("order");
        if (Parent.config != null && Parent.config.Keys.Contains(order_key))
        {
          Order = Parent.config.Keys.ToList().IndexOf(order_key);
        }
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
        : this(ecs, Engine.newGuid(), props) {}

      public ComponentTemplate(Engine e, string name, params ComponentProp[] props)
        : this(e.Ecs, name, props) {}

      public ComponentTemplate(Engine e, params ComponentProp[] props)
        : this(e.Ecs, Engine.newGuid(), props) {}

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
        => Id == other.Id;

      public bool Equals(ComponentTemplate other)
        => Id == other.Id;

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
      private ECS Ecs;
      private List<System> RenderSystems = new List<System>();
      private bool SortChildren = false;
      private static int Count;
      public int Id;
      public string Name = "Entity";
      public ID.Signature Signature = new ID.Signature();
      public List<Component> Components = new List<Component>();
      public Entity Parent;
      public List<Entity> Children = new List<Entity>();
      private int _Z = 0;
      public int Z {
        get => _Z;
        set { 
          if (Parent != null)
            Parent.SortChildren = true;
          _Z = value;
        }
      }
      public Transform Transform = new Transform();

      static Entity()
      {
        Count = 0;
      }

      public Entity(ECS ecs, params Component[] components)
      {
        Id = Count++;
        Ecs = ecs;
        foreach (Component c in components)
        {
          Add(c);
        }
        if (ecs.root != null && !this.Equals(ecs.root))
        {
          ecs.root.Add(this);
        }
      }

      public bool Has(ComponentTemplate c)
        => Signature.Has(c.Id);

      public Entity Add(Component c)
      {
        Signature.Add(c.Id);
        System.CheckAll(Ecs, this);
        Components.Add(c);
        c.Ent = this;
        return this;
      }

      public Entity Remove(ComponentTemplate t)
      {
        if (Signature.Remove(t.Id))
        {
          System.CheckAll(Ecs, this);
          Components.Remove(Components.Single(c => t.Equals(c)));
        }
        return this;
      }

      public Entity Add(Entity e)
      {
        if (!Children.Contains(e))
        {
          if (e.Parent != null)
            e.Parent.Remove(e);
          Children.Add(e);
          SortChildren = true;
          e.Parent = this;
        }
        return this;
      }

      public Entity Remove(Entity e)
      {
        Children.Remove(e);
        return this;
      }

      public Entity AddRenderSystem(System sys)
      {
        RenderSystems.Add(sys);
        RenderSystems.Sort((a, b) => a.Order.CompareTo(b.Order));
        return this;
      }

      public Entity RemoveRenderSystem(System sys)
      {
        RenderSystems.Remove(sys);
        return this;
      }

      public void Render()
      {
        // transformations
        if (Parent != null)
          Transform.Update(Parent.Transform);
        else 
          Transform.Update();
        // render this entity
        foreach (System sys in RenderSystems)
        {
          sys.Draw(this);
        }
        // render the children
        if (SortChildren)
        {
          Children.Sort((a, b) => a.Z.CompareTo(b.Z));
        }
        foreach (Entity child in Children)
        {
          child.Render();
        }
      }
 
      public bool Equals(Entity other)
      {
        return Id.Equals(other.Id);
      }

      public override string ToString()
      {
        return $"{Name}{{Id={Id}, Components={Signature}}}";
      }

      public Component Get(ComponentTemplate template)
      {
        return Components.Single(c => template.Id.Equals(c.Id));
      }

      public Component Get(string id)
      {
        return Components.Single(c => c.Id.Equals(id));
      }

      public Component[] Get(ID.Signature sig)
      {
        Component[] components = new Component[sig.Size()];
        int i = 0;
        foreach (string id in sig)
        {
          components[i++] = Get(id);
        }
        return components;
      }
    }
  }
}