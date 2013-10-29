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

namespace Coursework
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;

        Effect effect;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        Terrain landscape;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 500;

            graphics.ApplyChanges();
            Window.Title = "GP3 Coursework";

            landscape = new Terrain(GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            device = GraphicsDevice;

            viewMatrix = Matrix.CreateLookAt(new Vector3(130, 30, -50), new Vector3(0, 0, -40), new Vector3(0, 1, 0));
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.3f, 1000.0f);

            effect = Content.Load<Effect>("Effects/Series4Effects");
            landscape.LoadVertices(Content.Load<Texture2D>("Maps/heightMap"));

        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 100.0f;
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            device.RasterizerState = rs;

            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawTerrain(viewMatrix);

            base.Draw(gameTime);
        }

        private void DrawTerrain(Matrix currentViewMatrix)
        {
            Matrix worldMatrix = Matrix.Identity;
            effect.Parameters["xWorld"].SetValue(worldMatrix);
            effect.Parameters["xView"].SetValue(currentViewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);

            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xAmbient"].SetValue(0.4f);
            effect.Parameters["xLightDirection"].SetValue(new Vector3(-0.5f, -1, -0.5f));
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                landscape.Draw();
            }
        }
    }
}
