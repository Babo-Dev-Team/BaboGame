using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;
using System.Timers;
using BaboGameClient;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaboGame_test_2
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private SpriteFont _font;
        Debugger debugger;
        ServerHandler serverHandler;

        bool testMode; //Mode de pràctiques offline
        bool playable; //Poder controlar el personatge
        Texture2D backgroundImage;

        private List<Character> characterSprites;           // Personatges (inclòs el jugador)
        private List<Projectile> projectileSprites;         // Projectils, creats per projectileEngine
        private List<Sprite> overlaySprites;                // Sprites de la UI, de moment només la mira
        private List<Slime> slimeSprites;                   // Babes, creats per SlimeEngine
        private List<ScenarioObjects> scenarioSprites;      // Sprites per objectes sòlids que estiguin per pantalla

        ProjectileEngine projectileEngine;
        ProjectileManager projectileManager;
        Dictionary<string, Texture2D> projectileTexture;
        Dictionary<string, Animation> slugHealth;
        Dictionary<string, Animation> sightAnimation;
        SlimeEngine slimeEngine;
        HeartManager heartManager;                          // Mecanismes de la vida
        InputManager inputManager = new InputManager(Keys.W, Keys.S, Keys.A, Keys.D); // El passem ja inicialitzat als objectes
        KeyboardState _previousState;

        CharacterEngine characterEngine;
       
        Texture2D slimeTexture;                             // Textura per instanciar les babes
        Texture2D slugTexture;
        Texture2D sightTexture;
        Texture2D scenarioTexture;
        Texture2D projectileMenuTexture;

        //Variables del online
        initState initGame = new initState();
        GameState gameState = new GameState();
        user thisClient;
        Character Controllable;

        //Temporització de les babes
        private static Timer timer;
        int SlimeTime = 0;
        Random EnemyShoot = new Random(); //-------------------------------------Babo prova

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
        }

        public Game1(ServerHandler serverHandler, bool testMode)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            this.testMode = testMode;
            this.serverHandler = serverHandler;
            //serverHandler.SwitchToRealtimeMode();
            AllocConsole();
            Console.WriteLine("testline");
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            slimeSprites = new List<Slime>();
            slimeEngine = new SlimeEngine(slimeSprites);
            
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Funció Load Content
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
     
            slugHealth = new Dictionary<string, Animation>()
            {
                {"3/4 heart", new Animation(Content.Load<Texture2D>("Slug_status/heart-3_4"), 1) },
                {"2/4 heart", new Animation(Content.Load<Texture2D>("Slug_status/heart-2_4"), 1) },
                {"1/4 heart", new Animation(Content.Load<Texture2D>("Slug_status/heart-1_4"), 1) },
                {"Empty heart", new Animation(Content.Load<Texture2D>("Slug_status/heart-empty"), 1) },
                {"Babo down hit", new Animation(Content.Load<Texture2D>("Babo/Babo down hit"), 1) },
                {"Heart", new Animation(Content.Load<Texture2D>("Slug_status/Heart"), 1) },
            };



            sightAnimation = new Dictionary<string, Animation>()
            {
                {"ON", new Animation(Content.Load<Texture2D>("Sight/Sight_on"), 1) },
                {"OFF", new Animation(Content.Load<Texture2D>("Sight/Sight_off"), 1) },
            };

            slugTexture = Content.Load<Texture2D>("Babo/Babo down0 s0");
            sightTexture = Content.Load<Texture2D>("Sight/Sight_off");
            scenarioTexture = Content.Load<Texture2D>("Scenario/Block");

            projectileMenuTexture = Content.Load<Texture2D>("Slug_status/SaltMenu");
            projectileTexture = new Dictionary<string, Texture2D>()
            {
                {"Normal", Content.Load<Texture2D>("Projectile/Salt")},
                {"Direct", Content.Load<Texture2D>("Projectile/DirectSalt")},
                {"Slimed", Content.Load<Texture2D>("Projectile/NoNewtonianSlimedSalt")},
            };
            
            slimeTexture = Content.Load<Texture2D>("Projectile/slime2");

            characterSprites = new List<Character>();

            // La mira necessita que li passem inputManager per obtenir la posició del ratolí
            overlaySprites = new List<Sprite>()
            {
                new SightWeapon(sightAnimation, inputManager)
                {
                    Position = new Vector2(100,100),
                    Scale = 0.2f,
                    SolidObject = false,
                    Layer = 1f,
                },


            };

            scenarioSprites = new List<ScenarioObjects>()
            {
                new ScenarioObjects(scenarioTexture)
                {
                    Position = new Vector2(400,100),
                    Scale = 0.2f,
                    SolidObject = true,
                    HitBoxScale = 1f,
                    HasConducitivity = true,
                    Charge = 'P',
                },

                new ScenarioObjects(scenarioTexture)
                {
                    Position = new Vector2(400,500),
                    Scale = 0.2f,
                    SolidObject = true,
                    HitBoxScale = 1f,
                    HasConducitivity = true,
                    Charge = 'N',
                },
            };

            characterEngine = new CharacterEngine(characterSprites,Content);
            projectileSprites = new List<Projectile>();
            projectileEngine = new ProjectileEngine(projectileSprites);
            projectileManager = new ProjectileManager(projectileTexture, projectileEngine);

            heartManager = new HeartManager(overlaySprites);

            _font = Content.Load<SpriteFont>("Font");

            //timer
            timer = new Timer(60);
            timer.AutoReset = true;
            timer.Enabled = true;
            debugger = new Debugger(characterSprites,projectileSprites,overlaySprites,slimeSprites, timer.Interval,graphics.PreferredBackBufferWidth,graphics.PreferredBackBufferHeight,_font);
            serverHandler.RequestInitState();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            FreeConsole();
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Funció Upload
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
           if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
               Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.F11) && (_previousState.IsKeyUp(Keys.F11)))
                graphics.ToggleFullScreen();

            // Detectem inputs al teclat
            inputManager.detectKeysPressed();
            _previousState = Keyboard.GetState();

            int code = 0;

            if (ReceiverArgs.newDataFromServer == 1)
            {
                Console.WriteLine("Response Received: Code " + ReceiverArgs.responseType);
                Console.WriteLine(ReceiverArgs.responseStr);
                code = ReceiverArgs.responseType;
                UpdateOnline();
                ReceiverArgs.newDataFromServer = 0;
            }

            if(code == 101)
            {
                UpdateInit();
            }
            else if(code == 102)
            {

            }
            else if(code == 103)
            {
                PeriodicalUpdate();
            }

            if (playable)
            {
                // Actualitzem direcció i moviment del playerChar segons els inputs i les bales
                UpdateControllableCharacter(gameTime);
            }

            //Això actualitzaria els objectes del escenari
            foreach (var ScenarioObj in scenarioSprites)
            {
                ScenarioObj.Update(gameTime);
            }

            characterEngine.Update(gameTime,slimeSprites,scenarioSprites);
            // Això hauria de moure els projectils, calcular les colisions i notificar als characters si hi ha hagut dany.
            projectileEngine.UpdateProjectiles(gameTime, characterSprites, scenarioSprites);

            // Generem les babes amb una certa espera per no sobrecarregar i les instanciem al update del personatge
            timer.Elapsed += OnTimedEvent;

            foreach (var character in characterSprites.ToArray())
            {
                character.Update(gameTime, characterSprites);
                heartManager.UpdateHealth(character.IDcharacter, character.Health);
                if ((SlimeTime > 80) && (slimeSprites.Count < 400))
                {
                    slimeSprites.Add(
                       new Slime(new Vector2(character.Position.X, character.Position.Y + 20), character.IDcharacter, slimeTexture, 0.15f)
                       {
                           timer = 0,
                       }
                       );
                    character.isSlip = false;
                }
            }

            if ((SlimeTime > 80))
            {              
                foreach (var slime in slimeSprites)
                {
                    slime.timer++;
                }
                SlimeTime = 0;
            }

            //Això hauria de fer reaccionar les babes a projectils, characters i objectes de l'escenari
            slimeEngine.UpdateSlime(gameTime, characterSprites, projectileSprites, scenarioSprites);

            foreach (var overlay in this.overlaySprites)
            {
                overlay.Update(gameTime, overlaySprites);
            }
            
            PostUpdate();
            base.Update(gameTime);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Funcions pel Upload
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //Funció per definir la mort dels objectes
        private void PostUpdate()
        {
             for (int i = 0; i < characterSprites.Count; i++)
            {
                if (characterSprites[i].IsRemoved)
                {
                    characterSprites.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < projectileSprites.Count; i++)
            {
                if (projectileSprites[i].IsRemoved)
                {
                    projectileSprites.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < slimeSprites.Count; i++)
            {
                if (slimeSprites[i].IsRemoved)
                {
                    slimeSprites.RemoveAt(i);
                    i--;
                }
            }
        }
        
        //Actualitzar el temporitzador
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            SlimeTime++;
        }

        //Actualitzar la llista JSON
        private void UpdateOnline()
        {
            if (ReceiverArgs.responseType == 101)
            {
                initGame = JsonSerializer.Deserialize<initState>(ReceiverArgs.responseStr);

            }
            else if(ReceiverArgs.responseType == 102)
            {
                if (ReceiverArgs.responseStr == "START")
                    playable = true;
            }
            else if(ReceiverArgs.responseType == 103)
            {
                gameState = JsonSerializer.Deserialize<GameState>(ReceiverArgs.responseStr);
            }
        }

        //Incialitza els components amb el codi 101
        private void UpdateInit()
        {
            thisClient = initGame.thisUser;
            characterSprites.Clear();
            for (int i = 0; i < initGame.nPlayers; i++)
            {
                characterEngine.AddKnownCharacter(initGame.users[i].charName, new Vector2(i*60, 0), 0.20f, 20, initGame.users[i].charId, Color.White);
                heartManager.CreateHeart(initGame.users[i].charId, 5, 20, slugHealth, new Vector2(10, 40*i + 20));

                if (thisClient == initGame.users[i])
                {
                    projectileManager.CreateSaltMenu(projectileMenuTexture, overlaySprites, initGame.thisUser.charId, 0.1f);
                    Controllable = characterSprites.ToArray()[i];
                }

            }
        }

        //Actualitza els components amb el codi 102
        private void PeriodicalUpdate()
        {
            foreach(CharacterState characterState in gameState.characterStatesList)
            {
                bool found = false;
                int i = 0;
                while((!found)&&(i<gameState.characterStatesList.Count))
                {
                    if (characterState.charID == characterSprites[i].IDcharacter)
                    {
                        found = true;
                        characterSprites[i].Position = new Vector2(characterState.posX, characterState.posY);
                        characterSprites[i].Velocity = new Vector2(characterState.velX, characterState.velY);
                    }
                    else
                        i++;
                }

                
            }

            if (gameState.playable == 1)
                playable = true;
            else
                playable = false;
        }

        //Control dels personatges
        private void UpdateControllableCharacter(GameTime gameTime)
        {
            Controllable.Direction = VectorOps.UnitVector(inputManager.GetMousePosition() - Controllable.Position);

            if (playable)
            {
                if (inputManager.RightCtrlActive())
                {
                    Controllable.MoveRight();
                }
                if (inputManager.LeftCtrlActive())
                {
                    Controllable.MoveLeft();
                }
                if (inputManager.UpCtrlActive())
                {
                    Controllable.MoveUp();
                }
                if (inputManager.DownCtrlActive())
                {
                    Controllable.MoveDown();
                }

                // llançem projectils segons els inputs del jugador
                inputManager.DetectMouseClicks();
                projectileManager.Update(gameTime, inputManager.GetMouseWheelValue(), overlaySprites, characterSprites);
                if (inputManager.LeftMouseClick())
                {
                    Vector2 projOrigin = Controllable.Position;
                    Vector2 projTarget = inputManager.GetMousePosition();
                    projectileManager.AddProjectile(projOrigin, projTarget, Controllable.IDcharacter);
                }
            }
        }

        //Control personatges per la màquina
        private void CPUcharacter()
        {
            /*
            //Actualitzem moviment del llimac de prova ---------------------Limax prova
            playerChar3.Direction = VectorOps.UnitVector(playerChar.Position - playerChar3.Position);

            if (!Slug3Direction)
                playerChar3.MoveRight();
            else
                playerChar3.MoveLeft();
            if (Slug3Direction2)
                playerChar3.MoveUp();
            else
                playerChar3.MoveDown();

            if ((playerChar3.Position.X > graphics.PreferredBackBufferWidth))
                Slug3Direction = true;
            else if (playerChar3.Position.X < 0)
                Slug3Direction = false;

            if (playerChar3.Position.Y > graphics.PreferredBackBufferHeight)
                Slug3Direction2 = true;
            else if (playerChar3.Position.Y < 0)
                Slug3Direction2 = false;

            //if (EnemyShoot.Next(0,32) == 0) //--------------------------- Babo prova
            //projectileEngine.AddProjectile(playerChar2.Position, playerChar.Position, projectileTexture["Slimed"], 2,'S');
            */
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //Funció Draw
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.LinearWrap);
            //spriteBatch.Begin(SpriteSortMode.FrontToBack);
            //Fons
            backgroundImage = Content.Load<Texture2D>("Scenario/Scenario1");
            float backGroundScale = Math.Max(((float)graphics.PreferredBackBufferWidth / (float)backgroundImage.Width), ((float)graphics.PreferredBackBufferHeight / (float)backgroundImage.Height));
            spriteBatch.Draw(backgroundImage, new Vector2(0, 0), null, Color.White, 0f, new Vector2(0, 0), backGroundScale, SpriteEffects.None, 0f);

            debugger.DrawText(spriteBatch);

            foreach (var sprite in scenarioSprites)
            {
                sprite.Draw(spriteBatch);
            }
            foreach (var sprite in slimeSprites)
            {
                sprite.Draw(spriteBatch);
            }
            foreach (var sprite in characterSprites)
            {
                sprite.Draw(spriteBatch);
            }

            foreach (var sprite in projectileSprites)
            {
                sprite.Draw(spriteBatch);
            }
            foreach (var overlay in overlaySprites)
            {
                if(overlay.Visible)
                    overlay.Draw(spriteBatch);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
