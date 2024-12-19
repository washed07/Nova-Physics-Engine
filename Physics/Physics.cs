using System;
using System.Linq;
using Nova.Bodies;
using Nova.Geometry;
using Nova.Numerics;
using System.Collections.Generic;
using Nova.Demos;
using Nova.Physics;
using Nova.Physics.Generators;

namespace Nova.physics;

public class NovaPhysics(NovaEngine novaEngine)
{
    private readonly NovaEngine _novaEngine = novaEngine;

    public void Tick(Repository repository, List<RigidBody> bodies)
    {
        foreach (RigidBody body in _novaEngine.Bodies.Where(body => !body.IsMassInf()))
        {
            // This line is affecting the controlled body every tick, interfering with gravity
            // _novaEngine.Controller.Move(1);  // Remove or move this outside the loop

            Force.Register(repository, new Gravity((0, 9.80)), bodies);
        }
    }
    
}