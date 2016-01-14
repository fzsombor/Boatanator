using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boatanator
{
    class Islands
    {

        VertexBuffer vb;
        IndexBuffer ib;
        BasicEffect effect;
        Matrix local;
        Matrix[] worlds = new Matrix[]
        {
            Matrix.CreateScale(283) * Matrix.CreateTranslation(50, 0, 50),
            Matrix.CreateScale(1000) * Matrix.CreateTranslation(-1100, 0, 500),
            Matrix.CreateScale(500) * Matrix.CreateTranslation(-600, 0, -600),
            Matrix.CreateScale(150) * Matrix.CreateTranslation(100, 0, -150)
        };
        RasterizerState RasterizerState = new RasterizerState
        { 
            CullMode = CullMode.CullClockwiseFace,
        //FillMode = FillMode.WireFrame
        };



        public void Load(ContentManager content, GraphicsDevice device)
        {
            var hmTex = content.Load<Texture2D>("islandHeight");
            int w = hmTex.Width;
            int h = hmTex.Height;
            local = Matrix.CreateScale(1f / (w - 1), 1f / 1024, 1f / (h - 1)) * Matrix.CreateTranslation(0, -0.01f, 0);
            var hm = new uint[w * h];
            hmTex.GetData(hm);
            vb = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, w * h, BufferUsage.WriteOnly);
            var data = new VertexPositionNormalTexture[w * h];
            for (int j = 0; j < h; j++)
            {

                for (int i = 0; i < w; i++)
                {
                    data[j * w + i] = new VertexPositionNormalTexture(
                        new Vector3(i, (byte)(hm[j * w + i] >> 8), j),
                        new Vector3(0, 1, 0),
                        new Vector2(i / (w - 1f), j / (h - 1f)));
                }
            }

            for (int j = 1; j < h - 1; j++)
            {

                for (int i = 1; i < w - 1; i++)
                {
                    var v = data[j * w + i];
                    var p1 = data[j * w + i + 1].Position;
                    var p2 = data[(j + 1) * w + i].Position;
                    var p3 = data[j * w + i - 1].Position;
                    var p4 = data[(j - 1) * w + i].Position;
                    var n = Vector3.Normalize(-Vector3.Cross(p1 - p3, p2 - p4));
                    data[j * w + i] = new VertexPositionNormalTexture(v.Position, n, v.TextureCoordinate);
                }
            }

            vb.SetData(data);

            var ibData = new List<int>();
            for (int j = 0; j < h - 1; j++)
            {

                for (int i = 0; i < w; i++)
                {
                    ibData.Add(j * w + i);
                    ibData.Add(j * w + i + w);
                }
                ibData.Add(ibData.Last());
                ibData.Add((j + 1) * w);
            }

            ibData.Remove(ibData.Count - 1);
            ibData.Remove(ibData.Count - 1);

            ib = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, ibData.Count, BufferUsage.WriteOnly);
            ib.SetData(ibData.ToArray());

            effect = new BasicEffect(device);
            effect.TextureEnabled = true;
            effect.Texture = content.Load<Texture2D>("islandColor");
            effect.LightingEnabled = true;
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.Direction = new Vector3(0, -1, 0);
            effect.DirectionalLight0.DiffuseColor = new Vector3(1, 1, 1);
            effect.DirectionalLight0.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);
            effect.FogEnabled = true;
            effect.FogColor = new Vector3(0.5f, 0.7f, 0.8f);
            effect.FogStart = 700;
            effect.FogEnd = 1000;
        }

        public void Draw(Camera cam)
        {
            var device = vb.GraphicsDevice;
            effect.CurrentTechnique.Passes[0].Apply();
            effect.World = local;
            effect.View = cam.View;
            effect.Projection = cam.Projection;
            device.SetVertexBuffer(vb);
            device.Indices = ib;
            var oldRS = device.RasterizerState;
            device.RasterizerState = RasterizerState;
            foreach(var world in worlds)
            {
                effect.World = local * world;
                effect.CurrentTechnique.Passes[0].Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, vb.VertexCount, 0, ib.IndexCount - 2);
            }

            device.RasterizerState = oldRS;

        }
    }
}
