﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nova.Bodies;
using Nova.Numerics;

namespace Nova;

public class Core : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private          SpriteBatch           _spriteBatch;
    private readonly NovaEngine                _engine = new NovaEngine(); // The physics Engine
    private          Texture2D             _whiteTexture;
    private          BasicEffect           _basicEffect; // BasicEffect for drawing polygons
    private          VertexBuffer          _vertexBuffer;
    private          IndexBuffer           _indexBuffer;
    private          int                   _maxVertices = 1000; // Adjust based on expected maximum vertices
    private          int                   _maxIndices  = 3000; // Adjust based on expected maximum indices

    public Core()
    {
        _graphics             = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible        = true;

        // Set the target framerate to 144 FPS
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 144.0);
    }

    protected override void Initialize()
    {
        // Initialize the engine
        _engine.Initialize();

        // Set screen width & height
        _graphics.PreferredBackBufferWidth  = 1000;
        _graphics.PreferredBackBufferHeight = 800;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _engine.LoadContent();
        _whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
        _whiteTexture.SetData([Color.White]);

        // Initialize BasicEffect
        _basicEffect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true,
            Projection = Matrix.CreateOrthographicOffCenter
                (0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1)
        };

        // Initialize vertex and index buffers
        _vertexBuffer = new VertexBuffer
            (GraphicsDevice, typeof(VertexPositionColor), _maxVertices, BufferUsage.WriteOnly);
        _indexBuffer = new IndexBuffer
            (GraphicsDevice, IndexElementSize.ThirtyTwoBits, _maxIndices, BufferUsage.WriteOnly);
    }

    protected override void Update(GameTime gameTime)
    {
        // Exit the game if the back button or escape key is pressed
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape)) { Exit(); }

        // Update the physics engine
        _engine.Update(gameTime.ElapsedGameTime);
        Console.WriteLine(gameTime.ElapsedGameTime);

        base.Update(gameTime);
    }

    private void ResizeBuffers(int requiredVertices, int requiredIndices)
    {
        if (requiredVertices > _maxVertices)
        {
            _maxVertices = Math.Max(requiredVertices, _maxVertices * 2);
            _vertexBuffer?.Dispose();
            _vertexBuffer = new VertexBuffer
                (GraphicsDevice, typeof(VertexPositionColor), _maxVertices, BufferUsage.WriteOnly);
        }

        if (requiredIndices <= _maxIndices) { return; }

        _maxIndices = Math.Max(requiredIndices, _maxIndices * 2);
        _indexBuffer?.Dispose();
        _indexBuffer = new IndexBuffer
            (GraphicsDevice, IndexElementSize.ThirtyTwoBits, _maxIndices, BufferUsage.WriteOnly);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        List<VertexPositionColor> vertexList = [];
        List<int>                 indexList  = [];

        // Collect vertices and indices for all particles
        foreach (RigidBody body in _engine.Bodies)
        {
            try { AddPolygonVertices(body, vertexList, indexList, Color.White); }
            catch (Exception ex) { Console.WriteLine($"An error occurred while drawing the polygon: {ex.Message}"); }
        }

        // Update buffers and draw polygons if there are any vertices and indices
        if (vertexList.Count > 0 && indexList.Count > 0)
        {
            // Ensure buffers are large enough
            ResizeBuffers(vertexList.Count, indexList.Count);
            try
            {
                _vertexBuffer.SetData(vertexList.ToArray());
                _indexBuffer.SetData(indexList.ToArray());
                GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                GraphicsDevice.Indices = _indexBuffer;
                foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexList.Count / 3);
                }
            }
            catch (Exception ex) { Console.WriteLine($"Buffer error: {ex.Message}"); }
        }

        // Draw the outlines of the polygons
        _spriteBatch.Begin();
        foreach (Vect[] transformedVerts in _engine.Bodies.Select
                     (particle => particle.Polygon.TransformedVertices()))
        {
            for (int i = 0; i < transformedVerts.Length; i++)
            {
                Vector2 start = transformedVerts[i];
                Vector2 end   = transformedVerts[(i + 1) % transformedVerts.Length];
                DrawLine(_spriteBatch, _whiteTexture, start, end, Color.Red);
            }
        }
        
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    // Add vertices and indices for a polygon to the lists
    private static void AddPolygonVertices
        (RigidBody body, List<VertexPositionColor> vertexList, List<int> indexList, Color fillColor)
    {
        Vect[] vertices  = body.Polygon.TransformedVertices();
        int    baseIndex = vertexList.Count;
        vertexList.AddRange(vertices.Select(t => new VertexPositionColor(new Vector3(t, 0), fillColor)));
        for (int i = 0; i < vertices.Length - 2; i++)
        {
            indexList.Add(baseIndex);
            indexList.Add(baseIndex + i + 1);
            indexList.Add(baseIndex + i + 2);
        }
    }

    // Draw a line between two points
    private static void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color)
    {
        ArgumentNullException.ThrowIfNull(spriteBatch);
        ArgumentNullException.ThrowIfNull(texture);
        Vector2 edge  = end - start;
        float   angle = (float)Math.Atan2(edge.Y, edge.X);
        spriteBatch.Draw
            (
             texture,
             new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 1),
             null,
             color,
             angle,
             Vector2.Zero,
             SpriteEffects.None,
             0
            );
    }
}