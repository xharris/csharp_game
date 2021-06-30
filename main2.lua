window{title="test"}
background(white)

-- 2D: x, y, r, sx, sy
-- 3D: x, y, z, rx, ry, rz, sx, sy, sz

Planet = ecs.component{name="unknown", radius=0, color=white}
Orbit = ecs.component{spin=0.1}

function newPlanet(transform, planet, orbit)
  local planet = ecs.entity( transform, planet, primitive.circle{fill=planet.color, } ) 
  local orbit = ecs.entity( orbit or Orbit() )

  orbit.add(planet)
  planet.add(render)
  return orbit, planet
end

ecs.system{ Planet, primitive.circle }
  :update(function(e, dt, planet, circle)
    circle.fill = planet.color
    circle.r = planet.radius
  end)


ecs.system{ Transform, Orbit }
  :update(function(e, dt, tform, orbit)
    tform.r = tform.r + orbit.spin * dt
  end)

function setup()  
  local sun, solar_system = newPlanet( Transform{x=game.width/2, y=game.height/2}, Planet{radius=100, color=red} )
  local earth, earth_orbit = newPlanet( Transform{x=100}, Planet{radius=50, color=blue} )
  local moon, moon_orbit = newPlanet( Transform{x=20}, Planet{radius=20, color=white} )

  ecs.scene:add(
    solar_system + 
      earth_orbit + 
        moon_orbit
  )
end