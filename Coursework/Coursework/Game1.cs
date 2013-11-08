using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.Entities;
using BEPUphysics;
using BEPUphysics.DataStructures;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionShapes;
using BEPUphysicsDrawer.Models;
using BEPUphysicsDrawer.Font;
using BEPUphysicsDrawer.Lines;



namespace Coursework
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        /// <summary>
        /// World in which the simulation runs.
        /// </summary>
        Space space;
        /// <summary>
        /// Controls the viewpoint and how the user can see the world.
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// Graphical model to use for the boxes in the scene.
        /// </summary>
        public Model CubeModel;
        /// <summary>
        /// Graphical model to use for the environment.
        /// </summary>
        public Model terrain;
        /// <summary>
        /// Contains the latest snapshot of the keyboard's input state.
        /// </summary>
        public KeyboardState KeyboardState;
        /// <summary>
        /// Contains the latest snapshot of the mouse's input state.
        /// </summary>
        public MouseState MouseState;

        public EntityModel shipModel;

        public EntityModel playerModel;

        public Box shipColBox;
        public Vector3 shipPos = new Vector3(0f, 4.0f, 0f);
        public SpriteFont font;

        //public Vector3 shipPos { get; set; }
        //public ModelDrawer ModelDrawer;

        public MobileMesh shipMesh;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Setup the camera.
            Camera = new Camera(this, new Vector3(0, 3, 10), 5);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //This 1x1x1 cube model will represent the box entities in the space.
            CubeModel = Content.Load<Model>("Models/cube");

            terrain = Content.Load<Model>("Models/desert");

            font = Content.Load<SpriteFont>("Fonts/Debug");

            //ModelDrawer = new InstancedModelDrawer(this);

            //Construct a new space for the physics simulation to occur within.
            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            DrawTerrain();
            DrawPlayerShip();



        }

        public void DrawTerrain()
        {
            //===============================TERRAIN================================================
            //Create a physical environment from a triangle mesh.
            //First, collect the the mesh data from the model using a helper function.
            //This special kind of vertex inherits from the TriangleMeshVertex and optionally includes
            //friction/bounciness data.
            //The StaticTriangleGroup requires that this special vertex type is used in lieu of a normal TriangleMeshVertex array.
            Vector3[] vertices;
            int[] indices;
            TriangleMesh.GetVerticesAndIndicesFromModel(terrain, out vertices, out indices);
            //Give the mesh information to a new StaticMesh.  
            //Give it a transformation which scoots it down below the kinematic box entity we created earlier.
            var mesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -40, 0)));

            //Add it to the space!
            space.Add(mesh);
            //Make it visible too.
            Components.Add(new StaticModel(terrain, mesh.WorldTransform.Matrix, this));
            //======================================================================================         

        }

        public void DrawPlayerShip()
        {
            //Vector3[] ShipVertices;
            //int[] ShipIndices;
            ////Create a big hollow sphere (squished into an ellipsoid).
            //TriangleMesh.GetVerticesAndIndicesFromModel(Content.Load<Model>("Models/Ship"), out ShipVertices, out ShipIndices);
            //var transform = new AffineTransform(new Vector3(.0005f, .0005f, .0005f), Quaternion.Identity, new Vector3(0, 0, 0));

            shipColBox = new Box(shipPos, 0.9f, 0.9f, 0.9f);
            space.Add(shipColBox);
            shipModel = new EntityModel(shipColBox, Content.Load<Model>("Models/Ship"), Matrix.Identity * Matrix.CreateScale(0.0005f), this);
            shipColBox.Tag = shipModel;
            //shipColBox.LinearVelocity = new Vector3(0,-1,0);
            //shipColBox.Mass = 1.0f;
            shipColBox.IsAffectedByGravity = false;
            shipColBox.BecomeDynamic(1);

            Components.Add(shipModel);

            shipColBox.CollisionInformation.Events.DetectingInitialCollision += HandleCollision;
            ////Note that meshes can also be made solid (MobileMeshSolidity.Solid).  This gives meshes a solid collidable volume, instead of just
            ////being thin shells.  However, enabling solidity is more expensive.
            //shipMesh = new MobileMesh(ShipVertices, ShipIndices, transform, MobileMeshSolidity.Counterclockwise);
            //shipMesh.Position = new Vector3(1, 0, 0);
            //shipMesh.LinearVelocity = new Vector3(0, -1, 0);
            //shipMesh.Mass = 1;
            ////shipMesh.CollisionInformation.Shape.MeshCollisionMargin = 0.1f;
            ////shipMesh.LocalInertiaTensorInverse = new Matrix3X3();
            ////shipMesh.IgnoreShapeChanges = true;
            //space.Add(shipMesh);

            //Matrix scaling = Matrix.CreateScale(0.0005f); //Since the cube model is 1x1x1, it needs to be scaled to match the size of each individual box.
            //EntityModel model = new EntityModel(shipMesh, shipModel, scaling, this);
            ////Add the drawable game component for this entity to the game.
            //Components.Add(model);
        }

        void HandleCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            //This type of event can occur when an entity hits any other object which can be collided with.
            //They aren't always entities; for example, hitting a StaticMesh would trigger this.
            //Entities use EntityCollidables as collision proxies; see if the thing we hit is one.
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation != null)
            {
                // We hit an entity! remove it.
                space.Remove(otherEntityInformation.Entity);
                //Remove the graphics too.
                Components.Remove((EntityModel)otherEntityInformation.Entity.Tag);
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            //ModelDrawer.Update();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || KeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            //Update the camera.
            Camera.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            #region Block shooting

            if (MouseState.LeftButton == ButtonState.Pressed)
            {
                //If the user is clicking, start firing some boxes.
                //First, create a new dynamic box at the camera's location.
                Box toAdd = new Box(Camera.Position, 1, 1, 1, 1);
                //Set the velocity of the new box to fly in the direction the camera is pointing.
                //Entities have a whole bunch of properties that can be read from and written to.
                //Try looking around in the entity's available properties to get an idea of what is available.
                toAdd.LinearVelocity = Camera.WorldMatrix.Forward * 10;
                //Add the new box to the simulation.
                space.Add(toAdd);

                //Add a graphical representation of the box to the drawable game components.
                EntityModel model = new EntityModel(toAdd, CubeModel, Matrix.Identity, this);
                Components.Add(model);
                toAdd.Tag = model;  //set the object tag of this entity to the model so that it's easy to delete the graphics component later if the entity is removed.
            }


            #endregion


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
                shipColBox.Position += new Vector3(0, 0, 0.01f);
            }



            //Steps the simulation forward one time step.
            space.Update();





            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch = new SpriteBatch(GraphicsDevice);


            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rasterizerState;

            // TODO: Add your drawing code here
            //DrawModels(shipModel, Matrix.CreateScale(0.0005f));
            //ModelDrawer.Draw(Camera.ViewMatrix, Camera.ProjectionMatrix);
            //spriteBatch.Begin();

            //spriteBatch.DrawString(font, "Ship Pos: " + shipColBox.Position, new Vector2(10, 10), Color.Black);

            //spriteBatch.End();


            base.Draw(gameTime);
        }

        private void DrawModels(Model model, Matrix world)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * world;

                    // Use the matrices provided by the chase camera
                    effect.View = Camera.ViewMatrix;
                    effect.Projection = Camera.ProjectionMatrix;
                }
                mesh.Draw();
            }
        }
    }
}
