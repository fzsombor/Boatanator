using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boatanator
{
    class Sky
    {
        Texture2D tex;
        Sphere sphere;
        public void Load(GraphicsDevice decive, ContentManager content)
        {
            tex = content.Load<Texture2D>("skyhalf");
            sphere = Sphere.CreateHalf(decive, 15, 7, v=> new VertexPositionTexture(v.Position, new Vector2(v.TextureCoordinate.X , 1-v.TextureCoordinate.Y)));
            //sphere = Sphere.Create(decive);
            sphere.effect.Texture = tex;
            sphere.effect.TextureEnabled = true;
            
           
        }

        public void Draw(Camera cam)
        {
            sphere.effect.World = Matrix.CreateScale(1000)*Matrix.CreateTranslation(cam.Position.X, -1, cam.Position.Z);
            sphere.Draw(cam.View, cam.Projection);
        }
    }
}
