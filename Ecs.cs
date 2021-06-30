using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

using Guid = System.Guid;
using Console = System.Console;

namespace Blanke
{
  public delegate void DProcess(ECS.Entity[] e, float dt);
  public delegate void DUpdate(ECS.Entity e, float dt, params ECS.Component[] components);
  public delegate void DDraw(ECS.Entity e, params ECS.Component[] components);

  public class ECS {
    private List<System> systems;
    // { uuid:Component[] }
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
        sys.Update(dt);
      }
    }

    public void DrawAll()
    {
      foreach (System sys in systems)
      {
        sys.Draw();
      }
    }

    // SYSTEM
    public class System
    { 
      public DProcess ProcessFn;
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
        if (ProcessFn != null)
        {
          ProcessFn(Entities.ToArray(), dt);
        }
        else if (UpdateFn != null)
        {
          foreach (Entity ent in Entities)
          {
            UpdateFn(ent, dt, ent.Components.ToArray());
          }
        }
      }

      /// <summary>iterate entities in system using DrawFn</summary>
      [MoonSharpHidden]
      public void Draw()
      {
        if (DrawFn != null)
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
    }

    // COMPONENT
    public class ComponentProp {}
    public class ComponentPropGeneric<T> : ComponentProp {
      public string Key;
      public T Value;

      public ComponentPropGeneric(string key, T value)
        => (Key, Value) = (key, value);

      public bool Equals(ComponentPropGeneric<T> other)
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
        Id = new ID(name);
        Parent = ecs;
        Parent.templates[Name] = this;
        foreach (ComponentProp prop in props)
        {
          Add(prop);
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
        Component new_comp = new Component(Name);
        
        return new_comp;
      }
    }
    public class Component
    {
      public ID Id;
      public Dictionary<string, ComponentProp> Props;

      public Component(string name) 
      {
        Id = new ID(name);
        Props = new Dictionary<string, ComponentProp>();
      }

      public bool Equals(Component other)
      {
        return Id == other.Id;
      }

      public T Get<T>(string k)
      {
        return ((ComponentPropGeneric<T>)Props[k]).Value;
      }

      public Component Set<T>(string k, T v)
      {
        if (!Props.ContainsKey(k))
          Props.Add(k, new ComponentPropGeneric<T>(k, v));
        else 
          ((ComponentPropGeneric<T>)Props[k]).Value = v;
        return this;
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
        return this;
      }

      public Entity Remove(ComponentTemplate t)
      {
        if (Signature.Remove(t.Id))
        {
          System.CheckAll(Parent, this);
          Components.Remove(Components.Single(c => c.Id == t.Id));
        }
        return this;
      }
 
      public bool Equals(Entity other)
      {
        return Id == other.Id;
      }
    }
  }
}