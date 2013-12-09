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
using BEPUphysics.DataStructures;
using BEPUphysics.MathExtensions;
using BEPUphysics.Collidables;

using BEPUphysics;
using BEPUphysicsDrawer.Models;
using BEPUphysics.Entities.Prefabs;
using BEPUphysicsDrawer.Lines;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionRuleManagement;

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

        public int score = 0;

        KeyboardState lastKeyboardState = new KeyboardState();
        KeyboardState currentKeyboardState = new KeyboardState();

        public Player player;
        public ChaseCamera camera;
        public Enemy enemy;
        public Box bullet;

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
        bool soundFx = true;
        bool playMusic = true;

        Skybox skybox;
        Vector3 cameraPosition;

        SoundEffect laser;
        protected Song music;

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
            camera.DesiredPositionOffset = new Vector3(0.0f, 10.0f, 10.0f);
            camera.LookAtOffset = new Vector3(0.0f, 3.0f, 0.0f);

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

            player = new Player(this);
            enemy = new Enemy(this);

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

            CubeModel = Content.Load<Model>("Models/bullet");

            cockpit = Content.Load<Texture2D>("Textures/cockpit");

            skybox = new Skybox("Skyboxes/Sunset", Content);

            laser = Content.Load<SoundEffect>("Audio/Laser");

            music = Content.Load<Song>("Audio/background");
            if (playMusic == true)
            {
                MediaPlayer.Play(music);
            }

            if (playMusic == false)
            {
                MediaPlayer.Stop();
            }

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
            currentKeyboardState = Keyboard.GetState();

            // Exit when the Escape key or Back button is pressed
            if (currentKeyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // Pressing the A button or key toggles the spring behavior on and off
            if (lastKeyboardState.IsKeyUp(Keys.A) &&
                (currentKeyboardState.IsKeyDown(Keys.A)))
            {
                cameraSpringEnabled = !cameraSpringEnabled;
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


            if (lastKeyboardState.IsKeyUp(Keys.Space) &&
                (currentKeyboardState.IsKeyDown(Keys.Space)))
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

            if (lastKeyboardState.IsKeyDown(Keys.Delete) &&
                (currentKeyboardState.IsKeyUp(Keys.Delete)))
            {
                soundFx = false;
            }

            if (lastKeyboardState.IsKeyDown(Keys.Insert) &&
                (currentKeyboardState.IsKeyUp(Keys.Insert)))
            {
                soundFx = true;
            }

            if (lastKeyboardState.IsKeyDown(Keys.F11) &&
                (currentKeyboardState.IsKeyUp(Keys.F11)))
            {
                playMusic = false;
                MediaPlayer.Stop();
            }

            if (lastKeyboardState.IsKeyDown(Keys.F12) &&
                (currentKeyboardState.IsKeyUp(Keys.F12)))
            {
                playMusic = true;
                MediaPlayer.Play(music);
            }

            base.Update(gameTime);
        }

        public void Firebullet()
        {
            if (soundFx == true)
            {
                laser.Play();
            }
            bullet = new Box(player.shipColBox.Position, 0.3f, 0.3f, 0.3f, 1f);
            bullet.LinearVelocity = camera.ChaseDirection * 300;
            //bullet.Mass = 1;
            space.Add(bullet);
            EntityModel model = new EntityModel(bullet, Content.Load<Model>("Models/bullet"), Matrix.Identity * Matrix.CreateScale(0.3f), this);
            Components.Add(model);
            bullet.Tag = model;
            CollisionRules.AddRule(player.shipColBox, bullet, CollisionRule.NoBroadPhase);
            bullet.CollisionInformation.Events.InitialCollisionDetected += BulletCollision;
        }

        void BulletCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            var otherEntityInformation = other as EntityCollidable;
            if (otherEntityInformation != null)
            {             
                enemy.hit.Play();
                space.Remove(otherEntityInformation.Entity);
                Components.Remove((EntityModel)otherEntityInformation.Entity.Tag);
            }
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
                // cameraSpringEnabled = false;
                camera.Position = player.shipColBox.Position;
                //camera.ChaseDirection = player.shipColBox.OrientationMatrix.Forward;
                camera.Up = player.shipColBox.OrientationMatrix.Forward;
            }
        }

        /// <summary>
        /// Draws the ship and ground.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice device = graphics.GraphicsDevice;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            RasterizerState originalRasterizerState = graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            graphics.GraphicsDevice.RasterizerState = rasterizerState;

            skybox.Draw(camera.View, camera.Projection, cameraPosition);

            base.Draw(gameTime);

            Draw3rdPersonCamera();
            DrawDebug();
            ToggleSound();
            ToggleMusic();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        }

        private void DrawScenery()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            spriteBatch.Draw(cockpit, screenRectangle, Color.White);
        }

        private void Draw3rdPersonCamera()
        {
            if (cameraMode3rdPerson == true)
            {

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                DrawScenery();
                spriteBatch.End();
            }
        }

        private void ToggleSound()
        {
            if (soundFx == true && lastKeyboardState.IsKeyDown(Keys.Delete))
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, "SoundFX Off: ", new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2), Color.GreenYellow);
                spriteBatch.End();
            }

            if (soundFx == false && lastKeyboardState.IsKeyDown(Keys.Insert))
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, "SoundFX On: ", new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2), Color.GreenYellow);
                spriteBatch.End();
            }
        }

        private void ToggleMusic()
        {
            if (playMusic == true && lastKeyboardState.IsKeyDown(Keys.F11))
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, "Music Off: ", new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2), Color.GreenYellow);
                spriteBatch.End();
            }

            if (playMusic == false && lastKeyboardState.IsKeyDown(Keys.F12))
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, "Music On: ", new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2), Color.GreenYellow);
                spriteBatch.End();
            }
        }

        private void DrawDebug()
        {
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
        }

        ///// <summary>
        /// Simple model drawing method. The interesting part here is that
        /// the view and projection matrices are taken from the camera object.
        /// </summary>        
        private void DrawModel(Model model, Matrix world)
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
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                mesh.Draw();
            }
        }
        #endregion
    }
}
