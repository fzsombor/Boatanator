using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boatanator
{
    class Verlet
    {
        public const float dT = 1f/60;
        public Vector3 Pos, pPos, A, Friction;

        public Verlet(float x, float y, float z)
        {
            Pos = pPos = new Vector3(x, y, z);
        }
        public void Step()
        {
            float C = 1;
            var dPos = Pos - pPos + dT * dT * A;
            Vector3 f = dT * dT * Friction;
            if (Vector3.Dot(dPos,dPos+f)<0)
            {
                C = -dPos.LengthSquared() / Vector3.Dot(dPos, f);
            }
            var newPos = Pos + dPos + C * f;
           
            pPos = Pos;
            Pos = newPos;
            Friction = Vector3.Zero;
        }

        public Vector3 Velocity{get {return (Pos-pPos) / dT;}}
        public void AddSqFriction(Vector3 fDir, float fC)
        {
            float length = Vector3.Dot(Velocity, fDir);
            Friction -= fDir*length*Math.Abs(length)*fC;
        }



        public int Serialize(byte[] buffer, int p)
        {
            int op = p;
            BitConverter.GetBytes(Pos.X).CopyTo(buffer, p); p += 4;
            BitConverter.GetBytes(Pos.Y).CopyTo(buffer, p); p += 4;
            BitConverter.GetBytes(Pos.Z).CopyTo(buffer, p); p += 4;
            BitConverter.GetBytes(pPos.X).CopyTo(buffer, p); p += 4;
            BitConverter.GetBytes(pPos.Y).CopyTo(buffer, p); p += 4;
            BitConverter.GetBytes(pPos.Z).CopyTo(buffer, p); p += 4;
            return p - op;
        }
        public int Deserialize(byte[] buffer, int p)
        {
            int op = p;
            Pos.X = BitConverter.ToSingle(buffer, p); p+=4;
            Pos.Y = BitConverter.ToSingle(buffer, p); p+=4;
            Pos.Z = BitConverter.ToSingle(buffer, p); p+=4;
            pPos.X = BitConverter.ToSingle(buffer, p); p+=4;
            pPos.Y = BitConverter.ToSingle(buffer, p); p+=4;
            pPos.Z = BitConverter.ToSingle(buffer, p); p+=4;
            
            return p - op;
        }
    }
}
