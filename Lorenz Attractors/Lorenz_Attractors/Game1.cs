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

namespace Lorenz_Attractors
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const float IntervalleMAJ = 1/60f;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        const float FPS = 1 / 60f;
        RessourcesManager<Texture2D> TextureManager { get; set; }
        InputManager InputManager { get; set; }
        Caméra Camera { get; set; }
        bool Sleep { get; set; }
        SphèreTexturée Sphère { get; set; }

        float TempsÉcouléDepuisMAJ { get; set; }

        float x = 0.01f;
        float y = 0f;
        float z = 0f;

        float a = 10;
        float b = 28;
        float c = 8 / 3f;

        Vector3 Position;
        Vector3 AnciennePosition;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Position = new Vector3(x, y, z);
            AnciennePosition = Position;
            Sleep = false;
            InputManager = new InputManager(this);
            Services.AddService(typeof(InputManager), InputManager);
            TextureManager = new RessourcesManager<Texture2D>(this, "Textures");
            Services.AddService(typeof(RessourcesManager<Texture2D>), TextureManager);
            Services.AddService(typeof(RessourcesManager<SpriteFont>), new RessourcesManager<SpriteFont>(this, "Fonts"));
            Components.Add(InputManager);
            Components.Add(new Afficheur3D(this));
            Camera = new CaméraJoueur(this, new Vector3(-500, 0, 0), new Vector3(20, 0, 0), Vector3.Up, FPS, 10000);
            Services.AddService(typeof(Caméra), Camera);
            //Components.Add(new SphèreTexturée(this, 1, Vector3.Zero, new Vector3(0, 0, 0), 1, new Vector2(20, 20), "Blanc", FPS));
            Sphère = new SphèreTexturée(this, 1, Vector3.Zero, Position, 0.1f, new Vector2(20, 20), "Blanc", FPS);
            Components.Add(Sphère);
            Components.Add(Camera);
            Components.Add(new AfficheurFPS(this, "Arial", Color.Black, IntervalleMAJ));
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

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
            if (!Sleep)
            {
                // Allows the game to exit
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    this.Exit();

                // TODO: Add your update logic here

                float TempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
                TempsÉcouléDepuisMAJ += TempsÉcoulé;
                if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
                {
                    EffectuerMiseÀJour();
                    TempsÉcouléDepuisMAJ = 0;
                }

                base.Update(gameTime);
            }
        }

        int R = 0, G = 0, B = 0;
        double Hu = 0, S = 1, V = 1;

        void EffectuerMiseÀJour()
        {
            float dt = 0.01f;
            float dx = (a * (y - x)) * dt;
            float dy = (x * (b - z) - y) * dt;
            float dz = (x * y - c * z) * dt;
            x = x + dx;
            y = y + dy;
            z = z + dz;

            AnciennePosition = Position;
            Position = new Vector3(x, y, z);

            //Window.Title = Position.ToString();
            HsvToRgb(Hu, S, V, out R, out G, out B);
            Window.Title = new Color(R, G, B).ToString();

            Hu += 0.1;
            if (Hu > 255)
            {
                Hu = 0;
            }
            int constanteGrosseur = 10;

            Components.Add(new CylindreTexturé(this, 1f, new Vector3(0, 0, 0),
                                    Vector3.Zero, new Vector2(1, 1), new Vector2(20, 20),
                                    /*"Blanc"*/new Color(R, G, B), IntervalleMAJ, constanteGrosseur * AnciennePosition,
                                    constanteGrosseur * Position));

            //Sphère.AjouterSphère(Position);

        }

        // HslToRgb
        #region
        void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
        #endregion

        protected override void OnActivated(object sender, EventArgs args)
        {
            Sleep = false;
            base.OnActivated(sender, args);
            if (Camera != null)
            {
                (Camera as CaméraJoueur).EstCaméraSourisActivée = true;
            }
            IsMouseVisible = false;
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            Sleep = true;
            base.OnDeactivated(sender, args);
            if (Camera != null)
            {
                (Camera as CaméraJoueur).EstCaméraSourisActivée = false;
            }
            IsMouseVisible = true;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
