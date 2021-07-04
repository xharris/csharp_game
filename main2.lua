-- ecs.config = {
--   order = { "ui", "game" }
-- }

function main()  
  window{title="test"}
  background(color("grey", 900))

  print("make planets")
  local solar_system = newPlanet("Sun", 0, 100, color("red"))
  local earth_orbit = newPlanet("Earth", 140, 50, color("blue"))
  local moon_orbit = newPlanet("Moon", 50, 20, color("grey"))

  solar_system.Transform.x = game.width/2
  solar_system.Transform.y = game.height/2
  solar_system:Add(
    earth_orbit:Add(
      moon_orbit
    )
  )

  print(ecs:Tree())

  -- ecs.scene:add(
  --   solar_system + 
  --     earth_orbit + 
  --       moon_orbit
  -- )
end

Planet = ecs.component{radius=0, color=color("white")}
Orbit = ecs.component{distance=0, spin=1}

function newPlanet(name, distance, radius, c)
  -- orbit
  local e_orbit = ecs.entity( Orbit{distance=(distance > 0) and distance + radius or distance} )
  -- planet
  local e_planet = ecs.entity( 
    Planet{radius=radius, color=c}, 
    graphics.Circle{line=color("white"), fill=c, thickness=1} 
  ) 

  e_orbit.Name = name.."Orbit"
  e_planet.Name = name

  e_orbit:Add(e_planet)

  return e_orbit
end

ecs.system{ Planet, graphics.Circle }
  :update(function(e, dt, c)
    local planet, circle = unpack(c)
    circle.fill = planet.color
    circle.r = planet.radius
  end)

ecs.system{ Orbit }
  :update(function(e, dt, c)
    local orbit = unpack(c)
    -- for c, child in ipairs(e.Children) do 
    --   if c:Has(Planet) do 
    --     x = x + c:Get(Planet).radius
    --   end
    -- end
    if orbit.distance > 0 then 
      e.Transform.x = orbit.distance
    end
    e.Transform.r = e.Transform.r + orbit.spin * dt
  end)

main()