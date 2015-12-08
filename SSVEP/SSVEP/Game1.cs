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


namespace SSVEP
{
    //****************************************************************************************************************************
    // main class
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // some shit we need to keep track of
        #region Fields
        Texture2D Horiz1, Horiz2, Vert1, Vert2;
        Color StimulusColor = Color.White;
        //float stimScale = 0.32f;
        float stimScale = 1.0f;
        public Game game;
        public ContentManager content;
        public SpriteBatch spriteBatch;
        int State1 = 1; int Cntr1 = 0; int StimType1 = 1;
        int State2 = 1; int Cntr2 = 0; int StimType2 = 1;
        int State3 = 1; int Cntr3 = 0; int StimType3 = 1;
        int State4 = 1; int Cntr4 = 0; int StimType4 = 1;
        int SCREENWIDTH, SCREENHEIGHT;
        Vector2 p1, p2, p3, p4;
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
            TargetElapsedTime = new TimeSpan( 10000000L / 60L );
            InactiveSleepTime = TimeSpan.Zero;
        }


    //****************************************************************************************************************************
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        protected override void Initialize()
        {
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
            //graphics.PreferredBackBufferHeight = this.SCREENHEIGHT;
            //graphics.PreferredBackBufferWidth = this.SCREENWIDTH;

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
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;   
            //form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                        
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
            Horiz2 = this.Content.Load<Texture2D>("solidHB");
            Vert1 = this.Content.Load<Texture2D>("solidVW");
            Vert2 = this.Content.Load<Texture2D>("solidVB");

            //// for the little shit to match stimbox
            //Horiz1 = this.Content.Load<Texture2D>("W1");
            //Horiz2 = this.Content.Load<Texture2D>("B1");
            //Vert1 = this.Content.Load<Texture2D>("W1");
            //Vert2 = this.Content.Load<Texture2D>("B1");

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

            //// for the little shit to match the stimbox
            //// top stimulus is located center top
            //p1.X = SCREENWIDTH / 2;
            //p1.Y = 4.1f * SCREENHEIGHT / 10;
            //// bottom stimulus is located at center bottom
            //p2.X = SCREENWIDTH / 2;
            //p2.Y = 5.9f * SCREENHEIGHT / 10;
            //// right stimulus location
            //p3.X = 5.9f * SCREENWIDTH / 10;
            //p3.Y = SCREENHEIGHT / 2;
            //// left stimulus location
            //p4.X = 4.1f * SCREENWIDTH / 10; ;
            //p4.Y = SCREENHEIGHT / 2;

            base.LoadContent();
        }


    //****************************************************************************************************************************
        /// UnloadContent will be called once per game and is the place to unload all content.
        protected override void UnloadContent()
        {
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Update the counters, these count frames
            Cntr1++; Cntr2++; Cntr3++; Cntr4++;

            //Do we need to flip the stimuli?? (check cntr variables: possibly flip states and reset cntr variables)

            //Update the 8.5 Hz Stimulus <---------( period of 60/7, 3 on 4 off )
            if( Cntr1 == 3 && State1 ==1 )
            {
                StimType1 = 1; Cntr1 = 0; State1 = 0;
            }
            else if( Cntr1 == 4 && State1==0)
            {
                StimType1 = 2; Cntr1 = 0; State1 = 1;
            }

            //Update the 6 Hz Stimulus <---------( period of 60/10, 5 on 5 off )
            if (Cntr2 == 5 && State2 == 1)
            {
                StimType2 = 1; Cntr2 = 0; State2 = 0;
            }
            else if (Cntr2 == 5 && State2 == 0)
            {
                StimType2 = 2; Cntr2 = 0; State2 = 1;
            }

            //Update the 7.5 Hz stimulus <---------( period of 60/8, 4 on 4 off )
            if (Cntr3 == 4 && State3 == 1)
            {
                StimType3 = 1; Cntr3 = 0; State3 = 0;
            }
            else if (Cntr3 == 4 && State3 == 0)
            {
                StimType3 = 2; Cntr3 = 0; State3 = 1;
            }

            ////Update the 6.66 Hz stimulus <---------( period of 60/9, 4 on 5 off )
            if (Cntr4 == 1 && State4 == 1)
            {
                StimType4 = 1; Cntr4 = 0; State4 = 0;
            }
            else if (Cntr4 == 1 && State4 == 0)
            {
                StimType4 = 2; Cntr4 = 0; State4 = 1;
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
            //Top stimulus - 8.5 Hz
            if ( StimType1 == 1 )
            {
                spriteBatch.Draw(Horiz1, p1, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if ( StimType1 == 2 )
            {
                spriteBatch.Draw(Horiz2, p1, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }

            //Bottom stimulus - 6 Hz
            if (StimType2 == 1)
            {
                spriteBatch.Draw(Horiz1, p2, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if (StimType2 == 2)
            {
                spriteBatch.Draw(Horiz2, p2, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }

            //Right stimulus - 7.5 Hz 
            if (StimType3 == 1)
            {
                spriteBatch.Draw(Vert1, p3, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if (StimType3 == 2)
            {
                spriteBatch.Draw(Vert2, p3, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }

            //Left Stimulus - 6.66 Hz
            if (StimType4 == 1)
            {
                spriteBatch.Draw(Vert1, p4, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }
            if (StimType4 == 2)
            {
                spriteBatch.Draw(Vert2, p4, null, Color.White, 0, Vector2.Zero, stimScale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
