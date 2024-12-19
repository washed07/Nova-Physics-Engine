using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nova.Numerics;
using Nova.Bodies;

namespace Nova.Demos
{
    public class Controller
    {
        private RigidBody _controlledBody;

        public Controller(RigidBody controlledBody)
        {
            _controlledBody = controlledBody;
        }

        public void Move(Num speed)
        {
            Vector2       velocity = Vector2.Zero;
            KeyboardState state    = Keyboard.GetState();

            if (state.IsKeyDown(Keys.W))
            {
                velocity += new Vector2(0, -speed);
            }
            if (state.IsKeyDown(Keys.A))
            {
                velocity += new Vector2(-speed, 0);
            }
            if (state.IsKeyDown(Keys.S))
            {
                velocity += new Vector2(0, speed);
            }
            if (state.IsKeyDown(Keys.D))
            {
                velocity += new Vector2(speed, 0);
            }

            _controlledBody.SetVelocity(velocity);
        }
    }
}