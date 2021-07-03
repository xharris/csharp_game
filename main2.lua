function main()  
  window{title="test"}
  background(color("light_blue", 100))

  print("make planets")
  local sun, solar_system = newPlanet(game.width/2, game.height/2, 100, color("red"))
  local earth, earth_orbit = newPlanet(100, 0, 50, color("blue"))
  -- local moon, moon_orbit = newPlanet(20, 0, 20, color("white"))

  -- ecs.scene:add(
  --   solar_system + 
  --     earth_orbit + 
  --       moon_orbit
  -- )
end

-- -- 2D: x, y, r, sx, sy
-- -- 3D: x, y, z, rx, ry, rz, sx, sy, sz

print("setup components")
Transform = ecs.component{x=0,y=0,r=0}
Planet = ecs.component{name="unknown", radius=0, color=color("white")}
Orbit = ecs.component{spin=0.1}

function newPlanet(x, y, r, c)
  print("new planet! ")
  local e_planet = ecs.entity( 
    Transform{x=x, y=y}, 
    Planet{radius=r, color=c}, 
    graphics.Circle{line=color("white"), fill=c, thickness=3} 
  ) 
  local e_orbit = ecs.entity( Transform(), Orbit() )

  e_orbit:Add(e_planet)
  print("planet finished")
  return e_orbit, e_planet
end

ecs.config = {
  order = { "ui", "game" }
}

ecs.system{ Planet, graphics.Circle }
  :update(function(e, dt, c)
    local planet, circle = unpack(c)
    circle.fill = planet.color
    circle.r = planet.radius
  end)

print("setup system")
ecs.system{ Transform, Orbit }
  :update(function(e, dt, c)
    local tform, orbit = unpack(c)
    tform.r = tform.r + orbit.spin * dt
  end)

main()