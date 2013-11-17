using Microsoft.Xna.Framework;
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
        public Game game;

        //public Vector3 shipPos = new Vector3(0f, 4.0f, 0f);
        public Vector3 shipPos;

        public Box shipColBox;
        public EntityModel shipModel;


        public Vector3 PlayaPos
        {
            get
            {
                return shipPos;
            }
            set
            {
                //yaw = MathHelper.WrapAngle(value);
                shipPos = new Vector3(0f, 4.0f, 0f);
            }
        }






        public KeyboardState KeyboardState;
        /// <summary>
        /// Contains the latest snapshot of the mouse's input state.
        /// </summary>
        public MouseState MouseState;

        public Matrix cubeWorld;

        public void PlayerInitalize()
        {
            cubeWorld = Matrix.Identity;


        }

        public void PlayerLoad(Game1 game)
        {
            this.game = game;
            shipColBox = new Box(shipPos, 0.9f, 0.9f, 0.9f);
            game.space.Add(shipColBox);
            shipColBox.Mass = 1.0f;
            shipColBox.IsAffectedByGravity = false;
            //shipColBox.BecomeDynamic(1);          

            shipModel = new EntityModel(shipColBox, game.Content.Load<Model>("Models/Ship"), Matrix.Identity * Matrix.CreateScale(0.0005f), game);
            game.Components.Add(shipModel);

            shipColBox.Tag = shipModel;
        }

        public void PlayerUpdate(GameTime gameTime)
        {
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();

            if (KeyboardState.IsKeyDown(Keys.Up))
            {
                //shipColBox.LinearVelocity = new Vector3(0, 0, -1);
                //shipColBox.LinearVelocity = new Vector3(0, -1, 0);
                //shipPos += new Vector3(0, 1, 0);
                shipColBox.Position += new Vector3(0, 0, -0.01f);
            }
            if (KeyboardState.IsKeyDown(Keys.Down))
            {
                //shipColBox.LinearVelocity = new Vector3(0, 0, -1);
                //shipColBox.LinearVelocity = new Vector3(0, -1, 0);
                //shipPos += new Vector3(0, -1, 0);
                //player.shipColBox.Position += new Vector3(0, 0, 0.01f);
                shipColBox.Position += new Vector3(0, -0.1f, 0.01f);
            }

        }


        public void PlayerDraw()
        {


        }




    }
}