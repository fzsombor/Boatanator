using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boatanator
{
    class Water
    {

        VertexBuffer vb;
        Effect effect;
        Matrix local;
        Texture2D normalMap;

        public void Load(GraphicsDevice device, ContentManager content)
        {
            vb = new VertexBuffer(device, new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)), 4, BufferUsage.WriteOnly);
            vb.SetData(new Vector3[]
            {new Vector3(-1, 0, 1),
            new Vector3(-1, 0, -1),
            new Vector3(1, 0, 1),
            new Vector3(1, 0, -1)});
            local = Matrix.CreateScale(1000);
            effect = content.Load<Effect>("Water");
            normalMap = content.Load<Texture2D>("wave2");
            
        }

        public void Draw(Camera cam, GameTime gt, Texture2D reflectionMap)
        {
            vb.GraphicsDevice.SetVertexBuffer(vb);
            effect.Parameters["WVP"].SetValue(local * Matrix.CreateTranslation(cam.Position.X, 0, cam.Position.Z) * cam.View * cam.Projection);
            effect.Parameters["World"].SetValue(local * Matrix.CreateTranslation(cam.Position.X, 0, cam.Position.Z));
            effect.Parameters["CamPos"].SetValue(cam.Position);
            effect.Parameters["SunDir"].SetValue(Vector3.Normalize(new Vector3(-0.2f, -0.5f, -1)));
            effect.Parameters["NormMap"].SetValue(normalMap);
            effect.Parameters["ReflectionMap"].SetValue(reflectionMap);
            effect.Parameters["T"].SetValue((float)gt.TotalGameTime.TotalSeconds);
            effect.CurrentTechnique.Passes[0].Apply();
            vb.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }
    }
}
