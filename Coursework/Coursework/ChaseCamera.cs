using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework
{
    public class ChaseCamera
    {
        private Vector3 position;
        private Vector3 target;
        public Matrix viewMatrix, projectionMatrix;

        private float yaw, pitch, roll;
        private float speed;
        private Matrix cameraRotation;

        private Vector3 desiredPosition;
        private Vector3 desiredTarget;
        private Vector3 offsetDistance;



        public Player player;

        public enum CameraMode
        {
            free = 0,
            chase = 1,
            orbit = 2
        }
        public CameraMode currentCameraMode = CameraMode.free;


        public ChaseCamera()
        {
            ResetCamera();

        }

        public void ResetCamera()
        {
            desiredPosition = position;
            desiredTarget = target;

            offsetDistance = new Vector3(0, 0, 10);


            position = new Vector3(0, 0, 50);
            target = new Vector3();
            //target = player.shipPos;

            yaw = 0.0f;
            pitch = 0.0f;
            roll = 0.0f;

            speed = .3f;

            cameraRotation = Matrix.Identity;

            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), 16 / 9, .5f, 500f);
        }

        public void Update(Matrix chasedObjectsWorld)
        {
            HandleInput();
            UpdateViewMatrix(chasedObjectsWorld);
        }

        private void UpdateViewMatrix(Matrix chasedObjectsWorld)
        {
            switch (currentCameraMode)
            {
                case CameraMode.free:
                    cameraRotation.Forward.Normalize();
                    cameraRotation.Up.Normalize();
                    cameraRotation.Right.Normalize();

                    cameraRotation *= Matrix.CreateFromAxisAngle(cameraRotation.Right, pitch);
                    cameraRotation *= Matrix.CreateFromAxisAngle(cameraRotation.Up, yaw);
                    cameraRotation *= Matrix.CreateFromAxisAngle(cameraRotation.Forward, roll);

                    yaw = 0.0f;
                    pitch = 0.0f;
                    roll = 0.0f;

                    target = position + cameraRotation.Forward;

                    break;

                case CameraMode.chase:

                    cameraRotation.Forward.Normalize();
                    chasedObjectsWorld.Right.Normalize();
                    chasedObjectsWorld.Up.Normalize();

                    cameraRotation = Matrix.CreateFromAxisAngle(cameraRotation.Forward, roll);

                    desiredTarget = chasedObjectsWorld.Translation;
                    target = desiredTarget;
                    target += chasedObjectsWorld.Right * yaw;
                    target += chasedObjectsWorld.Up * pitch;

                    desiredPosition = Vector3.Transform(offsetDistance, chasedObjectsWorld);
                    position = Vector3.SmoothStep(position, desiredPosition, .15f);

                    yaw = MathHelper.SmoothStep(yaw, 0f, .1f);
                    pitch = MathHelper.SmoothStep(pitch, 0f, .1f);
                    roll = MathHelper.SmoothStep(roll, 0f, .2f);

                    break;

                case CameraMode.orbit:

                    cameraRotation.Forward.Normalize();

                    cameraRotation = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw) * Matrix.CreateFromAxisAngle(cameraRotation.Forward, roll);

                    desiredPosition = Vector3.Transform(offsetDistance, cameraRotation);
                    desiredPosition += chasedObjectsWorld.Translation;
                    position = desiredPosition;

                    target = chasedObjectsWorld.Translation;

                    roll = MathHelper.SmoothStep(roll, 0f, .2f);


                    break;

                //viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
            }
            viewMatrix = Matrix.CreateLookAt(position, target, cameraRotation.Up);
        }

        private void HandleInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (currentCameraMode == CameraMode.free)
            {
                if (keyboardState.IsKeyDown(Keys.J))
                {
                    yaw += .02f;
                }
                if (keyboardState.IsKeyDown(Keys.L))
                {
                    yaw += -.02f;
                }
                if (keyboardState.IsKeyDown(Keys.I))
                {
                    pitch += -.02f;
                }
                if (keyboardState.IsKeyDown(Keys.K))
                {
                    pitch += .02f;
                }
                if (keyboardState.IsKeyDown(Keys.U))
                {
                    roll += -.02f;
                }
                if (keyboardState.IsKeyDown(Keys.O))
                {
                    roll += .02f;
                }

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    MoveCamera(cameraRotation.Forward);
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    MoveCamera(-cameraRotation.Forward);
                }
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    MoveCamera(-cameraRotation.Right);
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    MoveCamera(cameraRotation.Right);
                }
                if (keyboardState.IsKeyDown(Keys.E))
                {
                    MoveCamera(cameraRotation.Up);
                }
                if (keyboardState.IsKeyDown(Keys.Q))
                {
                    MoveCamera(-cameraRotation.Up);
                }
            }

            if (currentCameraMode == CameraMode.chase)
            {

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    MoveCamera(cameraRotation.Backward);

                }




            }
        }

        private void MoveCamera(Vector3 addedVector)
        {
            position += speed * addedVector;
        }

        public void SwitchCameraMode()
        {
            ResetCamera();

            currentCameraMode++;

            if ((int)currentCameraMode > 2)
            {
                currentCameraMode = 0;
            }
        }

    }
}
