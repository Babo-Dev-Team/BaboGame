﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace BaboGame_test_2
{

    /*
     * Defineix el sprite del personatge que el jugador controlarà
     */
    public class CharacterEngine
    {
        List<Character> characterList;

        //Funcíó per trobar el contingut de les imatges
        ContentManager Content;
        Dictionary<string, Animation> BaboAnimations;
        Dictionary<string, Animation> LimaxAnimations;
        Dictionary<string, Animation> KalerAnimations;
        Dictionary<string, Animation> SwalotAnimations;
        static int BulletThreshold = 20;

        public CharacterEngine(List<Character> characterList, ContentManager Content)
        {
            this.characterList = characterList;

            //Imatges dels personatges
            BaboAnimations = new Dictionary<string, Animation>()
            {
                {"Slug down0", new Animation(Content.Load<Texture2D>("Babo/Babo down0"), 6) },
                {"Slug up0", new Animation(Content.Load<Texture2D>("Babo/Babo up0"), 6) },
                {"Slug down0 bck", new Animation(Content.Load<Texture2D>("Babo/Babo down0 s0"), 1) },
                {"Slug up0 bck", new Animation(Content.Load<Texture2D>("Babo/Babo up0 s0"), 1) },
                {"Slug right0", new Animation(Content.Load<Texture2D>("Babo/Babo right0"), 6) },
                {"Slug left0", new Animation(Content.Load<Texture2D>("Babo/Babo left0"), 6) },
                {"Slug down22_5", new Animation(Content.Load<Texture2D>("Babo/Babo down22_5"), 6) },
                {"Slug up22_5", new Animation(Content.Load<Texture2D>("Babo/Babo up22_5"), 6) },
                {"Slug right22_5", new Animation(Content.Load<Texture2D>("Babo/Babo right22_5"), 6) },
                {"Slug left22_5", new Animation(Content.Load<Texture2D>("Babo/Babo left22_5"), 6) },
                {"Slug down45", new Animation(Content.Load<Texture2D>("Babo/Babo down45"), 6) },
                {"Slug up45", new Animation(Content.Load<Texture2D>("Babo/Babo up45"), 6) },
                {"Slug right45", new Animation(Content.Load<Texture2D>("Babo/Babo right45"), 6) },
                {"Slug left45", new Animation(Content.Load<Texture2D>("Babo/Babo left45"), 6) },
                {"Slug down-22_5", new Animation(Content.Load<Texture2D>("Babo/Babo down-22_5"), 6) },
                {"Slug up-22_5", new Animation(Content.Load<Texture2D>("Babo/Babo up-22_5"), 6) },
                {"Slug right-22_5", new Animation(Content.Load<Texture2D>("Babo/Babo right-22_5"), 6) },
                {"Slug left-22_5", new Animation(Content.Load<Texture2D>("Babo/Babo left-22_5"), 6) },
                {"Slug down-45", new Animation(Content.Load<Texture2D>("Babo/Babo down-45"), 6) },
                {"Slug up-45", new Animation(Content.Load<Texture2D>("Babo/Babo up-45"), 6) },
                {"Slug right-45", new Animation(Content.Load<Texture2D>("Babo/Babo right-45"), 6) },
                {"Slug left-45", new Animation(Content.Load<Texture2D>("Babo/Babo left-45"), 6) },
                {"Slug up hit", new Animation(Content.Load<Texture2D>("Babo/Babo up hit"), 1) },
                {"Slug down hit", new Animation(Content.Load<Texture2D>("Babo/Babo down hit"), 1) },
                {"Slug right hit", new Animation(Content.Load<Texture2D>("Babo/Babo right hit"), 1) },
                {"Slug left hit", new Animation(Content.Load<Texture2D>("Babo/Babo left hit"), 1) },
                {"Slug defeat", new Animation(Content.Load<Texture2D>("Babo/Babo defeat"), 5) },

            };
            LimaxAnimations = new Dictionary<string, Animation>()
            {
                {"Slug down0", new Animation(Content.Load<Texture2D>("Limax/Limax down0"), 6) },
                {"Slug up0", new Animation(Content.Load<Texture2D>("Limax/Limax up0"), 6) },
                {"Slug down0 bck", new Animation(Content.Load<Texture2D>("Limax/Limax down0 s0"), 1) },
                {"Slug up0 bck", new Animation(Content.Load<Texture2D>("Limax/Limax up0 s0"), 1) },
                {"Slug right0", new Animation(Content.Load<Texture2D>("Limax/Limax right0"), 6) },
                {"Slug left0", new Animation(Content.Load<Texture2D>("Limax/Limax left0"), 6) },
                {"Slug down22_5", new Animation(Content.Load<Texture2D>("Limax/Limax down22_5"), 6) },
                {"Slug up22_5", new Animation(Content.Load<Texture2D>("Limax/Limax up22_5"), 6) },
                {"Slug right22_5", new Animation(Content.Load<Texture2D>("Limax/Limax right22_5"), 6) },
                {"Slug left22_5", new Animation(Content.Load<Texture2D>("Limax/Limax left22_5"), 6) },
                {"Slug down45", new Animation(Content.Load<Texture2D>("Limax/Limax down45"), 6) },
                {"Slug up45", new Animation(Content.Load<Texture2D>("Limax/Limax up45"), 6) },
                {"Slug right45", new Animation(Content.Load<Texture2D>("Limax/Limax right45"), 6) },
                {"Slug left45", new Animation(Content.Load<Texture2D>("Limax/Limax left45"), 6) },
                {"Slug down-22_5", new Animation(Content.Load<Texture2D>("Limax/Limax down-22_5"), 6) },
                {"Slug up-22_5", new Animation(Content.Load<Texture2D>("Limax/Limax up-22_5"), 6) },
                {"Slug right-22_5", new Animation(Content.Load<Texture2D>("Limax/Limax right-22_5"), 6) },
                {"Slug left-22_5", new Animation(Content.Load<Texture2D>("Limax/Limax left-22_5"), 6) },
                {"Slug down-45", new Animation(Content.Load<Texture2D>("Limax/Limax down-45"), 6) },
                {"Slug up-45", new Animation(Content.Load<Texture2D>("Limax/Limax up-45"), 6) },
                {"Slug right-45", new Animation(Content.Load<Texture2D>("Limax/Limax right-45"), 6) },
                {"Slug left-45", new Animation(Content.Load<Texture2D>("Limax/Limax left-45"), 6) },
                {"Slug up hit", new Animation(Content.Load<Texture2D>("Limax/Limax up hit"), 1) },
                {"Slug down hit", new Animation(Content.Load<Texture2D>("Limax/Limax down hit"), 1) },
                {"Slug right hit", new Animation(Content.Load<Texture2D>("Limax/Limax right hit"), 1) },
                {"Slug left hit", new Animation(Content.Load<Texture2D>("Limax/Limax left hit"), 1) },
                {"Slug defeat", new Animation(Content.Load<Texture2D>("Limax/Limax defeat"), 4) },

            };
            KalerAnimations = new Dictionary<string, Animation>()
            {
                {"Slug down0", new Animation(Content.Load<Texture2D>("Kaler/Kaler down0"), 6) },
                {"Slug up0", new Animation(Content.Load<Texture2D>("Kaler/Kaler up0"), 6) },
                //{"Slug down0 bck", new Animation(Content.Load<Texture2D>("Kaler/Kaler down0 s0"), 1) },
                //{"Slug up0 bck", new Animation(Content.Load<Texture2D>("Kaler/Kaler up0 s0"), 1) },
                {"Slug right0", new Animation(Content.Load<Texture2D>("Kaler/Kaler right0"), 6) },
                {"Slug left0", new Animation(Content.Load<Texture2D>("Kaler/Kaler left0"), 6) },
                {"Slug down22_5", new Animation(Content.Load<Texture2D>("Kaler/Kaler down22_5"), 6) },
                {"Slug up22_5", new Animation(Content.Load<Texture2D>("Kaler/Kaler up22_5"), 6) },
                {"Slug right22_5", new Animation(Content.Load<Texture2D>("Kaler/Kaler right22_5"), 6) },
                {"Slug left22_5", new Animation(Content.Load<Texture2D>("Kaler/Kaler left22_5"), 6) },
                {"Slug down45", new Animation(Content.Load<Texture2D>("Kaler/Kaler down45"), 6) },
                {"Slug up45", new Animation(Content.Load<Texture2D>("Kaler/Kaler up45"), 6) },
                {"Slug right45", new Animation(Content.Load<Texture2D>("Kaler/Kaler right45"), 6) },
                {"Slug left45", new Animation(Content.Load<Texture2D>("Kaler/Kaler left45"), 6) },
                {"Slug down-22_5", new Animation(Content.Load<Texture2D>("Kaler/Kaler down-22_5"), 6) },
                {"Slug up-22_5", new Animation(Content.Load<Texture2D>("Kaler/Kaler up-22_5"), 6) },
                {"Slug right-22_5", new Animation(Content.Load<Texture2D>("Kaler/Kaler right-22_5"), 6) },
                {"Slug left-22_5", new Animation(Content.Load<Texture2D>("Kaler/Kaler left-22_5"), 6) },
                {"Slug down-45", new Animation(Content.Load<Texture2D>("Kaler/Kaler down-45"), 6) },
                {"Slug up-45", new Animation(Content.Load<Texture2D>("Kaler/Kaler up-45"), 6) },
                {"Slug right-45", new Animation(Content.Load<Texture2D>("Kaler/Kaler right-45"), 6) },
                {"Slug left-45", new Animation(Content.Load<Texture2D>("Kaler/Kaler left-45"), 6) },
                {"Slug up hit", new Animation(Content.Load<Texture2D>("Kaler/Kaler up hit"), 1) },
                {"Slug down hit", new Animation(Content.Load<Texture2D>("Kaler/Kaler down hit"), 1) },
                {"Slug right hit", new Animation(Content.Load<Texture2D>("Kaler/Kaler right hit"), 1) },
                {"Slug left hit", new Animation(Content.Load<Texture2D>("Kaler/Kaler left hit"), 1) },
                {"Slug defeat", new Animation(Content.Load<Texture2D>("Kaler/Kaler defeat"), 5) },

            };
            SwalotAnimations = new Dictionary<string, Animation>()
            {
                {"Slug down0", new Animation(Content.Load<Texture2D>("Swalot/Swalot down0"), 6) },
                {"Slug up0", new Animation(Content.Load<Texture2D>("Swalot/Swalot up0"), 6) },
                //{"Slug down0 bck", new Animation(Content.Load<Texture2D>("Kaler/Kaler down0 s0"), 1) },
                //{"Slug up0 bck", new Animation(Content.Load<Texture2D>("Kaler/Kaler up0 s0"), 1) },
                {"Slug right0", new Animation(Content.Load<Texture2D>("Swalot/Swalot right0"), 6) },
                {"Slug left0", new Animation(Content.Load<Texture2D>("Swalot/Swalot left0"), 6) },
                {"Slug down22_5", new Animation(Content.Load<Texture2D>("Swalot/Swalot down22_5"), 6) },
                {"Slug up22_5", new Animation(Content.Load<Texture2D>("Swalot/Swalot up22_5"), 6) },
                {"Slug right22_5", new Animation(Content.Load<Texture2D>("Swalot/Swalot right22_5"), 6) },
                {"Slug left22_5", new Animation(Content.Load<Texture2D>("Swalot/Swalot left22_5"), 6) },
                {"Slug down45", new Animation(Content.Load<Texture2D>("Swalot/Swalot down45"), 6) },
                {"Slug up45", new Animation(Content.Load<Texture2D>("Swalot/Swalot up45"), 6) },
                {"Slug right45", new Animation(Content.Load<Texture2D>("Swalot/Swalot right45"), 6) },
                {"Slug left45", new Animation(Content.Load<Texture2D>("Swalot/Swalot left45"), 6) },
                {"Slug down-22_5", new Animation(Content.Load<Texture2D>("Swalot/Swalot down-22_5"), 6) },
                {"Slug up-22_5", new Animation(Content.Load<Texture2D>("Swalot/Swalot up-22_5"), 6) },
                {"Slug right-22_5", new Animation(Content.Load<Texture2D>("Swalot/Swalot right-22_5"), 6) },
                {"Slug left-22_5", new Animation(Content.Load<Texture2D>("Swalot/Swalot left-22_5"), 6) },
                {"Slug down-45", new Animation(Content.Load<Texture2D>("Swalot/Swalot down-45"), 6) },
                {"Slug up-45", new Animation(Content.Load<Texture2D>("Swalot/Swalot up-45"), 6) },
                {"Slug right-45", new Animation(Content.Load<Texture2D>("Swalot/Swalot right-45"), 6) },
                {"Slug left-45", new Animation(Content.Load<Texture2D>("Swalot/Swalot left-45"), 6) },
                {"Slug up hit", new Animation(Content.Load<Texture2D>("Swalot/Swalot up hit"), 1) },
                {"Slug down hit", new Animation(Content.Load<Texture2D>("Swalot/Swalot down hit"), 1) },
                {"Slug right hit", new Animation(Content.Load<Texture2D>("Swalot/Swalot right hit"), 1) },
                {"Slug left hit", new Animation(Content.Load<Texture2D>("Swalot/Swalot left hit"), 1) },
                {"Slug defeat", new Animation(Content.Load<Texture2D>("Swalot/Swalot defeat"), 6) },

            };
        }

        public void Update(GameTime gameTime, List<Slime> slimeList, List<ScenarioObjects> scenarioList, bool testmode, Character Controllable)
        {
            foreach(var character in characterList)
            {
                if ((character.IDcharacter == Controllable.IDcharacter) || (testmode))
                {

                    
                    // si tenim un impacte, desplaçament per impacte
                    if (character.isHit)
                    {
                        character.Velocity = Vector2.Zero; //Per compensar la cancelació de la velocitat la posaré abans d'agafar la direcció del cop
                        character.Velocity.X += character.hitImpulse * character.hitDirection.X / 5;
                        character.Velocity.Y += character.hitImpulse * character.hitDirection.Y / 5;
                    }
                    else
                    {
                        character.SlugSlipUpdate();
                        character.UpdateFriction();
                        character.UpdateForce();
                        character.UpdateVelocity();
                    }


                    character.UpdateCollision(scenarioList);
                    character.UpdateCharCollision(characterList, testmode);
                    character.Position += character.Velocity;


                    character.VelocityInform = character.Velocity;
                    //Velocity = Vector2.Zero; -- Ara la velocitat es conservarà
                    character.Acceleration = Vector2.Zero;

                    //Fem que els llimacs rellisquin amb les babes dels contrincants
                    foreach (var slime in slimeList)
                    {
                        if (character.DetectCollisions(slime) && (slime.ShooterID != character.IDcharacter))
                            character.isSlip = true;
                    }

                    //Bloquegem els límits de la pantalla perquè els personatges no els sobrepassin
                    if (character.Position.X < 30)
                        character.Position = new Vector2(30,character.Position.Y);
                    else if (character.Position.X > 1250)
                        character.Position = new Vector2(1250, character.Position.Y);

                    if (character.Position.Y < 30)
                        character.Position = new Vector2(character.Position.X, 30);
                    else if (character.Position.Y > 690)
                        character.Position = new Vector2(character.Position.X, 690);
                }
            }
        }

        public void CPUDecision(List<ScenarioObjects> scenarioList, List<Projectile> projectileList,ProjectileEngine projectileEngine, Dictionary<string,Texture2D> projectileTexture, char Difficulty, GameTime gameTime)
        {
            foreach (var character in characterList)
            {
                Random EnemyShoot = new Random();
                if ((character.CPU)&&(!character.Defeated))
                {
                    //Apunta al jugador més proper
                    int j = 0;
                    bool trobat = false;
                    Character nearest = character;

                    //Buscant el primer enemic de la llista
                    while ((j < characterList.Count) && (!trobat))
                    {
                        if ((characterList[j].IDcharacter != character.IDcharacter)&&(!characterList[j].Defeated))
                        {
                            trobat = true;
                            nearest = characterList[j];
                        }
                        else
                            j++;
                    }

                    if (trobat)
                    {
                        Vector2 distancePlayers = character.Position - nearest.Position;
                        //Buscant el enemic més proper de la llista
                        foreach (var opponent in characterList)
                        {
                            if ((opponent.IDcharacter != character.IDcharacter) && (!opponent.Defeated))
                            {
                                Vector2 newDistance = character.Position - opponent.Position;
                                if (VectorOps.ModuloVector(distancePlayers) > VectorOps.ModuloVector(newDistance))
                                {
                                    nearest = opponent;
                                    distancePlayers = character.Position - nearest.Position;
                                }
                            }
                        }

                        //Llimac apuntant el enemic
                        character.Direction = VectorOps.UnitVector(nearest.Position - character.Position);

                        //Llimac mantenint les distàncies al enemic
                        if (VectorOps.ModuloVector(distancePlayers) > 400)
                            character.Acceleration = character.Direction * character.LinearAcceleration;
                        else if (VectorOps.ModuloVector(distancePlayers) < 200)
                            character.Acceleration = new Vector2(-character.Direction.X, -character.Direction.Y) * character.LinearAcceleration;
                        //Llimac evitant parets
                        if (character.CPUBulletLost)
                        {
                            if(character.CPUBulletLostDirection)
                                character.Acceleration = new Vector2(character.Direction.Y, character.Direction.X) * character.LinearAcceleration;
                            else
                                character.Acceleration = new Vector2(-character.Direction.Y, -character.Direction.X) * character.LinearAcceleration;
                        }

                        //Llimac disparant el enemic amb una certa probabilitat d'error
                        switch (Difficulty)
                        {
                            case 'E':
                                if ((EnemyShoot.Next(0, 32) == 0) && (character.BulletNumber < BulletThreshold))
                                {
                                    projectileEngine.AddProjectile(character.Position, nearest.Position + new Vector2(EnemyShoot.Next(-100, 100), EnemyShoot.Next(-100, 100)), projectileTexture["Normal"], character.IDcharacter, 'N', 0);
                                    character.BulletNumber++;
                                }
                                break;
                            case 'M':
                                if ((EnemyShoot.Next(0, 32) == 0) && (character.BulletNumber < BulletThreshold))
                                {
                                    projectileEngine.AddProjectile(character.Position, nearest.Position + new Vector2(EnemyShoot.Next(-50, 50), EnemyShoot.Next(-50, 50)), projectileTexture["Normal"], character.IDcharacter, 'N', 0);
                                    character.BulletNumber++;
                                }
                                break;
                            case 'D':
                                if ((EnemyShoot.Next(0, 16) == 0) && (character.BulletNumber < BulletThreshold))
                                {
                                    projectileEngine.AddProjectile(character.Position, nearest.Position + new Vector2(EnemyShoot.Next(-5, 5), EnemyShoot.Next(-5, 5)), projectileTexture["Direct"], character.IDcharacter, 'D', 0);
                                    character.BulletNumber++;
                                }
                                break;
                            case 'I':
                                if ((EnemyShoot.Next(0, 8) == 0) && (character.BulletNumber < BulletThreshold))
                                { 
                                    projectileEngine.AddProjectile(character.Position, nearest.Position, projectileTexture["Direct"], character.IDcharacter, 'D', 0);
                                    character.BulletNumber++;
                                }
                                break;
                        }
                        
                    }


                    //Tècnica del llimac per esquivar bales properes per tal de no ser colpejat
                    float projectiledistance = 200;
                    Vector2 badProjectile = new Vector2();
                    foreach (var projectile in projectileList)
                    {
                        
                        if (projectile.IsNear(character.Position, 200)&&(projectiledistance > VectorOps.ModuloVector(character.Position - projectile.Position))&&(projectile.ShooterID != character.IDcharacter))
                        {
                            //Obtenir les dades de la dirrecció de la bala i l'angle perillós
                            projectiledistance = VectorOps.ModuloVector(character.Position - projectile.Position);
                            badProjectile = projectile.Position;
                            float badAngle = VectorOps.Vector2ToDeg(character.Position - projectile.Position);
                            Vector2 badAngleDirection = VectorOps.UnitVector(character.Position - projectile.Position);
                            float projectileAngle = VectorOps.Vector2ToDeg(projectile.Direction);

                            //Esquivar
                            if(((projectileAngle < badAngle)&&(projectileAngle > badAngle - 50))|| ((projectileAngle < badAngle + 360) && (projectileAngle > badAngle + 360 - 50)) || ((projectileAngle < badAngle - 360) && (projectileAngle > badAngle - 360 - 50)))
                                character.Acceleration = VectorOps.UnitVector(new Vector2(badAngleDirection.Y, badAngleDirection.X)) * character.LinearAcceleration;
                            else if (((projectileAngle < badAngle + 50) && (projectileAngle > badAngle)) || ((projectileAngle < badAngle + 360 + 50) && (projectileAngle > badAngle + 360)) || ((projectileAngle < badAngle - 360 + 50) && (projectileAngle > badAngle - 360)))
                                character.Acceleration = VectorOps.UnitVector(new Vector2(-badAngleDirection.Y,-badAngleDirection.X)) * character.LinearAcceleration;


                        }
                    }

                    //Tècnica del llimac per bloquejar bales i evitar ser colpejat amb una certa probabilitat d'error
                    
                    switch (Difficulty)
                    {
                        case 'E':
                            if ((EnemyShoot.Next(0, 32) == 0) && (projectiledistance < 200) && (character.BulletNumber < BulletThreshold))
                            {
                                projectileEngine.AddProjectile(character.Position, badProjectile + new Vector2(EnemyShoot.Next(-20, 20), EnemyShoot.Next(-20, 20)), projectileTexture["Normal"], character.IDcharacter, 'N', 0);
                                character.BulletNumber++;
                            }
                            break;
                        case 'M':
                            if ((EnemyShoot.Next(0, 16) == 0) && (projectiledistance < 200) && (character.BulletNumber < BulletThreshold))
                            {
                                projectileEngine.AddProjectile(character.Position, badProjectile + new Vector2(EnemyShoot.Next(-5, 5), EnemyShoot.Next(-20, 20)), projectileTexture["Normal"], character.IDcharacter, 'N', 0);
                                character.BulletNumber++;
                            }
                            break;
                        case 'D':
                            if ((EnemyShoot.Next(0, 8) == 0) && (projectiledistance < 200) && (character.BulletNumber < BulletThreshold))
                            {
                                projectileEngine.AddProjectile(character.Position, badProjectile, projectileTexture["Direct"], character.IDcharacter, 'D', 0);
                                character.BulletNumber++;
                            }
                            break;
                        case 'I':
                            if ((EnemyShoot.Next(0, 4) == 0) && (projectiledistance < 200) && (character.BulletNumber < BulletThreshold))
                            {
                                projectileEngine.AddProjectile(character.Position, badProjectile, projectileTexture["Direct"], character.IDcharacter, 'D', 0);
                                character.BulletNumber++;
                            }
                            break;
                    }

                    //Temps on esquiva la bala
                    if (character.CPUBulletLost)
                        character.CPUtimer += gameTime.ElapsedGameTime.Milliseconds;

                    if (character.CPUtimer > 500)
                    {
                        character.CPUtimer = 0;
                        character.CPUBulletLost = false;

                        if (character.CPUBulletLostDirection)
                            character.CPUBulletLostDirection = false;
                        else
                            character.CPUBulletLostDirection = true;
                    }
                }
            }

        }

        public void AddCharacter(Dictionary<string, Animation> slugAnimations, Vector2 Position, float Scale, float HitBoxScaleW, float HitBoxScaleH, int Health, int IDCharacter, Color color)
        {
            characterList.Add(new Character(slugAnimations,Position,Scale,HitBoxScaleW,HitBoxScaleH,Health,IDCharacter,color));
        }

        public void AddCharacter(Dictionary<string, Animation> slugAnimations, Vector2 Position, float Scale, float HitBoxScale, int Health, int IDCharacter, Color color)
        {
            characterList.Add(new Character(slugAnimations, Position, Scale, HitBoxScale, Health, IDCharacter, color));
        }

        //Cerca llimacs ja coneguts
        public void AddKnownCharacter(string slugName, Vector2 Position, float Scale, int Health, int IDCharacter, Color color)
        {
            if (slugName == "Babo")
                characterList.Add(new Character(BaboAnimations, Position, Scale, 0.6f, Health, IDCharacter, color) { Weight = 10, Velocity_Threshold = 12, LinearAcceleration = 2f, Attack = 1f, Defense = 10f, charType = 'B' });
            else if (slugName == "Limax")
                characterList.Add(new Character(LimaxAnimations, Position, Scale, 0.5f, Health, IDCharacter, color) { Weight = 8, Velocity_Threshold = 16, LinearAcceleration = 4f, Attack = 0.8f, Defense = 7f, charType = 'L'});
            else if (slugName == "Kaler")
                characterList.Add(new Character(KalerAnimations, Position, Scale, 0.5f, Health, IDCharacter, color) {Weight = 6, Velocity_Threshold = 12, LinearAcceleration = 2.5f, Attack = 1.2f, Defense = 9f, charType = 'K'});
            else if (slugName == "Swalot")
                characterList.Add(new Character(SwalotAnimations, Position, Scale, 0.7f, Health, IDCharacter, color) { Weight = 14, Velocity_Threshold = 10, LinearAcceleration = 1.5f, Attack = 1.8f, Defense = 15f, charType = 'S'});
        }

        public void AddKnownCharacter(string slugName, Vector2 Position, float Scale, int Health, int IDCharacter, Color color, bool CPUgame)
        {
            if (slugName == "Babo")
                characterList.Add(new Character(BaboAnimations, Position, Scale, 0.6f, Health, IDCharacter, color) { Weight = 10, Velocity_Threshold = 12, LinearAcceleration = 2f, CPU = CPUgame, Attack = 1f, Defense = 10f, charType = 'B'});
            else if (slugName == "Limax")
                characterList.Add(new Character(LimaxAnimations, Position, Scale, 0.5f, Health, IDCharacter, color) { Weight = 8, Velocity_Threshold = 16, LinearAcceleration = 4f, CPU = CPUgame, Attack = 0.8f, Defense = 7f, charType = 'L'});
            else if (slugName == "Kaler")
                characterList.Add(new Character(KalerAnimations, Position, Scale, 0.5f, Health, IDCharacter, color) { Weight = 6, Velocity_Threshold = 12, LinearAcceleration = 2.5f, CPU = CPUgame, Attack = 1.2f, Defense = 9f, charType = 'K'});
            else if (slugName == "Swalot")
                characterList.Add(new Character(SwalotAnimations, Position, Scale, 0.7f, Health, IDCharacter, color) { Weight = 14, Velocity_Threshold = 10, LinearAcceleration = 1.5f, CPU = CPUgame, Attack = 1.8f, Defense = 15f, charType = 'S'});
        }

        public void DisposeAll()
        {
            foreach(var item in BaboAnimations)
            {
                item.Value.Texture.Dispose();
            }
            foreach (var item in LimaxAnimations)
            {
                item.Value.Texture.Dispose();
            }
            foreach (var item in KalerAnimations)
            {
                item.Value.Texture.Dispose();
            }
            foreach (var item in SwalotAnimations)
            {
                item.Value.Texture.Dispose();
            }
        }
    }

    public class Character : Sprite
    {
        public int Health = 20;

        //Nous valors, farem les físiques dels llimacs més físicament realistes per millorar les mecàniques dels Babos
        public float Weight = 10; 
        public Vector2 Acceleration = new Vector2(0,0); 
        public float LinearAcceleration = 2f;
        public Vector2 Force = new Vector2(0,0);
        private float Friction = 1f;
        public float Velocity_Threshold = 12f;
        public bool CPU = false;
        public bool Defeated;
        public int BulletNumber;
        public bool CPUBulletLost = false;
        public float CPUtimer = 0;
        public bool CPUBulletLostDirection = false;
        public float Defense;
        public float Attack;
        public char charType; //B Babo, L Limax, K Kaler, S Swalot
        public int NextProjectileID;
        public bool SlugHability = false;
        public float HabilityRefresh = 0f;
        Random randomDamage = new Random();

        //animació Limax
        public AnimationManager VisualShadowAnimationManager;
        public Vector2 VisualShadowPosition;
        public Vector2 LastPosition;
        public Vector2 LastDirection;
        public Vector2 VisualShadowDirection;
        public float LastLayer;
        public float VisualShadowTimer = 0f;
        public bool visualShadowVisibility = false;

        //habilitat Babo
        public char charCopied = 'B';

        // Constructors
        public Character(Texture2D texture)
            : base(texture)
        {
            isHit = false;
            Defeated = false;
            BulletNumber = 0;
            NextProjectileID = 0;
        }

        public Character(Dictionary<string, Animation> animations)
           : base(animations)
        {
            isHit = false;
            Defeated = false;
            BulletNumber = 0;
            NextProjectileID = 0;

            //Animació Limax
            VisualShadowAnimationManager = new AnimationManager(animations.First().Value) {Ascale = Scale, Aeffects = Effect, };
        }

        public Character(Dictionary<string, Animation> animations, Vector2 _Position, float _Scale, float _HitBoxScaleW, float _HitBoxScaleH, int _Health, int _IDcharacter, Color _Color)
           : base(animations)
        {
            Position = _Position;
            Scale = _Scale;
            HitBoxScaleW = _HitBoxScaleW;
            HitBoxScaleH = _HitBoxScaleH;
            Health= _Health;
            IDcharacter = _IDcharacter;
            _color = _Color;
            isHit = false;
            Defeated = false;
            BulletNumber = 0;
            NextProjectileID = 0;

            //Animació Limax
            VisualShadowAnimationManager = new AnimationManager(animations.First().Value) { Ascale = Scale, Aeffects = Effect, };
        }

        public Character(Dictionary<string, Animation> animations, Vector2 _Position, float _Scale, float _HitBoxScale, int _Health, int _IDcharacter, Color _Color)
           : base(animations)
        {
            Position = _Position;
            Scale = _Scale;
            HitBoxScale = _HitBoxScale;
            Health = _Health;
            IDcharacter = _IDcharacter;
            _color = _Color;
            isHit = false;
            Defeated = false;
            BulletNumber = 0;
            NextProjectileID = 0;

            //Animació Limax
            VisualShadowAnimationManager = new AnimationManager(animations.First().Value) { Ascale = Scale, Aeffects = Effect, };
        }

        // interfície pública per moure el character
        public void MoveLeft()
        {
            if (!isHit)
            {
                //this.Velocity.X -= this.LinearVelocity;
                if(!isSlip)
                    this.Acceleration.X -= this.LinearAcceleration;
                else
                    this.Acceleration.X -= this.LinearAcceleration/4;
            }
        }
        public void MoveRight()
        {
            if (!isHit)
            {
                //this.Velocity.X += this.LinearVelocity;
                if(!isSlip)
                    this.Acceleration.X += this.LinearAcceleration;
                else
                    this.Acceleration.X += this.LinearAcceleration/4;
            }
        }
        public void MoveUp()
        {
            if (!isHit)
            {
                //this.Velocity.Y -= this.LinearVelocity;
                if(!isSlip)
                    this.Acceleration.Y -= this.LinearAcceleration;
                else
                    this.Acceleration.Y -= this.LinearAcceleration/4;
            }
        }
        public void MoveDown()
        {
            if (!isHit)
            {
                //this.Velocity.Y += this.LinearVelocity;
                if(!isSlip)
                    this.Acceleration.Y += this.LinearAcceleration;
                else
                    this.Acceleration.Y += this.LinearAcceleration/4;
            }
        }

        // Identificadors per la colisió amb projectils
        // mètode públic per restringir l'accés al flag isHit (mantenir-lo privat)
        public bool isHit;
        public bool IsHit()
        {
            return this.isHit;
        }
        // Identificar la relliscada amb les babes del contrincant
        public bool isSlip;
        //private Vector2 _previousLinearVelocity;
        //private Vector2 _previousDirection;

        public Vector2 hitDirection;
        public float hitImpulse;
        float _PainTimer = 0f;
        public Vector2 VelocityInform;

        // Detectem colisions i actualitzem posicions, timers i flags del character
        public void Update(GameTime gameTime, List<Character> characterSprites)
        {
            if (isHit)
                _PainTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_PainTimer > 0.3f)
            {
                _PainTimer = 0f;
                isHit = false;
            }

            if (Health <= 0)
            {
                Defeated = true;
            }
            else
                Defeated = false;

            //Reprodueix l'animació
            SetAnimations();
            float Framespeed;
            if (!Defeated)
                Framespeed = 0.2f * 4 / (4 + VectorOps.ModuloVector(VelocityInform));
            else
                Framespeed = 0.5f;

            this._animationManager.Update(gameTime, Framespeed);

            //Reprodueix l'animació del Limax del Dash
            if((VectorOps.ModuloVector(Position - LastPosition) > 70)&&((charType == 'L')||(charType == 'B')))
            {
                visualShadowVisibility = true;
                VisualShadowTimer = 0f;
                VisualShadowPosition = new Vector2(LastPosition.X, LastPosition.Y);
                VisualShadowDirection = new Vector2(LastDirection.X, LastDirection.Y);
                VisualShadowAnimationManager.ALayer = LastLayer;
            }

            if(visualShadowVisibility)
            {
                VisualShadowAnimationManager.Position = VisualShadowPosition;
                SetVisualShadowAnimations();
                VisualShadowAnimationManager.Update(gameTime, Framespeed);
                VisualShadowTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (VisualShadowTimer > 500f)
                {
                    VisualShadowTimer = 0f;
                    visualShadowVisibility = false;
                }
            }

            //"Equació" per definir a quina capa es mostrarà el "sprite" perquè un personatge no li estigui trapitjant la cara al altre
            float LayerValue = this.Position.Y / 10000;
            if (LayerValue > 0.4)
                Layer = 0.4f;
            else
                Layer = LayerValue + 0.01f;
        }

       
        // detectem colisions amb altres jugadors
        public bool DetectCharCollisions(List<Character> charSprites)
        {
            bool collisionDetected = false;
            foreach (var character in charSprites)
            if (character != this)
            {
                collisionDetected = this.IsTouchingBottom(character) || this.IsTouchingLeft(character) 
                                  || this.IsTouchingRight(character) || this.IsTouchingTop(character);
            }
            return collisionDetected;
        }

        //------------------------------------------- Funcions per les noves mecàniques -----------------------------------------------
        //Calcula el vector de força
        public void UpdateForce()
        {
            Force = Acceleration*Weight;
        }
        //Actualitza la velocitat segons l'acceleració actual
        public void UpdateVelocity()
        {
            Velocity += Acceleration;
            if(VectorOps.ModuloVector(Velocity) > Velocity_Threshold)
                Velocity = VectorOps.UnitVector(Velocity)*Velocity_Threshold;
        }
        //Aplica els efectes del fregament en el moviment
        public void UpdateFriction()
        {
            float VelocityModulo = VectorOps.ModuloVector(Velocity);
            VelocityModulo -= Friction;
            if (VelocityModulo > 0)
                Velocity = VectorOps.UnitVector(Velocity)*VelocityModulo;
            else
                Velocity = Vector2.Zero;
        }
        //Modifica el fregament segons si el llimac rellisca o no
        public void SlugSlipUpdate()
        {
            if(isSlip)
                Friction = 0.01f;
            else
                Friction = 1f;
        }

        //Actualitza la collisió
        public void UpdateCharCollision(List<Character> charSprites, bool testmode)
        {
            foreach (var character in charSprites)
            {
                if(character != this)
                {
                    if(this.IsTouchingBottom(character) || this.IsTouchingTop(character))
                    {
                        if(testmode)
                            Velocity.Y = character.Force.Y/character.Weight;
                        else
                        {
                            if ((Velocity.Y > 0) && (character.Velocity.Y > 0) || (Velocity.Y < 0) && (character.Velocity.Y < 0))
                                Velocity.Y = 0;
                            else
                                Velocity.Y = character.Velocity.Y * character.Weight / Weight;
                        }

                    }
                    if (this.IsTouchingLeft(character) || this.IsTouchingRight(character))
                    {
                        if(testmode)
                            Velocity.X = character.Force.X/character.Weight;
                        else
                        {
                            if ((Velocity.X > 0) && (character.Velocity.X > 0) || (Velocity.X < 0) && (character.Velocity.X < 0))
                                Velocity.X = 0;
                            else
                                Velocity.X = character.Velocity.X * character.Weight / Weight;
                        }
                            
                    }
                }
            }
        }
        
        //Detectar la col·lisió amb altres objectes
        public bool DetectCollisions(Sprite sprite)
        {
            bool collisionDetected = this.IsTouchingBottom(sprite) || this.IsTouchingLeft(sprite)
                                      || this.IsTouchingRight(sprite) || this.IsTouchingTop(sprite);
                
            return collisionDetected;
        }
        
        //Actualitza la collisió dels objectes
        public void UpdateCollision(List<ScenarioObjects> scenarioObjects)
        {
            foreach (var objectItem in scenarioObjects)
            {
                if (objectItem.SolidObject)
                {
                    //Eix Y
                    if (this.IsTouchingBottom(objectItem) && (this.Force.Y < 0) || this.IsTouchingTop(objectItem) && (this.Force.Y > 0))
                    {
                        this.Velocity.Y = -this.Force.Y / this.Weight;

                    } //Evitem el glith atravessa-parets
                    else if (this.IsTouchingBottom(objectItem) || this.IsTouchingTop(objectItem))
                    {
                        this.Velocity.Y = 0;
                    }

                    //Eix X
                    if (this.IsTouchingLeft(objectItem) && (this.Force.X < 0) || this.IsTouchingRight(objectItem) && (this.Force.X > 0))
                    {
                        this.Velocity.X = -this.Force.X / this.Weight;

                    } //Evitem el glith atravessa-parets
                    else if (this.IsTouchingLeft(objectItem) || this.IsTouchingRight(objectItem))
                    {
                        this.Velocity.X = 0;
                    }
                }
            }
        }

        //Apartat de les animacions
        protected virtual void SetAnimations()
        {

            if (isHit)
            {
                float angle = VectorOps.Vector2ToDeg(Direction);
                //Animació de ser colpejat per la salt
                if (angle < 315 && angle > 225)
                    _animationManager.Play(_animations["Slug up hit"]);
                else if (angle >= 315 || angle < 45)
                    _animationManager.Play(_animations["Slug right hit"]);
                else if (angle <= 225 && angle > 135)
                    _animationManager.Play(_animations["Slug left hit"]);
                else
                    _animationManager.Play(_animations["Slug down hit"]);
            }
            else if (Defeated)
            {
                _animationManager.Play(_animations["Slug defeat"]);
            }
            else
            {
                // Detecció del angle de dispar amb la corresponent animació (probablement s'haurà de fer de forma més eficient) 
                // Angle entre animacions: 18 graus || pi/10 radiants -- Desfasament: 9 graus || pi/20 radiant
                
                float angle = VectorOps.Vector2ToDeg(Direction);
                if ((angle <= 9 && angle >= 0) || (angle <= 360 && angle > 351))
                    _animationManager.Play(_animations["Slug right0"]);
                else if (angle <= 27 && angle > 9)
                    _animationManager.Play(_animations["Slug right-22_5"]);
                else if (angle <= 45 && angle > 27)
                    _animationManager.Play(_animations["Slug right-45"]);
                else if (angle <= 63 && angle > 45)
                    _animationManager.Play(_animations["Slug down45"]);
                else if (angle <= 81 && angle > 63)
                    _animationManager.Play(_animations["Slug down22_5"]);
                else if (angle <= 99 && angle > 81)
                    _animationManager.Play(_animations["Slug down0"]);
                else if (angle <= 117 && angle > 99)
                    _animationManager.Play(_animations["Slug down-22_5"]);
                else if (angle <= 135 && angle > 117)
                    _animationManager.Play(_animations["Slug down-45"]);
                else if (angle <= 153 && angle > 135)
                    _animationManager.Play(_animations["Slug left45"]);
                else if (angle <= 171 && angle > 153)
                    _animationManager.Play(_animations["Slug left22_5"]);
                else if (angle <= 189 && angle > 171)
                    _animationManager.Play(_animations["Slug left0"]);
                else if (angle <= 207 && angle > 189)
                    _animationManager.Play(_animations["Slug left-22_5"]);
                else if (angle <= 225 && angle > 207)
                    _animationManager.Play(_animations["Slug left-45"]);
                else if (angle <= 243 && angle > 225)
                    _animationManager.Play(_animations["Slug up45"]);
                else if (angle <= 261 && angle > 243)
                    _animationManager.Play(_animations["Slug up22_5"]);
                else if (angle <= 279 && angle > 261)
                    _animationManager.Play(_animations["Slug up0"]);
                else if (angle <= 297 && angle > 279)
                    _animationManager.Play(_animations["Slug up-22_5"]);
                else if (angle <= 315 && angle > 297)
                    _animationManager.Play(_animations["Slug up-45"]);
                else if (angle <= 333 && angle > 315)
                    _animationManager.Play(_animations["Slug right45"]);
                else if (angle <= 351 && angle > 333)
                    _animationManager.Play(_animations["Slug right22_5"]);
                else
                    _animationManager.Play(_animations["Slug down0"]);
            }
        }

        //Apartat de les animacions
        protected virtual void SetVisualShadowAnimations()
        {
                // Detecció del angle de dispar amb la corresponent animació (probablement s'haurà de fer de forma més eficient) 
                // Angle entre animacions: 18 graus || pi/10 radiants -- Desfasament: 9 graus || pi/20 radiant

                float angle = VectorOps.Vector2ToDeg(VisualShadowDirection);
                if ((angle <= 9 && angle >= 0) || (angle <= 360 && angle > 351))
                    VisualShadowAnimationManager.Play(_animations["Slug right0"]);
                else if (angle <= 27 && angle > 9)
                    VisualShadowAnimationManager.Play(_animations["Slug right-22_5"]);
                else if (angle <= 45 && angle > 27)
                    VisualShadowAnimationManager.Play(_animations["Slug right-45"]);
                else if (angle <= 63 && angle > 45)
                    VisualShadowAnimationManager.Play(_animations["Slug down45"]);
                else if (angle <= 81 && angle > 63)
                    VisualShadowAnimationManager.Play(_animations["Slug down22_5"]);
                else if (angle <= 99 && angle > 81)
                    VisualShadowAnimationManager.Play(_animations["Slug down0"]);
                else if (angle <= 117 && angle > 99)
                    VisualShadowAnimationManager.Play(_animations["Slug down-22_5"]);
                else if (angle <= 135 && angle > 117)
                    VisualShadowAnimationManager.Play(_animations["Slug down-45"]);
                else if (angle <= 153 && angle > 135)
                    VisualShadowAnimationManager.Play(_animations["Slug left45"]);
                else if (angle <= 171 && angle > 153)
                    VisualShadowAnimationManager.Play(_animations["Slug left22_5"]);
                else if (angle <= 189 && angle > 171)
                    VisualShadowAnimationManager.Play(_animations["Slug left0"]);
                else if (angle <= 207 && angle > 189)
                    VisualShadowAnimationManager.Play(_animations["Slug left-22_5"]);
                else if (angle <= 225 && angle > 207)
                    VisualShadowAnimationManager.Play(_animations["Slug left-45"]);
                else if (angle <= 243 && angle > 225)
                    VisualShadowAnimationManager.Play(_animations["Slug up45"]);
                else if (angle <= 261 && angle > 243)
                    VisualShadowAnimationManager.Play(_animations["Slug up22_5"]);
                else if (angle <= 279 && angle > 261)
                    VisualShadowAnimationManager.Play(_animations["Slug up0"]);
                else if (angle <= 297 && angle > 279)
                    VisualShadowAnimationManager.Play(_animations["Slug up-22_5"]);
                else if (angle <= 315 && angle > 297)
                    VisualShadowAnimationManager.Play(_animations["Slug up-45"]);
                else if (angle <= 333 && angle > 315)
                    VisualShadowAnimationManager.Play(_animations["Slug right45"]);
                else if (angle <= 351 && angle > 333)
                    VisualShadowAnimationManager.Play(_animations["Slug right22_5"]);
                else
                    VisualShadowAnimationManager.Play(_animations["Slug down0"]);
            
        }

        //Notifica el dolor
        public void NotifyHit(Vector2 hitDirection, int shooterID, float damage, float hitImpulse, float shooterAttack, char charType)
        {
            this.isHit = true;
            float randomValue = (float) randomDamage.NextDouble();
            float randomCritical = (float)randomDamage.NextDouble();

            if (((randomCritical < 0.036)&&(charType == 'B'))|| ((randomCritical < 0.018) && (charType == 'L'))|| ((randomCritical < 0.06) && (charType == 'K'))|| ((randomCritical < 0.006) && (charType == 'S')))
                this.Health -= (int)damage / 2;
            else
            {
                if(randomValue* randomValue * shooterAttack * damage/this.Defense > 3)
                    this.Health -= 4;
                else if(randomValue * randomValue* shooterAttack * damage / this.Defense > 2)
                    this.Health -= 3;
                else if (randomValue* randomValue * shooterAttack * damage / this.Defense > 1)
                    this.Health -= 2;
                else 
                    this.Health -= 1;
            }

            this.hitDirection = hitDirection;
            this.hitImpulse = hitImpulse;
        }

        //Notifica el CPU que s'ha quedat aturat a una paret
        public void CPUNotifyLostBullet()
        {
            CPUBulletLost = true;
        }

        //Mostra l'animació de la ombra visual de Limax
        public void VisualShadowDraw(SpriteBatch spriteBatch)
        {
            if (_texture != null)
                spriteBatch.Draw(_texture, VisualShadowPosition, null, Color.Aqua, _rotation, Origin, Scale, Effect, Layer);
            else if (VisualShadowAnimationManager != null)
            {
                VisualShadowAnimationManager.Ascale = Scale;
                VisualShadowAnimationManager.ALayer = Layer;
                VisualShadowAnimationManager.Aeffects = Effect;
                VisualShadowAnimationManager.Acolor = Color.Aqua;
                VisualShadowAnimationManager.AHitBoxScale = HitBoxScale;

                VisualShadowAnimationManager.Draw(spriteBatch);
            }
            else throw new Exception("No tens cap textura per aquest sprite");
        }

    }
}