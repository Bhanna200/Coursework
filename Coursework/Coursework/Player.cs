﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.Entities;
using BEPUphysics;
using BEPUphysics.DataStructures;
using BEPUphysics.NarrowPhaseSystems.Pairs;

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
        /// Full speed at which ship can rotate; measured in radians per second.
        /// </summary>
        private const float RotationRate = 1.5f;

        /// <summary>
        /// Mass of ship.
        /// </summary>
        private const float Mass = 1.0f;

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

        public Player(GraphicsDevice device, Game1 game)
        {

            graphicsDevice = device;
            this.game = game;
            shipColBox = new Box(shipPos, 4f, 4f, 4f);
            //shipColBox = new Box(shipPos, 0.9f, 0.9f, 0.9f);
            game.space.Add(shipColBox);
            shipColBox.Mass = 10.0f;
            shipColBox.IsAffectedByGravity = false;
            //shipColBox.BecomeDynamic(1);          
            //shipColBox.LinearVelocity = new Vector3(0,-100f,0f);
            shipModel = new EntityModel(shipColBox, game.Content.Load<Model>("Models/Ship"), Matrix.Identity * Matrix.CreateScale(0.005f), game);
            game.Components.Add(shipModel);

            shipColBox.Tag = shipModel;

            Reset();
        }

        /// <summary>
        /// Restore the ship to its original starting state
        /// </summary>
        public void Reset()
        {
            //Position = new Vector3(0, 100, 0);
            Direction = shipColBox.OrientationMatrix.Forward;
            Up = shipColBox.OrientationMatrix.Up;
            right = shipColBox.OrientationMatrix.Right;
            Velocity = Vector3.Zero;
        }

        #endregion

        bool TouchLeft()
        {
            MouseState mouseState = Mouse.GetState();
            return mouseState.LeftButton == ButtonState.Pressed &&
                mouseState.X <= graphicsDevice.Viewport.Width / 3;
        }

        bool TouchRight()
        {
            MouseState mouseState = Mouse.GetState();
            return mouseState.LeftButton == ButtonState.Pressed &&
                mouseState.X >= 2 * graphicsDevice.Viewport.Width / 3;
        }

        bool TouchDown()
        {
            MouseState mouseState = Mouse.GetState();
            return mouseState.LeftButton == ButtonState.Pressed &&
                mouseState.Y <= graphicsDevice.Viewport.Height / 3;
        }

        bool TouchUp()
        {
            MouseState mouseState = Mouse.GetState();
            return mouseState.LeftButton == ButtonState.Pressed &&
                mouseState.Y >= 2 * graphicsDevice.Viewport.Height / 3;
        }

        /// <summary>
        /// Applies a simple rotation to the ship and animates position based
        /// on simple linear motion physics.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            MouseState mouseState = Mouse.GetState();

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;


            // Determine rotation amount from input
            //Vector2 rotationAmount = -gamePadState.ThumbSticks.Left;
            if (keyboardState.IsKeyDown(Keys.Left) || TouchLeft())
            {
                //rotationAmount.X = 1.0f;
                shipColBox.AngularVelocity = new Vector3(0, 2, 0);

            }

            else
                shipColBox.AngularVelocity = new Vector3(0, 0, 0);

            if (keyboardState.IsKeyDown(Keys.Right) || TouchRight())
            {
                //rotationAmount.X = -1.0f;
                shipColBox.AngularVelocity = new Vector3(0, -2, 0);
            }

            if (keyboardState.IsKeyDown(Keys.Up) || TouchUp())
            {
                //rotationAmount.Y = -1.0f;
                shipColBox.AngularVelocity = new Vector3(-1, 0, 0);
            }

            if (keyboardState.IsKeyDown(Keys.Down) || TouchDown())
            {
                //rotationAmount.Y = 1.0f;
                shipColBox.AngularVelocity = new Vector3(1, 0, 0);
            }



            // Scale rotation amount to radians per second
            //rotationAmount = rotationAmount * RotationRate * elapsed;

            // Correct the X axis steering when the ship is upside down
            //if (Up.Y < 0)
            //    //rotationAmount.X = -rotationAmount.X;
            //    shipColBox.AngularVelocity = -shipColBox.AngularVelocity;


            // Create rotation matrix from rotation amount
            //Matrix rotationMatrix =
            //    Matrix.CreateFromAxisAngle(Right, rotationAmount.Y) *
            //    Matrix.CreateRotationY(rotationAmount.X);

            // Rotate orientation vectors
            //Direction = Vector3.TransformNormal(Direction, rotationMatrix);
            //Up = Vector3.TransformNormal(Up, rotationMatrix);

            // Re-normalize orientation vectors
            // Without this, the matrix transformations may introduce small rounding
            // errors which add up over time and could destabilize the ship.
            Direction.Normalize();
            Up.Normalize();

            // Re-calculate Right
            right = Vector3.Cross(Direction, Up);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            Up = Vector3.Cross(Right, Direction);

            // Determine thrust amount from input
            float thrustAmount = gamePadState.Triggers.Right;
            if (keyboardState.IsKeyDown(Keys.W) || mouseState.LeftButton == ButtonState.Pressed)
            {
                thrustAmount = 10.0f;
                //shipColBox.LinearVelocity = new Vector3(0, 0, -20f);

            }
            else
                shipColBox.LinearVelocity = new Vector3(0, 0, 0f);

            // Calculate force from thrust amount
            Vector3 force = shipColBox.OrientationMatrix.Forward * thrustAmount * ThrustForce;

            // Apply acceleration
            Vector3 acceleration = force / Mass;
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