﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace BaboGame_test_2
{
    public class ScenarioObjects : Sprite
    {
        //Variables entorn a la electricitat
        public bool HasConducitivity = false;
        public char Charge = 'I'; //Positive P, Negative N, Insulating I
        
        public ScenarioObjects(Texture2D texture)
            : base(texture)
        {
            SolidObject = true;
        }

        public ScenarioObjects(Dictionary<string, Animation> animations)
           : base(animations)
        {

        }

        public void Update(GameTime gameTime)
        {
            float LayerValue = this.Position.Y / 10000;
            if (LayerValue > 0.4)
                Layer = 0.4f;
            else
                Layer = LayerValue + 0.01f;
        }

        public bool IsNear(Vector2 Position)
        {
            if((VectorOps.ModuloVector(this.Position - Position)) > (Math.Max(this._texture.Width, this._texture.Height)*this.HitBoxScale*this.Scale))
                return false;
            else
                return true;
        }

    }
}
