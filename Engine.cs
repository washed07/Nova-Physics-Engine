using System;
using System.Collections.Generic;
using System.Linq;
using Nova.physics;
using Nova.Bodies;
using Nova.Geometry;
using Nova.Numerics;
using Nova.Physics;
using Nova.Demos;

namespace Nova;

public class NovaEngine
{
    private static readonly Num TicksPerSecond = 144;
    private static readonly Num Speed          = 1;
    private static readonly Num Iterations     = 1;

    private NovaPhysics _physics;
    private Repository _repository;
    public Controller Controller;
    private Num     _runtime = 0;
    private Num     _timer   = 0;

    private static Num TimeStep => 1 / TicksPerSecond / Iterations * Speed;

    public List<RigidBody> Bodies { get; set; } = 
        [
            new RigidBody(Shape.Rect(20, 20), (100, 100), 100, 1),
            new RigidBody(Shape.Rect(20, 20), (200, 100), 100, 1),
            new RigidBody(Shape.Rect(20, 20), (300, 100), 100, 1),
            new RigidBody(Shape.Rect(500, 20), (100, 400), -1, 1),
        ];

    private List<RigidBody> RemoveQueue => [];

    public void Initialize()
    {
        _physics = new NovaPhysics(this);
        _repository = new Repository();
        Controller = new Controller(Bodies[0]);
        foreach (RigidBody body in Bodies) { body.Initialize(); }
    }

    public void LoadContent()
    {
        // Load content
    }

    public void Update(Num deltaTime)
    {
        _timer += deltaTime;
        if (!(_timer > TimeStep)) { return; }

        _timer   =  0;
        _runtime += TimeStep;

        for (int _Iteration = 0; _Iteration < Iterations; _Iteration++) 
        { 
            _physics.Tick(_repository, Bodies);
            _repository.Commit();
            _repository.Clear();
        }

        foreach (RigidBody body in Bodies) // Integrate particle physics
        {
            Controller.Move(1);
            body.Integrate();
            body.Update();
            Collision.ResolveAll(Bodies);
        }

        foreach (RigidBody body in RemoveQueue) { Bodies.Remove(body); } // Remove aged particles

        RemoveQueue.Clear();
    }

    public void RomoveBody(RigidBody body) { RemoveQueue.Add(body); }
}