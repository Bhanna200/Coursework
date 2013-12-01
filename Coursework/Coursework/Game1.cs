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
        #region Fields

        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        public SpriteFont font;


        KeyboardState lastKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        MouseState lastMousState = new MouseState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        MouseState currentMouseState = new MouseState();

        public Player player;
        public ChaseCamera camera;

        //Model shipModel;
        //Model groundModel;
        public Model terrain;
        public Model CubeModel;

        public bool cameraMode3rdPerson;
        public bool cameraModeChase = true;

        public Texture2D cockpit;

        public Space space;

        BoundingBoxDrawer debugBoundingBoxDrawer;
        BasicEffect debugDrawer;

        bool cameraSpringEnabled = true;
        bool debug = false;

        Skybox skybox;
        Vector3 cameraPosition;

        SoundEffect chainGun;


        #endregion

        #region Initialization


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.SupportedOrientations = DisplayOrientation.Portrait;


            Content.RootDirectory = "Content";
            IsMouseVisible = true;

#if WINDOWS_PHONE
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;
            
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.IsFullScreen = true;
#else
            graphics.PreferredBackBufferWidth = 853;
            graphics.PreferredBackBufferHeight = 480;
#endif
            //space = new Space();
            //space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);
            // Create the chase camera
            camera = new ChaseCamera();

            // Set the camera offsets
            camera.DesiredPositionOffset = new Vector3(0.0f, 20.0f, 60.0f);
            camera.LookAtOffset = new Vector3(0.0f, 15.0f, 0.0f);

            // Set camera perspective
            camera.NearPlaneDistance = 10.0f;
            camera.FarPlaneDistance = 100000.0f;

            //TODO: Set any other camera invariants here such as field of view
        }


        /// <summary>
        /// Initalize the game
        /// </summary>
        protected override void Initialize()
        {
            debugBoundingBoxDrawer = new BoundingBoxDrawer(this);
            debugDrawer = new BasicEffect(GraphicsDevice);

            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            player = new Player(GraphicsDevice, this);

            // Set the camera aspect ratio
            // This must be done after the class to base.Initalize() which will
            // initialize the graphics device.
            //camera.AspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
            //    graphics.GraphicsDevice.Viewport.Height;


            // Perform an inital reset on the camera so that it starts at the resting
            // position. If we don't do this, the camera will start at the origin and
            // race across the world to get behind the chased object.
            // This is performed here because the aspect ratio is needed by Reset.
            UpdateCameraChaseTarget();
            camera.Reset();

            base.Initialize();
        }


        /// <summary>
        /// Load graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Fonts/Debug");

            terrain = Content.Load<Model>("Models/desert");

            font = Content.Load<SpriteFont>("Fonts/Debug");

            CubeModel = Content.Load<Model>("Models/cube");

            cockpit = Content.Load<Texture2D>("Models/cockpit");

            skybox = new Skybox("Skyboxes/Sunset", Content);

            chainGun = Content.Load<SoundEffect>("Audio/chainGun");

            DrawTerrain();
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
            var mesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -30, 0)));

            //Add it to the space!
            space.Add(mesh);
            //Make it visible too.

            Components.Add(new StaticModel(terrain, mesh.WorldTransform.Matrix, this));
            //======================================================================================         

        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            //Steps the simulation forward one time step.
            space.Update();

            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;
            lastMousState = currentMouseState;

            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentMouseState = Mouse.GetState();

            // Exit when the Escape key or Back button is pressed
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            bool touchTopLeft = currentMouseState.LeftButton == ButtonState.Pressed &&
                    lastMousState.LeftButton != ButtonState.Pressed &&
                    currentMouseState.X < GraphicsDevice.Viewport.Width / 10 &&
                    currentMouseState.Y < GraphicsDevice.Viewport.Height / 10;

            // Pressing the A button or key toggles the spring behavior on and off
            if (lastKeyboardState.IsKeyUp(Keys.A) &&
                (currentKeyboardState.IsKeyDown(Keys.A)) ||
                (lastGamePadState.Buttons.A == ButtonState.Released &&
                currentGamePadState.Buttons.A == ButtonState.Pressed) ||
                touchTopLeft)
            {
                cameraSpringEnabled = !cameraSpringEnabled;
            }

            // Reset the ship on R key or right thumb stick clicked
            if (currentKeyboardState.IsKeyDown(Keys.R) ||
                currentGamePadState.Buttons.RightStick == ButtonState.Pressed)
            {
                player.Reset();
                camera.Reset();
            }

            // Update the ship
            player.Update(gameTime);

            // Update the camera to chase the new target
            UpdateCameraChaseTarget();

            // The chase camera's update behavior is the springs, but we can
            // use the Reset method to have a locked, spring-less camera
            if (cameraSpringEnabled)
                camera.Update(gameTime);
            else
                camera.Reset();


            if (currentKeyboardState.IsKeyDown(Keys.Space))
            {
                Firebullet();

            }

            if (currentKeyboardState.IsKeyDown(Keys.D1))
            {
                cameraModeChase = true;
                cameraMode3rdPerson = false;
            }

            if (currentKeyboardState.IsKeyDown(Keys.D2))
            {
                cameraModeChase = false;
                cameraMode3rdPerson = true;
            }


            if (currentKeyboardState.IsKeyDown(Keys.U))
            {
                debug = true;
            }
            else debug = false;

            //angle += 0.002f;
            //cameraPosition = distance * new Vector3((float)Math.Sin(angle), 0, (float)Math.Cos(angle));
            //view = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 0, 0), Vector3.UnitY);

            base.Update(gameTime);
        }

        public void Firebullet()
        {
            chainGun.Play();
            Box bullet = new Box(player.shipColBox.Position, 1f, 1f, 1f, 1000f);
            bullet.LinearVelocity = camera.ChaseDirection * 100;
            space.Add(bullet);
            EntityModel model = new EntityModel(bullet, Content.Load<Model>("Models/cube"), Matrix.Identity, this);
            Components.Add(model);
            bullet.Tag = model;

        }

        /// <summary>
        /// Update the values to be chased by the camera
        /// </summary>
        private void UpdateCameraChaseTarget()
        {
            if (cameraModeChase == true)
            {
                camera.ChasePosition = player.shipColBox.Position;
                camera.ChaseDirection = player.shipColBox.OrientationMatrix.Forward;
                camera.Up = player.shipColBox.OrientationMatrix.Up;
            }
            //else cameraModeChase = false;

            if (cameraMode3rdPerson == true)
            {
                cameraModeChase = false;
                camera.Position = player.shipColBox.Position;
                //camera.ChaseDirection = player.shipColBox.OrientationMatrix.Forward;
                camera.Up = player.shipColBox.OrientationMatrix.Up;

            }
        }

        /// <summary>
        /// Draws the ship and ground.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {

            //GraphicsDevice device = graphics.GraphicsDevice;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //RasterizerState rasterizerState = new RasterizerState();
            //rasterizerState.FillMode = FillMode.Solid;
            //GraphicsDevice.RasterizerState = rasterizerState;

            RasterizerState originalRasterizerState = graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            graphics.GraphicsDevice.RasterizerState = rasterizerState;

            skybox.Draw(camera.View, camera.Projection, cameraPosition);


            if (debug == true)
            {
                debugDrawer.LightingEnabled = false;
                debugDrawer.VertexColorEnabled = true;
                debugDrawer.World = Matrix.Identity;
                debugDrawer.View = camera.View;
                debugDrawer.Projection = camera.Projection;

                debugBoundingBoxDrawer.Draw(debugDrawer, space);

                spriteBatch.Begin();

                spriteBatch.DrawString(font, "Ship Pos: " + player.shipColBox.Position, new Vector2(10, 10), Color.Black);

                spriteBatch.End();

            }

            base.Draw(gameTime);

            if (cameraMode3rdPerson == true)
            {

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                DrawScenery();

                spriteBatch.End();

            }





            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;


        }

        private void DrawScenery()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            spriteBatch.Draw(cockpit, screenRectangle, Color.White);
        }


        ///// <summary>
        ///// Simple model drawing method. The interesting part here is that
        ///// the view and projection matrices are taken from the camera object.
        ///// </summary>        
        //private void DrawModel(Model model, Matrix world)
        //{
        //    Matrix[] transforms = new Matrix[model.Bones.Count];
        //    model.CopyAbsoluteBoneTransformsTo(transforms);

        //    foreach (ModelMesh mesh in model.Meshes)
        //    {
        //        foreach (BasicEffect effect in mesh.Effects)
        //        {
        //            effect.EnableDefaultLighting();
        //            effect.World = transforms[mesh.ParentBone.Index] * world;

        //            // Use the matrices provided by the chase camera
        //            effect.View = camera.View;
        //            effect.Projection = camera.Projection;
        //        }
        //        mesh.Draw();
        //    }
        //}
        #endregion
    }
}
