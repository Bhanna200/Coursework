using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework
{
    class Terrain
    {       
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        private GraphicsDevice device;
        private Texture2D terrainTexture;
        private float textureScale;
        private float[,] heights;
        
        public Terrain(GraphicsDevice graphicsDevice, Texture2D heightMap, Texture2D terrainTexture, float textureScale, int terrainWidth, int terrainHeight, float heightScale)
        {
            device = graphicsDevice;
            this.terrainTexture = terrainTexture;
            this.textureScale = textureScale;

            ReadHeightMap(heightMap, terrainWidth, terrainHeight, heightScale);

            BuildVertexBuffer(terrainWidth, terrainHeight, heightScale);

            BuildIndexBuffer(terrainWidth, terrainHeight);
        }
       
        private void ReadHeightMap(Texture2D heightMap, int terrainWidth, int terrainHeight, float heightScale)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            heights = new float[terrainWidth, terrainHeight];

            Color[] heightMapData = new Color[heightMap.Width * heightMap.Height];

            heightMap.GetData(heightMapData);
            for (int x = 0; x < terrainWidth; x++)
                for (int z = 0; z < terrainHeight; z++)
                {
                    byte height = heightMapData[x + z * terrainWidth].R;
                    heights[x, z] = (float)height / 255f;

                    max = MathHelper.Max(max, heights[x, z]);
                    min = MathHelper.Min(min, heights[x, z]);
                }

            float range = (max - min);

            for (int x = 0; x < terrainWidth; x++)
                for (int z = 0; z < terrainHeight; z++)
                {
                    heights[x, z] =
                        ((heights[x, z] - min) / range) * heightScale;
                }
        }
       
        private void BuildVertexBuffer(int width, int height, float heightScale)
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[width * height];

            for (int x = 0; x < width; x++)
                for (int z = 0; z < height; z++)
                {
                    vertices[x + (z * width)].Position =
                      new Vector3(x, heights[x, z], z);
                    vertices[x + (z * width)].TextureCoordinate =
                        new Vector2((float)x / textureScale, (float)z / textureScale);
                }

            vertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);

            vertexBuffer.SetData(vertices);
        }
        

        
        private void BuildIndexBuffer(int width, int height)
        {
            int indexCount = (width - 1) * (height - 1) * 6;
            short[] indices = new short[indexCount];
            int counter = 0;

            for (short z = 0; z < height - 1; z++)
                for (short x = 0; x < height - 1; x++)
                {
                    short upperLeft = (short)(x + (z * width));
                    short upperRight = (short)(upperLeft + 1);
                    short lowerLeft = (short)(upperLeft + width);
                    short lowerRight = (short)(upperLeft + width + 1);

                    indices[counter++] = upperLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;
                    indices[counter++] = upperLeft;
                    indices[counter++] = upperRight;
                    indices[counter++] = lowerRight;
                }

            indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }
        
        public void Draw(Camera camera, Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["Technique1"];
            effect.Parameters["terrainTexture1"].SetValue(terrainTexture);
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(vertexBuffer);
                device.Indices = indexBuffer;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
            }
        }
    }
}
