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

        Matrix viewMatrix;
        Matrix projectionMatrix;
        float scale = 1.0f;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        Vector3 translation = new Vector3(-50000.0f, 0.0f, 50000.0f);

        Ship shhip;
        private GraphicsDevice device;
        private Texture2D terrainTexture; //greyscale heightmap image of the terrain
        private float textureScale;
        private float[,] heights; // 2D Array to store height values read from height map

        public Terrain(GraphicsDevice graphicsDevice, Texture2D heightMap, Texture2D terrainTexture,
                       float textureScale, int terrainWidth, int terrainHeight, float heightScale)
        {
            device = graphicsDevice;
            this.terrainTexture = terrainTexture;
            this.textureScale = textureScale;

            ReadHeightMap(heightMap, terrainWidth, terrainHeight, heightScale);

            BuildVertexBuffer(terrainWidth, terrainHeight, heightScale);

            BuildIndexBuffer(terrainWidth, terrainHeight);
        }



        // looks at each pixel of the height map and translates its color value 
        // into the heights array. uses min and max values to describe the lowest and highest points 
        // in the height data taken from the height map. 

        private void ReadHeightMap(Texture2D heightMap, int terrainWidth, int terrainHeight, float heightScale)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            //2D array to store hieghts of each vertex in the terrain
            heights = new float[terrainWidth, terrainHeight];

            // array to store color data from the heightMap
            Color[] heightMapData = new Color[
                heightMap.Width * heightMap.Height];

            // Loops through the heightMap and reads the value of the red component
            // then divides this value by 225 and stores it in the heights array
            // this gives a value between 0 and 1, o being the lowest point and 1 the highest
            heightMap.GetData(heightMapData);
            for (int x = 0; x < terrainWidth; x++)
                for (int z = 0; z < terrainHeight; z++)
                {
                    byte height = heightMapData[x + z * terrainWidth].R;
                    heights[x, z] = (float)height / 255f;

                    max = MathHelper.Max(max, heights[x, z]);
                    min = MathHelper.Min(min, heights[x, z]);
                }

            // Loops through the height array and subtracts the min value from each height
            // then divides by the range before multiplying ny the heightscale 
            // this places the lowest point in the terrain at 0 on the Y axis
            float range = (max - min);

            for (int x = 0; x < terrainWidth; x++)
                for (int z = 0; z < terrainHeight; z++)
                {
                    heights[x, z] =
                        ((heights[x, z] - min) / range) * heightScale;
                }
        }




        //Creates a grid of vertices and stores then in the vertex buffer
        private void BuildVertexBuffer(int width, int height, float heightScale)
        {

            //Empty array of VertexPositionNormalTexture objects then loops throught
            //the width and height of the terrain creating a VertexPositionNormalTexture for each 
            // vertex
            VertexPositionNormalTexture[] vertices =
                new VertexPositionNormalTexture[width * height];

            for (int x = 0; x < width; x++)
                for (int z = 0; z < height; z++)
                {
                    vertices[x + (z * width)].Position =
                      new Vector3(x, heights[x, z], z);
                    vertices[x + (z * width)].TextureCoordinate =
                        new Vector2((float)x / textureScale, (float)z / textureScale);
                }
            //Adds values from the VertexPositionNormalTexture array to the vertex buffer
            vertexBuffer = new VertexBuffer(
                device,
                typeof(VertexPositionNormalTexture),
                vertices.Length,
                BufferUsage.WriteOnly);

            vertexBuffer.SetData(vertices);
        }

        //creats the indexBuffer and uses the data to construct triangles
        //loops through array stopping one vertex from the end in each direction
        //and calculate the ammount of indices needed to create the terrain,
        // ad stores these in the index buffer
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

        public void Draw(ChaseCamera camera, Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["Technique1"];
            effect.Parameters["terrainTexture1"].SetValue(terrainTexture);
            //effect.Parameters["World"].SetValue(world); 
            effect.Parameters["World"].SetValue(Matrix.CreateScale(1000.0f) * Matrix.CreateRotationY(90.0f) * Matrix.CreateTranslation(translation));
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);

            {



                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    pass.Apply();
                device.SetVertexBuffer(vertexBuffer);
                device.Indices = indexBuffer;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
            }
        }
    }
}
