// Some basic SSVEP software

// Go to: https://mxa.codeplex.com/releases 
// and get everything downloaded so you can
// build this shit in Visual Studio 2013


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Text;



namespace MSeq
{
    //****************************************************************************************************************************
    // main class
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // some shit we need to keep track of
        #region Fields
        Texture2D Horiz1, Horiz2, Horiz3, Vert1, Vert2, Vert3;
        Color StimulusColor = Color.White;
        //float stimScale = 0.32f;
        float stimScale = 1.0f;
        public Game game;
        public ContentManager content;
        public SpriteBatch spriteBatch;
        double Cntr1 = 0; int StimType1 = 1;
        double Cntr2 = 0; int StimType2 = 1;
        double Cntr3 = 0; int StimType3 = 1;
        double Cntr4 = 0; int StimType4 = 1;
        int SCREENWIDTH, SCREENHEIGHT;        
        Vector2 p1, p2, p3, p4;

        // used some function off the matlab website to generate this sequence
        int[] mSequence = { 1, 1, 2, 1, 1, 0, 2, 1, 2, 2, 2, 0, 1, 1, 0, 0, 1, 2, 1, 0, 0, 2, 2, 1, 2, 1, 2, 0, 2, 0, 0, 2, 0, 1, 2, 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 2, 0, 1, 0, 0, 0, 2, 1, 1, 2, 2, 1, 0, 1, 2, 2, 0, 0, 1, 0, 1, 0, 2, 0, 2, 2, 0, 2, 1, 0, 2, 2, 2, 2, 1 };

