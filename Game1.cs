using Boatanator.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Boatanator
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        RenderTarget2D reflectionMap;
        //Boat boat = new Boat();
        Heli heli = new Heli();
        Sky sky;
        Camera freeCam = new Camera();
        Camera camBoat = new Camera();
        Camera cam;
        Water water = new Water();
        Islands island = new Islands();

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        byte[] buffer = new byte[64 * 1024];
        TimeSpan lastSend = TimeSpan.Zero;
        IPEndPoint broadcast = new IPEndPoint(IPAddress.Broadcast, 12345);
        Dictionary<string, Heli> helis = new Dictionary<string, Heli>();
        IPAddress[] localAdresses;
        
        
        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            sky = new Sky();
            cam = camBoat;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Window.AllowUserResizing = true;
            Mouse.SetPosition(100, 100);
           
            Mouse.SetPosition(100, 100);
            socket.EnableBroadcast = true;
            localAdresses = Dns.GetHostAddresses(Dns.GetHostName());
            //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            socket.Bind(new IPEndPoint(IPAddress.Any, 12345));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            heli.Load(Content);
            sky.Load(GraphicsDevice, Content);
            water.Load(GraphicsDevice, Content);
            island.Load(Content, GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var ks = Keyboard.GetState();
            bool w = false, a = false, s = false, d = false, q = false, e = false, o = false, l = false;
            
         



            if(cam == freeCam)
            {

              
                if (ks.IsKeyDown(Keys.Tab))
                    cam = camBoat;

                if (ks.IsKeyDown(Keys.W))
                    cam.Position += cam.Direction;

                if (ks.IsKeyDown(Keys.S))
                    cam.Position -= cam.Direction;

                if (ks.IsKeyDown(Keys.D))
                    cam.Position += cam.Right;

                if (ks.IsKeyDown(Keys.A))
                    cam.Position -= cam.Right;
            }
            else
            {
                if (ks.IsKeyDown(Keys.Tab))
                    cam = freeCam;

                w = ks.IsKeyDown(Keys.W);
                a = ks.IsKeyDown(Keys.A);
                s = ks.IsKeyDown(Keys.S);
                d = ks.IsKeyDown(Keys.D);
                q = ks.IsKeyDown(Keys.Q);
                e = ks.IsKeyDown(Keys.E);
                o = ks.IsKeyDown(Keys.O);
                l = ks.IsKeyDown(Keys.L);
            }


            
            heli.Step(w,s,a,d,q,e,o,l);
            camBoat.Position = heli.Position - new Vector3(heli.Direction.X, 0, heli.Direction.Z)*10+new Vector3(0,4,0);
            camBoat.Target = heli.Position + new Vector3(0, 2, 0);
            //camBoat.Direction = boat.Position;
            
            var ms = Mouse.GetState();
            var pos = ms.Position;

            try { Mouse.SetPosition(100, 100); }
            catch { }

            
            var delta = pos - new Point(100, 100);
            var m = Matrix.CreateRotationY(-delta.X/200f)*Matrix.CreateFromAxisAngle(cam.Right, -delta.Y / 200f);
            cam.Direction = Vector3.Transform(cam.Direction, m);

            foreach (var b in helis.Values)
                b.Step(false, false, false, false, false, false, false, false);
                
       




            if (socket.Available > 0)
            {
                EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
               int len =  socket.ReceiveFrom(buffer, ref ep);
               if (!Array.Exists(localAdresses, x => x.Equals( ((IPEndPoint)ep).Address)))
               {
                   string key = ep.ToString();

                   Heli b;
                   if (!helis.TryGetValue(key, out b))
                       helis[key] = b = new Heli();
                   int p = 1 + b.Deserialize(buffer, 1);
                  
              }
                //byte cmd
                // n byte boat
                // m byte control
                //string name
                // color
                

            }

            if (lastSend + TimeSpan.FromMilliseconds(1000) < gameTime.TotalGameTime)
            {
                int packetLength = 0;
                buffer[0] = 0;
                packetLength =  1 + heli.Serialize(buffer, 1);
                buffer[packetLength++] = (byte)( w ? 1 : 0);
                buffer[packetLength++] = (byte)(a ? 1 : 0);
                buffer[packetLength++] = (byte)(s ? 1 : 0);
                buffer[packetLength++] = (byte)(d ? 1 : 0);

               



                int len = socket.SendTo(buffer, packetLength, SocketFlags.None, broadcast);
                lastSend = gameTime.TotalGameTime;
            }



            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if(Window.ClientBounds.Width != GraphicsDevice.Viewport.Width
                ||
                Window.ClientBounds.Height != GraphicsDevice.Viewport.Height)
            {
                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                graphics.ApplyChanges();
            }
            if(reflectionMap == null)
            {
                reflectionMap = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            }

            cam.AspectRatio = GraphicsDevice.Viewport.AspectRatio;
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.SetRenderTarget(reflectionMap);
            GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Red, 1, 0);
            var rCam = cam.CreateReflectionCam();

            var depthState = new DepthStencilState();
            depthState.DepthBufferEnable = false;
            
            sky.Draw(rCam);

            depthState.DepthBufferEnable = true;
            heli.Draw(rCam);
            foreach (var b in helis.Values)
                b.Draw(rCam);
            
            island.Draw(rCam);

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Red, 1, 0);
            heli.Draw(cam);
            foreach (var b in helis.Values)
                b.Draw(cam);
            island.Draw(cam);
            sky.Draw(cam);
            water.Draw(cam, gameTime, reflectionMap);

            spriteBatch.Begin();
            //spriteBatch.Draw(reflectionMap, new Rectangle(0, 0, 100, 60), Color.White);
            var pos = GraphicsDevice.Viewport.Project(heli.Position, cam.Projection, cam.View, Matrix.Identity);
            spriteBatch.DrawString(font, heli.nickname, new Vector2(pos.X, pos.Y - 100), heli.color);
            foreach (var b in helis)
            {
                var posb = GraphicsDevice.Viewport.Project(b.Value.Position, cam.Projection, cam.View, Matrix.Identity);
            spriteBatch.DrawString(font, b.Value.nickname, new Vector2(posb.X, posb.Y - 100), b.Value.color);
            spriteBatch.DrawString(font, b.Key, new Vector2(posb.X, posb.Y - 80), b.Value.color);
            }


            spriteBatch.DrawString(font, "Collective: " + heli.collective.ToString(), new Vector2(10, 1), heli.color);
            spriteBatch.DrawString(font, "Roll: " + heli.roll.ToString(), new Vector2(10,25), heli.color);
            spriteBatch.DrawString(font, "Pitch: " + heli.pitch.ToString(), new Vector2(10, 50), heli.color);
            spriteBatch.DrawString(font, Math.Sin(heli.pitch).ToString(), new Vector2(10, 75), heli.color);
            spriteBatch.DrawString(font, Math.Sin(heli.roll).ToString(), new Vector2(10, 100), heli.color);
            spriteBatch.DrawString(font, (Math.Cos(heli.pitch)*Math.Cos(heli.roll)).ToString(), new Vector2(10, 125), heli.color);
           
            spriteBatch.End();
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            base.Draw(gameTime);
        }

        public float dir { get; set; }
    }
}
