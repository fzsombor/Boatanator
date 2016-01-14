using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boatanator.Content
{
    class Heli : RigidBody
    {
        static Model model;
        static Matrix local;
        Matrix world;
        //Verlet verlet = new Verlet();
        public string nickname = "haha";
        public string host = "";
        public Color color = Color.DeepSkyBlue;
        public float roll = 0;  //-15 - 15
        public float pitch = 0; //-15 - 15
        public float collective = 0.5f;

        public Heli()
        {
            float L = 2, W = 1, H = 1, D = 0.5f, HH = 50;
            var vs = new Verlet[]
            {
                new Verlet(L,HH+0,W),
                new Verlet(L,HH+0,-W),
                new Verlet(-L,HH+0,-W), //tail rotor push (e) 3
                new Verlet(-L,HH+0, W), //tail rotor pull (q) 5
                new Verlet(-0.4f,HH+H,0), //rotor 4
                new Verlet(-L,HH-D,0)

            };
            Init(vs);
        }

        public void Load(ContentManager content)
        {
            model = content.Load<Model>("Black Hawk uh-60");
            local = Matrix.CreateScale(0.3f)*Matrix.CreateRotationY(MathHelper.Pi);
        }

        public void Draw(Camera cam)
        {

            world =  Matrix.CreateWorld(Position, Direction, Up);
            model.Draw(local*world, cam.View, cam.Projection);
        }

        public void Step(bool w, bool s, bool a, bool d, bool q, bool e, bool o, bool l)
        {
            
            var dir = Direction;
            var up = Up;
            var right = Vector3.Cross(dir, Up);
            float power = 200;
            
            ///////////////////////////////////////
            // Collective
            ///////////////////////////////////////
            if(collective<1.0f & collective>0)
            {
                if (o)
                    collective += 0.005f;
                if (l)
                    collective -= 0.005f;
            }

            ///////////////////////////////////////
            // Gravity
            ///////////////////////////////////////
            float g = 8;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].A = new Vector3(0, -g, 0);
            }

            ///////////////////////////////////////
            // Lift
            ///////////////////////////////////////

          //  vertices[4].A += new Vector3(0, power*collective, 0);

           // for (int i = 0; i < 4; i++)
           // {
           //     if (vertices[i].Pos.Y < 0)
           //     vertices[i].A += new Vector3(0, Math.Max(0, Math.Min(20, (1.3f-vertices[i].Pos.Y) * g)), 0);
           // }

            ///////////////////////////////////////
            // Friction
            ///////////////////////////////////////
            for (int i = 0; i < 4; i++)
            {
                if (vertices[i].Pos.Y < 0)
                {
                    vertices[i].AddSqFriction(dir, 0.001f);
                    vertices[i].AddSqFriction(right, 0.1f);
                    vertices[i].AddSqFriction(up, 1f);
                }
            }



            ///////////////////////////////////////
            // Control
            ///////////////////////////////////////

           
            var strength = collective * power + 0.05f;

            

            float accUp = (float)Math.Cos(roll) * (float)Math.Cos(pitch) * strength;
            float accRight = (float)Math.Sin(pitch) * strength;
            float accForward = (float)Math.Sin(roll) * strength;

            Vector3 accRotor = new Vector3(accForward, accUp, accRight); // it must be parallel to the cross of dir and right (up)
            Vector4 v4temp = Vector4.Transform(new Vector4(accRotor,0), world);
            Vector3 transformedAccRotor = new Vector3(v4temp.X, v4temp.Y, v4temp.Z) * collective;
            //Vector3.
            //dir += accforward*Vector3.Normalize(dir);
            //vertices[4].A += (accup*power) * Vector3.Cross(dir, right)  ;
            
            Vector3 tailAcc = Vector3.Zero;
            Vector3 acc = Vector3.Zero;



            
            

            if (q)
                tailAcc = Vector3.Normalize(dir + right)*10;
            if (e)
                tailAcc = Vector3.Normalize(dir - right)*10;
            if (pitch < 15 & w)
                pitch += 0.005f;
            if (pitch > -15 & s)
                pitch -= 0.005f;
            if (roll > -15 & a)
                roll -= 0.005f;
            if (roll < 15 & d)
                roll += 0.005f;
            


            
            //float strength = 20;
            float small = 2;

            

            if (vertices[5].Pos.Y < 0)
            {
                if (w)
                {
                    if (d)
                    {
                        acc = Vector3.Normalize(dir - right) * strength;
                    }
                    else if (a)
                    {
                        acc = Vector3.Normalize(dir + right) * strength;
                    }
                    else
                    {
                        acc = dir * strength;
                    }
                }
                else
                {
                    if (d)
                    {
                        acc = -right * small;
                    }
                    else if (a)
                    {
                        acc = right * small;
                    }
                    else if (s)
                    {
                        acc = -dir * strength / 2;
                    }
                }
                
            }
            vertices[4].A += transformedAccRotor;    //add forces to the vertices
            vertices[5].A += tailAcc;
            


            base.Step();
        }

        public Vector3 Position { get { return (vertices[0].Pos + vertices[2].Pos) * 0.5f; } }
        public Vector3 Direction { get { return Vector3.Normalize(vertices[0].Pos - vertices[3].Pos); } }
        public Vector3 Up { get { return Vector3.Normalize(2*vertices[4].Pos - vertices[2].Pos - vertices[0].Pos); } }


        public int Serialize(byte[] buffer, int p)
        {
            int op = p;
            foreach (var v in vertices)
	        {
		       p += v.Serialize(buffer, p);
	        }
            int offset = p;
            p += 1;
            p += Encoding.UTF8.GetBytes(nickname, 0, nickname.Length, buffer, p);
            buffer[offset] = (byte)(p - offset - 1);
            buffer[p++] = color.R;
            buffer[p++] = color.G;
            buffer[p++] = color.B;
            return p - op;

            
        }
        public int Deserialize(byte[] buffer, int p)
        {
            int op = p;
            foreach (var v in vertices)
            {
               p += v.Deserialize(buffer, p);
            }
            byte len = buffer[p++];
            nickname =  Encoding.UTF8.GetString(buffer, p, len);
            p += len;
            color.R = buffer[p++];
            color.G = buffer[p++];
            color.B = buffer[p++];
            return p - op;


        }
    }
}
