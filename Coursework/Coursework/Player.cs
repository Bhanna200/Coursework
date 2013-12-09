using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using BEPUphysics.Entities.Prefabs;
using BEPUphysicsDrawer.Lines;
using Microsoft.Xna.Framework.Audio;

namespace Coursework
{
    public class Player
    {
        #region Fields

        public Game game;
        public Vector3 shipPos = new Vector3(0f, 10.0f, 0f);
        public Box shipColBox;
        public EntityModel shipModel;
        public Matrix playerWorld;
        public float thrustAmount = 0;
        public SoundEffect laser;

        //private const float MinimumAltitude = 350.0f;

        /// <summary>
        /// A reference to the graphics device used to access the viewport for touch input.
        /// </summary>
        private GraphicsDevice graphicsDevice;

        /// <summary>
        /// Location of ship in world space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Direction ship is facing.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Ship's up vector.
        /// </summary>
        public Vector3 Up;

        private Vector3 right;
        /// <summary>
        /// Ship's right vector.
        /// </summary>
        public Vector3 Right
        {
            get { return right; }
        }

        /// <summary>
        /// Maximum force that can be applied along the ship's direction.
        /// </summary>
        private const float ThrustForce = 24.0f;

        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.97f;

        /// <summary>
        /// Current ship velocity.
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// Ship world transform matrix.
        /// </summary>
        public Matrix World
        {
            get { return world; }
        }
        private Matrix world;

        #endregion

        #region Initialization

        public Player(Game1 game)
        {

            //graphicsDevice = device;
            this.game = game;
            shipColBox = new Box(shipPos, 1f, 1f, 1f);
            shipColBox.Mass = 2.0f;
            shipColBox.IsAffectedByGravity = false;
            game.space.Add(shipColBox);
            shipModel = new EntityModel(shipColBox, game.Content.Load<Model>("Models/Ship"), Matrix.Identity * Matrix.CreateScale(0.0005f), game);
            laser = game.Content.Load<SoundEffect>("Audio/Laser");
            game.Components.Add(shipModel);

            shipColBox.Tag = shipModel;


        }

        #endregion

        /// <summary>
        /// Applies a simple rotation to the ship and animates position based
        /// on simple linear motion physics.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();


            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;


            // Determine rotation amount from input            
            if (keyboardState.IsKeyDown(Keys.Left))
            {

                shipColBox.AngularVelocity = new Vector3(0, 2, 0);

            }

            else
                shipColBox.AngularVelocity = new Vector3(0, 0, 0);

            if (keyboardState.IsKeyDown(Keys.Right))
            {

                shipColBox.AngularVelocity = new Vector3(0, -2, 0);
            }

            if (keyboardState.IsKeyDown(Keys.Up))
            {

                shipColBox.AngularVelocity = new Vector3(-1, 0, 0);
            }

            if (keyboardState.IsKeyDown(Keys.Down))
            {

                shipColBox.AngularVelocity = new Vector3(1, 0, 0);
            }

            // Scale rotation amount to radians per second
            //rotationAmount = rotationAmount * RotationRate * elapsed;

            // Correct the X axis steering when the ship is upside down
            //if (Up.Y < 0)
            //    //rotationAmount.X = -rotationAmount.X;
            //    shipColBox.AngularVelocity = -shipColBox.AngularVelocity;


            //Create rotation matrix from rotation amount
            Matrix rotationMatrix =
                Matrix.CreateFromAxisAngle(Right, shipColBox.Orientation.Y) *
                Matrix.CreateRotationY(shipColBox.Orientation.X);

            //Rotate orientation vectors
            Direction = Vector3.TransformNormal(shipColBox.OrientationMatrix.Forward, rotationMatrix);
            Up = Vector3.TransformNormal(shipColBox.OrientationMatrix.Up, rotationMatrix);

            // Re-normalize orientation vectors
            // Without this, the matrix transformations may introduce small rounding
            // errors which add up over time and could destabilize the ship.
            Direction.Normalize();
            Up.Normalize();

            // Re-calculate Right
            //right = Vector3.Cross(Direction, Up);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            //Up = Vector3.Cross(Right, Direction);

            // Determine thrust amount from input

            if (keyboardState.IsKeyDown(Keys.W))
            {
                thrustAmount = 5.0f;
                //shipColBox.LinearVelocity = new Vector3(0, 0, -20f);

            }
            else
                thrustAmount = 0;
            shipColBox.LinearVelocity = new Vector3(0, 0, 0f);

            // Calculate force from thrust amount
            Vector3 force = shipColBox.OrientationMatrix.Forward * thrustAmount * ThrustForce;

            // Apply acceleration
            Vector3 acceleration = force / shipColBox.Mass;
            Velocity += acceleration * elapsed;

            //Apply psuedo drag
            Velocity *= DragFactor;

            //Apply velocity
            shipColBox.Position += Velocity * elapsed;

            // Prevent ship from flying under the ground
            //Position.Y = Math.Max(Position.Y, MinimumAltitude);


            // Reconstruct the ship's world matrix
            world = Matrix.Identity;
            world.Forward = Direction;
            world.Up = Up;
            world.Right = right;
            world.Translation = Position;
        }


    }
}
