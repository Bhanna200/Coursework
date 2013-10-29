using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Coursework
{
    public struct VertexPositionNormalColored
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public static int SizeInBytes = 7 * 4;
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
              (
                  new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                  new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                  new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
              );
    }


    class Terrain
    {
        GraphicsDevice graphicsDevice;

        Texture2D heightMap;

        VertexPositionNormalColored[] vertices;

        int terrainWidth;
        int terrainLength;
        float[,] heightData;
        int[] indices;

        VertexBuffer terrainVertexBuffer;
        IndexBuffer terrainIndexBuffer;

        public Terrain(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void LoadVertices(Texture2D heightMap)
        {
            this.heightMap = heightMap;
            LoadHeightData();
            SetUpTerrainVertices();
            SetUpTerrainIndices();
            CalculateNormals();
            CopyToTerrainBuffers();
        }

        public void LoadHeightData()
        {
            float minimumHeight = float.MaxValue;
            float maximumHeight = float.MinValue;

            terrainWidth = heightMap.Width;
            terrainLength = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainLength];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainLength];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R;
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                    heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 30.0f;
        }


        private void SetUpTerrainVertices()
        {
            vertices = new VertexPositionNormalColored[terrainWidth * terrainLength];

            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainLength; y++)
                {
                    vertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);

                    if (heightData[x, y] < 6)
                        vertices[x + y * terrainWidth].Color = Color.Blue;
                    else if (heightData[x, y] < 15)
                        vertices[x + y * terrainWidth].Color = Color.Green;
                    else if (heightData[x, y] < 25)
                        vertices[x + y * terrainWidth].Color = Color.Brown;
                    else
                        vertices[x + y * terrainWidth].Color = Color.White;
                }
            }
        }

        private void SetUpTerrainIndices()
        {
            indices = new int[(terrainWidth - 1) * (terrainLength - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainLength - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }


        private void CalculateNormals()
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();
        }

        private void CopyToTerrainBuffers()
        {
            terrainVertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalColored.VertexDeclaration, vertices.Length,
                BufferUsage.WriteOnly);
            terrainVertexBuffer.SetData(vertices);

            terrainIndexBuffer = new IndexBuffer(graphicsDevice, typeof(int), indices.Length, BufferUsage.WriteOnly);
            terrainIndexBuffer.SetData(indices);
        }

        public void Draw()
        {
            graphicsDevice.Indices = terrainIndexBuffer;
            graphicsDevice.SetVertexBuffer(terrainVertexBuffer);
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);
        }
    }
}
