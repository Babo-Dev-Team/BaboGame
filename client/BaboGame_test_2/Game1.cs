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
        private SpriteFont _namesFont;
        InputManager inputManager = new InputManager(Keys.W, Keys.S, Keys.A, Keys.D); // El passem ja inicialitzat als objectes
        KeyboardState _previousState;

        CharacterEngine characterEngine;
       
        Texture2D slimeTexture;                             // Textura per instanciar les babes
        Texture2D slugTexture;
        Texture2D sightTexture;
        Texture2D scenarioTexture;
        Texture2D projectileMenuTexture;

        public class NameFontModel
        {
            public Vector2 Position;
            public string name;
            public float rotation;
            public float scale;
            public Vector2 origin;
            public float layer;
            public SpriteEffects effect;
            public Color color;
            public int charID;

            public NameFontModel(string name, Vector2 position,Color color, float rotation, float scale, Vector2 origin,SpriteEffects effect, float layer, int charID)
            {
                this.name = name;
                this.Position = position;
                this.color = color;
                this.rotation = rotation;
                this.scale = scale;
                this.origin = origin;
                this.effect = effect;
                this.layer = layer;
                this.charID = charID;
            }
        }
        List<NameFontModel> playersNames;

        public class LocalGameState
        {
            public int[] Player_ID;
            public string PlayerCharacter_Selected;
            public List<string> OpponentCharacter_Selected = new List<string>();
            public int Opponentnum_players;

            public LocalGameState(int[] Player_ID, string PlayerCharacter_Selected, List<string> OpponentCharacter_Selected, int Opponentnum_players)
            {
                this.Player_ID = Player_ID;
                this.PlayerCharacter_Selected = PlayerCharacter_Selected;
                this.OpponentCharacter_Selected = OpponentCharacter_Selected;
                this.Opponentnum_players = Opponentnum_players;
            }

            public LocalGameState()
            {

            }

        }
        LocalGameState localGameState;

        //Variables del online
        initState initGame = new initState();
        GameState gameState = new GameState();
        user thisClient;
        Character Controllable;
        bool Initialized;
        bool InitRequested;

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
            Initialized = false;
            InitRequested = false;
        }

        public Game1(ServerHandler serverHandler)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            this.testMode = false;
            Initialized = false;

            this.serverHandler = serverHandler;
            //serverHandler.SwitchToRealtimeMode();
            //AllocConsole();
            Console.WriteLine("testline");
        }

        public Game1(LocalGameState localGameState)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            this.testMode = true;
            Initialized = false;
            this.localGameState = localGameState;
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
            serverHandler.RequestInitState();
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

            //Omplim els diccionaris d'imatges perquè els objectes de l'escenari tinguin un bon repertori
            //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            //Cors de la vida del llimac     
            slugHealth = new Dictionary<string, Animation>()
            {
                {"3/4 heart", new Animation(Content.Load<Texture2D>("Slug_status/heart-3_4"), 1) },
                {"2/4 heart", new Animation(Content.Load<Texture2D>("Slug_status/heart-2_4"), 1) },
                {"1/4 heart", new Animation(Content.Load<Texture2D>("Slug_status/heart-1_4"), 1) },
                {"Empty heart", new Animation(Content.Load<Texture2D>("Slug_status/heart-empty"), 1) },
                {"Babo down hit", new Animation(Content.Load<Texture2D>("Babo/Babo down hit"), 1) },
                {"Heart", new Animation(Content.Load<Texture2D>("Slug_status/Heart"), 1) },
            };

            //Animació de la mira
            sightAnimation = new Dictionary<string, Animation>()
            {
                {"ON", new Animation(Content.Load<Texture2D>("Sight/Sight_on"), 1) },
                {"OFF", new Animation(Content.Load<Texture2D>("Sight/Sight_off"), 1) },
            };

            //Textures
            slugTexture = Content.Load<Texture2D>("Babo/Babo down0 s0");
            sightTexture = Content.Load<Texture2D>("Sight/Sight_off");
            scenarioTexture = Content.Load<Texture2D>("Scenario/Block");

            //Menú de les bales del llimac
            projectileMenuTexture = Content.Load<Texture2D>("Slug_status/SaltMenu");
            projectileTexture = new Dictionary<string, Texture2D>()
            {
                {"Normal", Content.Load<Texture2D>("Projectile/Salt")},
                {"Direct", Content.Load<Texture2D>("Projectile/DirectSalt")},
                {"Slimed", Content.Load<Texture2D>("Projectile/NoNewtonianSlimedSalt")},
            };
            
            //Babes
            slimeTexture = Content.Load<Texture2D>("Projectile/slime2");

            //Inicialitzem llistes, engines i managers pel funcionament del projecte

            //Llista de sprites dels jugadors
            characterSprites = new List<Character>();

            // La mira necessita que li passem inputManager per obtenir la posició del ratolí
            //Llista de sprites del objectes de pantalla
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
            playersNames = new List<NameFontModel>();

            //Llista de sprites de l'escenari
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

            //Llista de sprites de les bales
            projectileSprites = new List<Projectile>();

            //Engines i managers dels cors, personatges i projectils
            characterEngine = new CharacterEngine(characterSprites, Content);
            projectileEngine = new ProjectileEngine(projectileSprites);
            projectileManager = new ProjectileManager(projectileTexture, projectileEngine);
            heartManager = new HeartManager(overlaySprites);

            //Text en pantalla
            _font = Content.Load<SpriteFont>("Font");
            _namesFont = Content.Load<SpriteFont>("NamesFont"); 

            //timer
            timer = new Timer(60);
            timer.AutoReset = true;
            timer.Enabled = true;
            debugger = new Debugger(characterSprites,projectileSprites,overlaySprites,slimeSprites, timer.Interval,graphics.PreferredBackBufferWidth,graphics.PreferredBackBufferHeight,_font);
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

        //private bool initStateRequested = false;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
          // if (!initStateRequested)
            //{
              //  this.serverHandler.RequestInitState();
                //initStateRequested = true;
           // }
           if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
               Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.F11) && (_previousState.IsKeyUp(Keys.F11)))
                graphics.ToggleFullScreen();

            // Detectem inputs al teclat
            inputManager.detectKeysPressed();
            _previousState = Keyboard.GetState();

            if (!testMode)
            {


                if (ReceiverArgs.newDataFromServer == 1)
                {
                    //int responseType;
                    //string responseStr;
                    GenericResponse response;
                    for (int i = 0; i < ReceiverArgs.responseFifo.Count(); i++)
                    {
                        response = ReceiverArgs.responseFifo.Dequeue();

                        Console.WriteLine("Response Received: Code " + response.responseType);
                        Console.WriteLine(response.responseStr);
                        //UpdateOnline();


                        if (response.responseType == 101)
                        {
                            initGame = JsonSerializer.Deserialize<initState>(response.responseStr);
                            UpdateInit();
                            Initialized = true;
                        }
                        else if (response.responseType == 102)
                        {
                            /*if (response.responseStr == "START")
                            {
                                playable = true;
                            }
                            else if (response.responseStr.Split('/')[0] == "END")
                            {
                                playable = false;
                                // TODO: CODI PER PARAR LA PARTIDA, extreure resultats etc.
                            }*/
                        }
                        else if ((response.responseType == 103) && (Initialized))
                        {
                            gameState = JsonSerializer.Deserialize<GameState>(response.responseStr);
                            PeriodicalUpdate();
                        }
                    }
                    ReceiverArgs.newDataFromServer = 0;
                }

            }
            else
            {
                InitTraining();
            }

            if ((Initialized)||(testMode))
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
       /* private void UpdateOnline()
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
        }*/

        //Incialitza els components amb el codi 101
        private void UpdateInit()
        {
            thisClient = initGame.thisUser;
            characterSprites.Clear();
            for (int i = 0; i < initGame.nPlayers; i++)
            {
                characterEngine.AddKnownCharacter(initGame.users[i].charName, new Vector2(i*60, 0), 0.20f, 20, initGame.users[i].charId, Color.White);
                Vector2 HeartPosition = HeartPosInScreen(initGame.nPlayers,i,5);
                heartManager.CreateHeart(initGame.users[i].charId, 5, 20, slugHealth, HeartPosition);

                if (thisClient.charId == initGame.users[i].charId)
                {
                    playersNames.Add(new NameFontModel(initGame.users[i].userName, new Vector2(HeartPosition.X, HeartPosition.Y - 65), Color.Black, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.99f, initGame.users[i].charId));
                    playersNames.Add(new NameFontModel(initGame.users[i].userName, new Vector2(HeartPosition.X, HeartPosition.Y - 70), Color.LightGreen, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 1f, initGame.users[i].charId));
                    projectileManager.CreateSaltMenu(projectileMenuTexture, overlaySprites, initGame.thisUser.charId, 0.1f);
                    Controllable = characterSprites.ToArray()[i];
                }
                else
                {
                    playersNames.Add(new NameFontModel(initGame.users[i].userName, new Vector2(HeartPosition.X, HeartPosition.Y - 65), Color.Black, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.99f, initGame.users[i].charId));
                    playersNames.Add(new NameFontModel(initGame.users[i].userName, new Vector2(HeartPosition.X, HeartPosition.Y - 70), Color.White, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 1f, initGame.users[i].charId));
                }

            }
        }

        //Inicialitza entrenament

        //Retorna la posició dels cors
        public Vector2 HeartPosInScreen(int nPlayers, int i, int heartNum)
        {
            Vector2 HeartPos;

            float height = 720;

            switch(nPlayers)
            {
                case 2: //2 jugadors
                    if(i==0) //Lateral esquerra
                        HeartPos = new Vector2(50, 120);
                    else //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum*30, 120);
                    break;
                case 3: //3 jugadors
                    if (i == 0) //Lateral esquerra
                        HeartPos = new Vector2(50, 120);
                    else if (i==1) //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum * 30, 120);
                    else //Part inferior
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth/2 - heartNum * 30/2, 600);
                    break;
                case 4: //4 jugadors
                    if (i < 2) //Lateral esquerra
                        HeartPos = new Vector2(50, 120 * (i*4 + 1));
                    else //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum * 30, 120 * (i * 4 + 1));
                    break;
                case 5: //5 jugadors
                    if (i < 2) //Lateral esquerra
                        HeartPos = new Vector2(50, 120 * (i * 3 + 1));
                    else if (i < 4) //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum * 30, 120 * (i * 3 + 1));
                    else //Part inferior
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth / 2 - heartNum * 30 / 2, 600);
                    break;
                case 6: //6 jugadors
                    if (i < 3) //Lateral esquerra
                        HeartPos = new Vector2(50, 120 * (i * 2 + 1));
                    else //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum * 30, 120 * (i * 2 + 1));
                    break;
                case 7: //7 jugadors
                    if (i < 3) //Lateral esquerra
                        HeartPos = new Vector2(50, 90 * (i * 2 + 1));
                    else if (i < 7) //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum * 30, 90 * (i * 2 + 1));
                    else //Part inferior
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth / 2 - heartNum * 30 / 2, 630);
                    break;
                case 8: //8 jugadors
                    if (i < 4) //Lateral esquerra
                        HeartPos = new Vector2(50, 90 * (i * 2 + 1));
                    else //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum * 30, 90 * (i * 2 + 1));
                    break;
                default:
                    HeartPos = new Vector2(10, 40 * i + 20);
                    break;
            }

            return HeartPos;
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

        //Incialitza els components amb el codi 101
        private void InitTraining()
       {
           
            characterSprites.Clear();
            for (int i = 0; i < localGameState.Opponentnum_players + 1; i++)
            {
                Vector2 HeartPosition;
                if (localGameState.Player_ID[i] == 1)
                {
                    characterEngine.AddKnownCharacter(localGameState.PlayerCharacter_Selected, new Vector2(i * 60, 0), 0.20f, 20, localGameState.Player_ID[i], Color.White);
                    HeartPosition = HeartPosInScreen(localGameState.Opponentnum_players+1, i, 5);
                    heartManager.CreateHeart(localGameState.Player_ID[i], 5, 20, slugHealth, HeartPosition);
                    playersNames.Add(new NameFontModel("Jugador", new Vector2(HeartPosition.X, HeartPosition.Y - 65), Color.Black, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.99f, localGameState.Player_ID[i]));
                    playersNames.Add(new NameFontModel("Jugador", new Vector2(HeartPosition.X, HeartPosition.Y - 70), Color.LightGreen, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 1f, localGameState.Player_ID[i]));
                    projectileManager.CreateSaltMenu(projectileMenuTexture, overlaySprites, localGameState.Player_ID[i], 0.1f);
                    Controllable = characterSprites.ToArray()[i];
                }
                else if (i!=0)
                {
                    characterEngine.AddKnownCharacter(localGameState.OpponentCharacter_Selected[i-1], new Vector2(i * 60, 0), 0.20f, 20, localGameState.Player_ID[i], Color.White);
                    HeartPosition = HeartPosInScreen(localGameState.Opponentnum_players + 1, i, 5);
                    heartManager.CreateHeart(localGameState.Player_ID[i], 5, 20, slugHealth, HeartPosition);
                    playersNames.Add(new NameFontModel("CPU " + localGameState.Player_ID[i], new Vector2(HeartPosition.X, HeartPosition.Y - 65), Color.Black, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.99f, localGameState.Player_ID[i]));
                    playersNames.Add(new NameFontModel("CPU " + localGameState.Player_ID[i], new Vector2(HeartPosition.X, HeartPosition.Y - 70), Color.White, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 1f, localGameState.Player_ID[i]));

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
            foreach (var names in playersNames)
            {
                spriteBatch.DrawString(_namesFont, names.name, names.Position, names.color, names.rotation, names.origin, names.scale, names.effect, names.layer);
                
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