        UdpClient mServ = new UdpClient();
        // THIS IS THE PORT THAT WE USE TO LISTEN IN ON FOR BCI2000!!!!!!!!!!!!!!!!!!
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11001);
        #endregion Fields

        // setup some graphics crap
        GraphicsDeviceManager graphics;
        public System.Windows.Forms.Form form;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                SynchronizeWithVerticalRetrace = true,
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            TargetElapsedTime = new TimeSpan(10000000L / 60L);
            InactiveSleepTime = TimeSpan.Zero;
        }


        //****************************************************************************************************************************
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        protected override void Initialize()
        {   
            // starting position in the vector, spread this shit out
            Cntr1 = (Math.Round(0.0*mSequence.Length / 4.0));
            Cntr2 = (Math.Round(1.0*mSequence.Length / 4.0));
            Cntr3 = (Math.Round(2.0*mSequence.Length / 4.0));
            Cntr3 = (Math.Round(3.0*mSequence.Length / 4.0));


            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
            int maxHeight = 0; int maxWidth = 0; // vars to choose the max width and height
            GraphicsAdapter g = graphics.GraphicsDevice.Adapter;
            foreach (DisplayMode dm in g.SupportedDisplayModes)
            {
                if (maxHeight < dm.Height)
                {
                    maxHeight = dm.Height;
                }
                if (maxWidth < dm.Width)
                {
                    maxWidth = dm.Width;
                }
            }
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // fuck it, let's not try and fit it to screen resolution
            // hard code this shit to 800 x 600
            this.SCREENHEIGHT = 600;
            this.SCREENWIDTH = 800;

            // or make this shit take up the whole screen????
            //this.SCREENHEIGHT = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;
            //this.SCREENWIDTH = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
            graphics.PreferredBackBufferHeight = this.SCREENHEIGHT;
            graphics.PreferredBackBufferWidth = this.SCREENWIDTH;

            graphics.PreferMultiSampling = false;
            graphics.ApplyChanges();
            int[] margins = new int[] { -1, -1, -1, -1 };
            User32.DwmExtendFrameIntoClientArea(Window.Handle, ref margins);
            form = System.Windows.Forms.Control.FromHandle(Window.Handle).FindForm();
            form.Visible = true;
            form.AllowTransparency = true;
            form.WindowState = System.Windows.Forms.FormWindowState.Normal;
            form.TopMost = true;
            // window border, or no?
            //form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            base.Initialize();
        }


        //****************************************************************************************************************************
        /// LoadContent will be called once per game and is the place to load your graphics
        protected override void LoadContent()
        {
            // let's load in the stupid shit we flash, black and white blocks 
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // for the big shit
            Horiz1 = this.Content.Load<Texture2D>("solidHW");
            Horiz2 = this.Content.Load<Texture2D>("solidHG");
            Horiz3 = this.Content.Load<Texture2D>("solidHB");

            Vert1 = this.Content.Load<Texture2D>("solidVW");
            Vert2 = this.Content.Load<Texture2D>("solidVG");
            Vert3 = this.Content.Load<Texture2D>("solidVB");

            // for the big shit
            // top stimulus is located center top
            p1.X = (SCREENWIDTH - (Horiz1.Width * stimScale)) / 2;
            p1.Y = 0;
            // bottom stimulus is located at center bottom
            p2.X = (SCREENWIDTH - (Horiz1.Width * stimScale)) / 2;
            p2.Y = SCREENHEIGHT - (Horiz1.Height * stimScale);
            // right stimulus location
            p3.X = (SCREENWIDTH - (Vert1.Width * stimScale));
            p3.Y = (SCREENHEIGHT - (Vert1.Height * stimScale)) / 2;
            // left stimulus location
            p4.X = 0;
            p4.Y = (SCREENHEIGHT - (Vert2.Height * stimScale)) / 2;
      
            base.LoadContent();
        }


        //****************************************************************************************************************************
        /// UnloadContent will be called once per game and is the place to unload all content.
        protected override void UnloadContent()
        {
            mServ.Close();
            // TODO: Unload any non ContentManager content here
            foreach (GameComponent gc in Components)
            {
                gc.Dispose();
            }
        }


        //****************************************************************************************************************************        
        /// THE LOGIC BEHIND THE GAME!!!! CALLED 60 TIMES PER SECOND
        /// so it has to take < 1/60th of a second to run
        /// gameTime Provides a snapshot of timing values
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            KeyboardState mKeyState = Keyboard.GetState();
            if( mKeyState.IsKeyDown(Keys.Escape))
                this.Exit();
            
            //Do we need to flip the stimuli?? (check cntr variables: possibly flip states and reset cntr variables)
            StimType1 = mSequence[Convert.ToInt32(Cntr1)];
            StimType2 = mSequence[Convert.ToInt32(Cntr2)];
            StimType3 = mSequence[Convert.ToInt32(Cntr3)];
            StimType4 = mSequence[Convert.ToInt32(Cntr4)];
            
            // string to send
            string toSend = "mSeqPosition1 " + Cntr1.ToString() + '\n';
            byte[] mState = Encoding.ASCII.GetBytes( toSend );
            mServ.Send( mState, mState.Length,ep );

            // Update the counters, these count frames
            Cntr1++; Cntr2++; Cntr3++; Cntr4++;

            // Roll over the sequence?
            if (Cntr1 >= mSequence.Length)
            {
                Cntr1 = 0;
            }
            if (Cntr2 >= mSequence.Length)
            {
                Cntr2 = 0;
            }
            if (Cntr3 >= mSequence.Length)
            {
                Cntr3 = 0;
            }
            if (Cntr4 >= mSequence.Length)
            {
                Cntr4 = 0;
            }
            base.Update(gameTime);
        }

        //****************************************************************************************************************************        
        /// THIS IS CALLED WHEN THE GAME SHOULD DRAW ITSELF, 60 TIMES PER SECOND
        /// gameTime Provides a snapshot of timing values
        protected override void Draw(GameTime gameTime)
        {
            // Let's Draw this on a semi-transparent window, just for fucks.
            GraphicsDevice.Clear(new Color(0, 0, 0, 1.0f));

            spriteBatch.Begin();
            //Top stimulus
            if (StimType1 == 1)
            {
                spriteBatch.Draw(Horiz1, p1, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if (StimType1 == 2)
            {
                spriteBatch.Draw(Horiz2, p1, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if (StimType1 == 3)
            {
                spriteBatch.Draw(Horiz3, p1, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }

            //Bottom stimulus
            if (StimType2 == 1)
            {
                spriteBatch.Draw(Horiz1, p2, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if (StimType2 == 2)
            {
                spriteBatch.Draw(Horiz2, p2, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if (StimType2 == 3)
            {
                spriteBatch.Draw(Horiz3, p2, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }

            //Right stimulus
            if (StimType3 == 1)
            {
                spriteBatch.Draw(Vert1, p3, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if (StimType3 == 2)
            {
                spriteBatch.Draw(Vert2, p3, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if (StimType3 == 3)
            {
                spriteBatch.Draw(Vert3, p3, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }

            //Left Stimulus
            if (StimType4 == 1)
            {
                spriteBatch.Draw(Vert1, p4, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if (StimType4 == 2)
            {
                spriteBatch.Draw(Vert2, p4, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if (StimType4 == 3)
            {
                spriteBatch.Draw(Vert3, p4, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
