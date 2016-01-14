using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boatanator
{
    class RigidBody
    {
        
        public Verlet[] vertices;
        float[,] Lengths;
        public void Init(Verlet[] vs)
        {
            vertices = vs;
            Lengths = new float[vs.Length, vs.Length];
            for (int i = 0; i < vs.Length; i++)
            {
                for (int j = 0; j < vs.Length; j++)
                {
                    Lengths[i, j] = Vector3.Distance(vs[i].Pos, vs[j].Pos);
                }
            }
        }

        public void Step()
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Step();
            }
            ApplyConstraints();
        }


        public void ApplyConstraints()
        {
            for (int i = 0; i < vertices.Length-1; i++)
            {
                for (int j = i+1; j < vertices.Length; j++)
                {
                    ApplyConstraints(ref vertices[i].Pos, ref vertices[j].Pos, Lengths[i, j]);
                }
            }
        }

        void ApplyConstraints(ref Vector3 p1, ref Vector3 p2, float distance)
        {
            Vector3 dPos = p2 - p1;
            float dLength = dPos.Length();
            Vector3 correction = (dLength - distance) / dLength * 0.5f * dPos;
            p1 += correction;
            p2 -= correction;
        }


    }

}
