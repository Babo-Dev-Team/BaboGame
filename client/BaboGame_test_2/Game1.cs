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
        Texture2D PauseImage;

        //Llista d'objectes en pantalla
        private List<Character> characterSprites;           // Personatges (inclòs el jugador)
        private List<Projectile> projectileSprites;         // Projectils, creats per projectileEngine
        private List<Sprite> overlaySprites;                // Sprites de la UI, de moment només la mira
        private List<Slime> slimeSprites;                   // Babes, creats per SlimeEngine
        private List<ScenarioObjects> scenarioSprites;      // Sprites per objectes sòlids que estiguin per pantalla

        //Engines, managers i diccionaris d'animacions
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
        
        //Textures dels objectes
        Texture2D slimeTexture;                             // Textura per instanciar les babes
        Texture2D slugTexture;
        Texture2D sightTexture;
        Texture2D scenarioTexture;
        Texture2D projectileMenuTexture;

        //Objecte del text dels noms
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
            public bool visible;

            public NameFontModel(string name, Vector2 position,Color color, float rotation, float scale, Vector2 origin,SpriteEffects effect, float layer, int charID, bool visible)
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
                this.visible = visible;
            }
        }
        List<NameFontModel> playersNames;
        List<NameFontModel> otherTexts; // charID = -2 pausa offline, charID = -3 joc acabat 

        //Estat del joc en Offline
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
        char Difficulty; //E -easy, M - medium, D- difficult, I - insane, N - None

        //Variables del online
        initState initGame = new initState();
        GameState gameState = new GameState();
        user thisClient;
        Character Controllable;
        bool Initialized;
        bool InitRequested;

        //Estats del joc
        bool GameEnded;
        bool GamePaused; //Només offline
        bool HasWinner;
        int IDwinner;

        //Temporització de les babes
        private static Timer timer;
        private static Timer slimeTimer;
        float SlimeTime = 0;
        //Temportizació dels pdates cap el servidor
        float UpdateOnlineTime = 0;
        int NextProjectileID;
        static int BulletThreshold = 20;
        static int projectileOnlineUpdateThreshold = 32;
        
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Procediments de les estructures
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            //Default
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            Initialized = false;
            InitRequested = false;
            NextProjectileID = 0;
            GameEnded = false;
            GamePaused = false;
            HasWinner = false;
        }

        //Mode Online
        public Game1(ServerHandler serverHandler)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            this.testMode = false;
            Initialized = false;
            GameEnded = false;
            GamePaused = false;
            HasWinner = false;

            this.serverHandler = serverHandler;
            //serverHandler.SwitchToRealtimeMode();
            //AllocConsole();
            Console.WriteLine("testline");
            NextProjectileID = 0;
        }

        //Mode offline
        public Game1(LocalGameState localGameState, char difficulty)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            this.testMode = true;
            Initialized = false;
            this.localGameState = localGameState;
            Difficulty = difficulty;
            NextProjectileID = 0;
            GameEnded = false;
            GamePaused = false;
            HasWinner = false;
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
            if(!testMode)
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
            otherTexts = new List<NameFontModel>();

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

            /*
            //timer
            timer = new Timer(0.1);
            timer.AutoReset = true;
            timer.Enabled = true;
            //timer de les babes
            slimeTimer = new Timer(60);
            slimeTimer.AutoReset = true;
            slimeTimer.Enabled = true;
            */
            debugger = new Debugger(characterSprites,projectileSprites,overlaySprites,slimeSprites, 0,graphics.PreferredBackBufferWidth,graphics.PreferredBackBufferHeight,_font);
            
            //timer.Elapsed += OnTimedEvent;
            //slimeTimer.Elapsed += OnSlimeTimedEvent;
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
            if ((Keyboard.GetState().IsKeyDown(Keys.Space))&&(GameEnded))
                Exit();
            //if (Keyboard.GetState().IsKeyDown(Keys.F11) && (_previousState.IsKeyUp(Keys.F11)))
            //graphics.ToggleFullScreen();

            if (Keyboard.GetState().IsKeyDown(Keys.P) && (_previousState.IsKeyUp(Keys.P)) && (testMode))
            {
                if (GamePaused)
                    GamePaused = false;
                else
                    GamePaused = true;
            }
            

            // Detectem inputs al teclat
            inputManager.detectKeysPressed();
            _previousState = Keyboard.GetState();

            if (!testMode)
            {


            if (ReceiverArgs.newDataFromServer != 0)
            {
                if (ReceiverArgs.newDataFromServer == 1)
                {
                    GenericResponse response;
                    for (int i = 0; i < ReceiverArgs.responseFifo.Count(); i++)
                    {
                        response = ReceiverArgs.responseFifo.Dequeue();

                        //Console.WriteLine("Response Received: Code " + response.responseType);
                        //Console.WriteLine(response.responseStr);
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
                        /*
                        else if ((response.responseType == 103) && (Initialized))
                        {
                            gameState = JsonSerializer.Deserialize<GameState>(response.responseStr);
                            PeriodicalUpdate();
                        }*/
                    }
                }
                else if (ReceiverArgs.newDataFromServer == 103 && Initialized)
                {
                    GenericResponse response = ReceiverArgs.realtimeResponse;
                    Console.WriteLine("Response Received: Code " + response.responseType);
                    Console.WriteLine(response.responseStr);
                    gameState = JsonSerializer.Deserialize<GameState>(response.responseStr);
                    PeriodicalUpdate();
                }

                else if (ReceiverArgs.newDataFromServer == 1103)
                {
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
                            if (response.responseStr == "START")
                            {
                                playable = true;
                            }
                            else if (response.responseStr.Split('/')[0] == "END")
                            {
                                playable = false;
                                // TODO: CODI PER PARAR LA PARTIDA, extreure resultats etc.
                            }
                        }
                    }
                    if (Initialized)
                    {
                        response = ReceiverArgs.realtimeResponse;
                        //Console.WriteLine("Response Received: Code " + response.responseType);
                        //Console.WriteLine(response.responseStr);
                        gameState = JsonSerializer.Deserialize<GameState>(response.responseStr);
                        PeriodicalUpdate();
                    }
                }
                //int responseType;
                //string responseStr;
                
                ReceiverArgs.newDataFromServer = 0;
            }

        }
        else if (!Initialized)
        {
            InitTraining();
            Initialized = true;
            playable = true;
        }

        //La partida s'actualitza de forma normal
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        if ((!GameEnded) && (!GamePaused)) //Si no es pausa ni s'acaba el joc
        {
            if (Initialized)
            {
                // Actualitzem direcció i moviment del playerChar segons els inputs i les bales
                UpdateControllableCharacter(gameTime);
                if ((testMode) && (Difficulty != 'N'))
                    characterEngine.CPUDecision(scenarioSprites, projectileSprites, projectileEngine, projectileTexture, Difficulty);
            }


            //Això actualitzaria els objectes del escenari
            foreach (var ScenarioObj in scenarioSprites)
            {
                ScenarioObj.Update(gameTime);
            }

            if (Initialized)
            {
                characterEngine.Update(gameTime, slimeSprites, scenarioSprites, testMode, Controllable);
                // Això hauria de moure els projectils, calcular les colisions i notificar als characters si hi ha hagut dany.
                projectileEngine.UpdateProjectiles(gameTime, characterSprites, scenarioSprites);
            }
                // Generem les babes amb una certa espera per no sobrecarregar i les instanciem al update del personatge

                // AIXO NOMES S'HAURIA DE FER 1 COP AL INICIALITZAR!!!!!
                UpdateOnlineTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                SlimeTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            foreach (var character in characterSprites.ToArray())
            {
                character.Update(gameTime, characterSprites);
                heartManager.UpdateHealth(character.IDcharacter, character.Health);
                if ((SlimeTime > 60) && (slimeSprites.Count < 400))
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

            if ((SlimeTime > 60))
            {
                foreach (var slime in slimeSprites)
                {
                    slime.timer++;
                }
                SlimeTime = 0;
            }

            //Això hauria de fer reaccionar les babes a projectils, characters i objectes de l'escenari
            slimeEngine.UpdateSlime(gameTime, characterSprites, projectileSprites, scenarioSprites);

            //Envia missatges actualitzades
            if ((UpdateOnlineTime >= 10) && (!testMode) && (Initialized)&&(playable)) //Diferència en milisegons de cada missatge a enviar
            {
                UpdateOnlineTime = 0;
                //Actualitza el jugador 
                CharacterState characterState = new CharacterState();
                characterState.charID = Controllable.IDcharacter;
                characterState.posX = (int)Controllable.Position.X;
                characterState.posY = (int)Controllable.Position.Y;
                characterState.velX = (int)Controllable.Velocity.X;
                characterState.velY = (int)Controllable.Velocity.Y;
                characterState.dirX = Controllable.Direction.X;
                characterState.dirY = Controllable.Direction.Y;
                characterState.health = Controllable.Health;
                //Actualitzem la llista de projectils
                List<projectileState> projectileStates = new List<projectileState>();
                foreach (Projectile projectile in projectileSprites)
                {
                    if ((projectile.ShooterID == Controllable.IDcharacter) && (projectile.ProjectileOnlineUpdates < projectileOnlineUpdateThreshold) && (!projectile.IsRemoved))
                    {

                        //projectileStates.Add(new projectileState(projectile.projectileID, projectile.ShooterID, projectile.ProjectileType, (int) projectile.Position.X, (int) projectile.Position.Y, projectile.Direction.X, projectile.Direction.Y, projectile.LinearVelocity, projectile.HitCount, (int)projectile.Target.X, (int)projectile.Target.Y));
                        projectileState projectileStateAdded = new projectileState();
                        projectileStateAdded.projectileID = projectile.projectileID;
                        projectileStateAdded.shooterID = projectile.ShooterID;
                        projectileStateAdded.projectileType = projectile.ProjectileType;
                        projectileStateAdded.posX = (int)projectile.Position.X;
                        projectileStateAdded.posY = (int)projectile.Position.Y;
                        projectileStateAdded.directionX = projectile.Direction.X;
                        projectileStateAdded.directionY = projectile.Direction.Y;
                        projectileStateAdded.targetX = (int)projectile.Target.X;
                        projectileStateAdded.targetY = (int)projectile.Target.Y;
                        projectileStateAdded.hitCount = projectile.HitCount;
                        projectileStateAdded.LinearVelocity = projectile.LinearVelocity;
                        projectileStates.Add(projectileStateAdded);

                        projectile.ProjectileOnlineUpdates++;
                    }
                }
                //Genera el paquet a passar per Json
                playerUpdate playerClientUpdate = new playerUpdate();
                playerClientUpdate.characterState = characterState;
                playerClientUpdate.projectileStates = projectileStates;
                //Passa el request
                serverHandler.RequestRealTimeUpdate(playerClientUpdate);

                    
            }
            //Acaba el joc offline si es donen les condicions
            if(Initialized)
                EndOfflineGame();
            //Posa invisible els texts d'altres pantalles
            foreach (var text in otherTexts)
            {
                if (text.charID == -2)
                    text.visible = false;
                else if (text.charID == -3)
                    text.visible = false;
            }
        }
        //La partida obre un menú del joc
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        else if(GameEnded)
        {

            foreach (var character in characterSprites.ToArray())
            {
                character.Update(gameTime, characterSprites);
                character.Layer = 0.85f;
            }
                //Posa invisible els texts d'altres pantalles

            foreach (var text in otherTexts)
            {
                if (text.charID == -2)
                    text.visible = false;
                else if (text.charID == -3)
                    text.visible = true;
            }
        }
        else if(GamePaused)
        {
            //Posa invisible els tests d'altres pantalles
            foreach(var text in otherTexts)
            {
                if (text.charID == -2)
                    text.visible = true;
                else if (text.charID == -3)
                    text.visible = false;
            }
        }

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
            UpdateOnlineTime++;
        }

        //Actualitzar el temporitzador
        private void OnSlimeTimedEvent(Object source, ElapsedEventArgs e)
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
                
                Vector2 HeartPosition = HeartPosInScreen(initGame.nPlayers,i,5);
                characterEngine.AddKnownCharacter(initGame.users[i].charName, new Vector2(HeartPosition.X + 5*30/2, HeartPosition.Y - 40), 0.20f, 20, initGame.users[i].charId, Color.White);
                heartManager.CreateHeart(initGame.users[i].charId, 5, 20, slugHealth, HeartPosition);

                if (thisClient.charId == initGame.users[i].charId)
                {
                    playersNames.Add(new NameFontModel(initGame.users[i].userName, new Vector2(HeartPosition.X, HeartPosition.Y - 65), Color.Black, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.99f, initGame.users[i].charId, true));
                    playersNames.Add(new NameFontModel(initGame.users[i].userName, new Vector2(HeartPosition.X, HeartPosition.Y - 70), Color.LightGreen, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.999f, initGame.users[i].charId, true));
                    projectileManager.CreateSaltMenu(projectileMenuTexture, overlaySprites, initGame.thisUser.charId, 0.1f);
                    //Controllable = characterSprites.ElementAt(i); 
                }
                else
                {
                    playersNames.Add(new NameFontModel(initGame.users[i].userName, new Vector2(HeartPosition.X, HeartPosition.Y - 65), Color.Black, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.99f, initGame.users[i].charId, true));
                    playersNames.Add(new NameFontModel(initGame.users[i].userName, new Vector2(HeartPosition.X, HeartPosition.Y - 70), Color.White, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.999f, initGame.users[i].charId, true));
                }
                
              

            }
            bool found = false;
            for (int i = 0; i < characterSprites.Count; i++)
            {
                if (thisClient.charId == characterSprites.ElementAt(i).IDcharacter)
                {
                    found = true;
                    Controllable = characterSprites.ElementAt(i);
                }
            }
            if (!found)
            {
                Exception ex = new Exception("Error:, controllable character not found");
                throw ex;
            }
        }

        //Inicialitza entrenament

        //Retorna la posició dels cors
        public Vector2 HeartPosInScreen(int nPlayers, int i, int heartNum)
        {
            Vector2 HeartPos;

            float height = 720;

            switch (nPlayers)
            {
                case 1: //Mode entrenament individual
                    HeartPos = new Vector2(graphics.PreferredBackBufferWidth / 2 - heartNum * 30 / 2, 630);
                    break;
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
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth/2 - heartNum * 30/2, 630);
                    break;
                case 4: //4 jugadors
                    if (i < 2) //Lateral esquerra
                        HeartPos = new Vector2(50, 120 * (i*4 + 1));
                    else //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum * 30, 120 * ((i-2) * 4 + 1));
                    break;
                case 5: //5 jugadors
                    if (i < 2) //Lateral esquerra
                        HeartPos = new Vector2(50, 120 * (i * 3 + 1));
                    else if (i < 4) //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum * 30, 120 * ((i-2) * 3 + 1));
                    else //Part inferior
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth / 2 - heartNum * 30 / 2, 630);
                    break;
                case 6: //6 jugadors
                    if (i < 3) //Lateral esquerra
                        HeartPos = new Vector2(50, 120 * (i * 2 + 1));
                    else //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum * 30, 120 * ((i-3) * 2 + 1));
                    break;
                case 7: //7 jugadors
                    if (i < 3) //Lateral esquerra
                        HeartPos = new Vector2(50, 90 * (i * 2 + 1));
                    else if (i < 7) //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum * 30, 90 * ((i-3) * 2 + 1));
                    else //Part inferior
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth / 2 - heartNum * 30 / 2, 630);
                    break;
                case 8: //8 jugadors
                    if (i < 4) //Lateral esquerra
                        HeartPos = new Vector2(50, 90 * (i * 2 + 1));
                    else //Lateral dret
                        HeartPos = new Vector2(graphics.PreferredBackBufferWidth - 50 - heartNum * 30, 90 * ((i-4) * 2 + 1));
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
            if (gameState.playable == 1)
                playable = true;

            //Actualització dels personatges
            foreach (CharacterState characterState in gameState.characterStatesList)
            {
                bool found = false;
                int i = 0;
                while((!found)&&(i< characterSprites.Count))
                {
                    if ((characterState.charID == characterSprites[i].IDcharacter) && ((characterState.charID != Controllable.IDcharacter) || !playable)) //Desacobla el client de les actualitzacions
                    {
                        found = true;
                        characterSprites[i].Position = new Vector2(characterState.posX, characterState.posY);
                        characterSprites[i].Velocity = new Vector2(characterState.velX, characterState.velY);
                        characterSprites[i].Direction = new Vector2(characterState.dirX, characterState.dirY);
                        characterSprites[i].Health = characterState.health;
                    }
                    else
                        i++;
                }

                
            }

            //Actualització dels projectils
            foreach (projectileState projectileState in gameState.projectileStates)
            {
                bool found = false;
                int i = 0;
                while ((!found) && (i < projectileSprites.Count))
                {
                    //Actualitzem el projectil
                    if ((projectileState.shooterID == projectileSprites[i].ShooterID)&&(projectileState.projectileID == projectileSprites[i].projectileID) && ((projectileState.shooterID != Controllable.IDcharacter) || !playable)) //Desacobla el client de les actualitzacions
                    {
                        found = true;
                        projectileSprites[i].Position = new Vector2(projectileState.posX, projectileState.posY);
                        projectileSprites[i].Direction = new Vector2(projectileState.directionX, projectileState.directionY);
                        projectileSprites[i].Target = new Vector2(projectileState.targetX, projectileState.targetY);
                        projectileSprites[i].LinearVelocity = projectileState.LinearVelocity;
                        projectileSprites[i].ProjectileType = projectileState.projectileType;
                        projectileSprites[i].HitCount = projectileState.hitCount;
                    }
                    else
                        i++;
                }

                if((!found)&& ((projectileState.shooterID != Controllable.IDcharacter) || !playable)) //El projectil no existeix, s'ha de crear
                {
                    if (projectileState.projectileType == 'N')
                        projectileEngine.AddProjectile(new Vector2(projectileState.posX, projectileState.posY), new Vector2(projectileState.targetX, projectileState.targetY), projectileTexture["Normal"], projectileState.shooterID, projectileState.projectileType, projectileState.projectileID);
                    else if (projectileState.projectileType == 'D')
                        projectileEngine.AddProjectile(new Vector2(projectileState.posX, projectileState.posY), new Vector2(projectileState.targetX, projectileState.targetY), projectileTexture["Direct"], projectileState.shooterID, projectileState.projectileType, projectileState.projectileID);
                    else if (projectileState.projectileType == 'S')
                        projectileEngine.AddProjectile(new Vector2(projectileState.posX, projectileState.posY), new Vector2(projectileState.targetX, projectileState.targetY), projectileTexture["Slimed"], projectileState.shooterID, projectileState.projectileType, projectileState.projectileID);

                    foreach(Character chara in characterSprites)
                    {
                        if (projectileState.shooterID == chara.IDcharacter)
                            chara.BulletNumber++;
                    }
                }

                

            }


        }

        //Control dels personatges
        private void UpdateControllableCharacter(GameTime gameTime)
        {
            Controllable.Direction = VectorOps.UnitVector(inputManager.GetMousePosition() - Controllable.Position);

            if ((playable)&&(!Controllable.Defeated))
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
                    if (Controllable.BulletNumber < BulletThreshold)
                    {
                        projectileManager.AddProjectile(projOrigin, projTarget, Controllable.IDcharacter, NextProjectileID);
                        Controllable.BulletNumber++;
                        NextProjectileID++;
                    }
                }

            }
        }

        //MODE ENTRENAMENT
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //Incialitza els components amb el codi 101
        private void InitTraining()
       {
           
            characterSprites.Clear();
            for (int i = 0; i < localGameState.Opponentnum_players + 1; i++)
            {
                Vector2 HeartPosition;
                if (localGameState.Player_ID[i] == 1)
                {
                    HeartPosition = HeartPosInScreen(localGameState.Opponentnum_players + 1, i, 5);
                    characterEngine.AddKnownCharacter(localGameState.PlayerCharacter_Selected, new Vector2(HeartPosition.X + 5 * 30 / 2, HeartPosition.Y - 40), 0.20f, 20, localGameState.Player_ID[i], Color.White);
                    heartManager.CreateHeart(localGameState.Player_ID[i], 5, 20, slugHealth, HeartPosition);
                    playersNames.Add(new NameFontModel("Jugador", new Vector2(HeartPosition.X, HeartPosition.Y - 65), Color.Black, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.99f, localGameState.Player_ID[i], true));
                    playersNames.Add(new NameFontModel("Jugador", new Vector2(HeartPosition.X, HeartPosition.Y - 70), Color.LightGreen, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.999f, localGameState.Player_ID[i], true));
                    projectileManager.CreateSaltMenu(projectileMenuTexture, overlaySprites, localGameState.Player_ID[i], 0.1f);
                    Controllable = characterSprites.ToArray()[i];
                }
                else if (i!=0)
                {
                    HeartPosition = HeartPosInScreen(localGameState.Opponentnum_players + 1, i, 5);
                    characterEngine.AddKnownCharacter(localGameState.OpponentCharacter_Selected[i - 1], new Vector2(HeartPosition.X + 5 * 30 / 2, HeartPosition.Y - 40), 0.20f, 20, localGameState.Player_ID[i], Color.White, true);
                    heartManager.CreateHeart(localGameState.Player_ID[i], 5, 20, slugHealth, HeartPosition);
                    playersNames.Add(new NameFontModel("CPU " + localGameState.Player_ID[i], new Vector2(HeartPosition.X, HeartPosition.Y - 65), Color.Black, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.99f, localGameState.Player_ID[i], true));
                    playersNames.Add(new NameFontModel("CPU " + localGameState.Player_ID[i], new Vector2(HeartPosition.X, HeartPosition.Y - 70), Color.White, 0, 0.9f, new Vector2(0, 0), SpriteEffects.None, 0.999f, localGameState.Player_ID[i], true));

                }

            }

            otherTexts.Add(new NameFontModel("JOC PAUSAT", new Vector2(250, 300), Color.Black, 0f, 2f, new Vector2(0, 0), SpriteEffects.None, 0.9999f, -2, false));
            otherTexts.Add(new NameFontModel("JOC PAUSAT", new Vector2(250, 290), Color.White, 0f, 2f, new Vector2(0, 0), SpriteEffects.None, 1f, -2, false));
        }

        //Activa el final de la partida si queda només un jugador o menys al offline (excepte si el jugador ha decidit jugar sol)
        private void EndOfflineGame()
        {
            if((testMode)&&(characterSprites.Count > 1))
            {
                int characterAlive = 0;
                int IDwinnerOferted = -1;
                foreach (Character chara in characterSprites)
                {
                    if ((!chara.Defeated) && (chara.Health > 0))
                    {
                        characterAlive++;
                        IDwinnerOferted = chara.IDcharacter;
                    }
                }

                if(characterAlive == 1)
                {
                    projectileSprites.Clear();
                    slimeSprites.Clear();
                    HasWinner = true;
                    GameEnded = true;
                    IDwinner = IDwinnerOferted;
                    otherTexts.Add(new NameFontModel("GUANYADOR:", new Vector2(700, 90), Color.Black, 0f, 1f, new Vector2(0, 0), SpriteEffects.None, 0.9999f, -3, false));
                    otherTexts.Add(new NameFontModel("GUANYADOR:", new Vector2(700, 85), Color.White, 0f, 1f, new Vector2(0, 0), SpriteEffects.None, 1f, -3, false));
                }
                else if(characterAlive == 0)
                {
                    projectileSprites.Clear();
                    slimeSprites.Clear();
                    HasWinner = false;
                    GameEnded = true;
                }

                
                otherTexts.Add(new NameFontModel("Pitja ESC o el espai per sortir del joc", new Vector2(100, 650), Color.Black, 0f, 0.5f, new Vector2(0, 0), SpriteEffects.None, 0.9999f, -3, false));
                otherTexts.Add(new NameFontModel("Pitja ESC o el espai per sortir del joc", new Vector2(100, 645), Color.White, 0f, 0.5f, new Vector2(0, 0), SpriteEffects.None, 1f, -3, false));

                //Actualitzem la pantalla només un cop

                if ((HasWinner) && (GameEnded))
                {
                    foreach (Character character in characterSprites)
                    {
                        character.Direction = new Vector2(0, 1);
                        if (character.IDcharacter == IDwinner)
                        {
                            character.Scale = 0.6f;

                            character.Position = new Vector2(240, 240);
                        }
                        else
                            character.Position = new Vector2(character.IDcharacter * 120 + 60, 480);

                        character.Layer = 0.85f;
                    }
                    foreach (NameFontModel name in playersNames)
                    {
                        if (name.charID == IDwinner)
                        {
                            if (name.color == Color.Black)
                                name.Position = new Vector2(700, 190);
                            else
                                name.Position = new Vector2(700, 185);
                        }
                        else
                        {
                            name.scale = 0.25f;
                            if (name.color == Color.Black)
                                name.Position = new Vector2(name.charID * 120 + 20, 555);
                            else
                                name.Position = new Vector2(name.charID * 120 + 20, 550);
                        }
                    }
                }
                else if (GameEnded)
                {

                    foreach (Character character in characterSprites)
                    {
                        character.Direction = new Vector2(0, 1);
                        character.Layer = 0.85f;
                        character.Position = new Vector2(character.IDcharacter * 120 + 60, 480);
                    }

                    foreach (NameFontModel name in playersNames)
                    {
                        name.scale = 0.25f;
                        if (name.color == Color.Black)
                            name.Position = new Vector2(name.charID * 120 + 20, 555);
                        else
                            name.Position = new Vector2(name.charID * 120 + 20, 550);
                    }
                }
            }
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

            if ((GamePaused) || (GameEnded))
            {
                PauseImage = Content.Load<Texture2D>("Sight/DarkScreen");
                float PauseScale = Math.Max(((float)graphics.PreferredBackBufferWidth / (float)PauseImage.Width), ((float)graphics.PreferredBackBufferHeight / (float)PauseImage.Height));
                spriteBatch.Draw(PauseImage, new Vector2(0, 0), null, Color.White, 0f, new Vector2(0, 0), PauseScale, SpriteEffects.None, 0.8f);
            }

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
                if((overlay.Visible)&&((!overlay.IsHealth)||((overlay.IsHealth)&&(!GameEnded))))
                    overlay.Draw(spriteBatch);
            }
            foreach (var names in playersNames)
            {
                if(names.visible)
                    spriteBatch.DrawString(_namesFont, names.name, names.Position, names.color, names.rotation, names.origin, names.scale, names.effect, names.layer);
                
            }
            foreach (var texts in otherTexts)
            {
                if (texts.visible)
                    spriteBatch.DrawString(_namesFont, texts.name, texts.Position, texts.color, texts.rotation, texts.origin, texts.scale, texts.effect, texts.layer);

            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
