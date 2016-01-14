using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boatanator
{
    class Camera
    {
        public Vector3 Position = new Vector3(10, 2, 10);
        public Vector3 Direction = new Vector3(1, 0, 0);
        public Vector3 Up = Vector3.Up; // 0 1 0
        public float NearPlane = 1;
        public float FarPlane = 1010;
        public float AspectRatio = 1;
        public float FOV = MathHelper.Pi / 3;


        public Vector3 Target 
        {
            get{return Position + Direction;}
            set { Direction = Vector3.Normalize( value - Position); }
        
        }
        public Vector3 Right { get { return Vector3.Normalize(Vector3.Cross(Direction, Up)); } }


        public Matrix View { get {  return Matrix.CreateLookAt(Position, Target, Up);  } }
        public Matrix Projection { get { return Matrix.CreatePerspectiveFieldOfView(FOV, AspectRatio, NearPlane, FarPlane); } }

        public Camera CreateReflectionCam()
        {
            return new Camera
            {
                Position = new Vector3(Position.X, -Position.Y, Position.Z),
                Direction = new Vector3(Direction.X, -Direction.Y, Direction.Z),
                Up = Up, // 0 1 0
                NearPlane = NearPlane,
                FarPlane = FarPlane,
                AspectRatio = AspectRatio,
                FOV = FOV
            };
        }
    }
}
