using BEPUphysics.Entities.Prefabs;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace Coursework
{
    public class Enemy
    {
        private Game game;
        public Player player;
        public Vector3 enemyPos = new Vector3(0f, 10.0f, -10f);
        public Box enemyColBox;
        public EntityModel enemyModel;
        public Matrix enemyWorld;
        public SoundEffect hit;


        public Enemy(Game1 game)
        {
            this.game = game;
            enemyColBox = new Box(enemyPos, 3f, 3f, 3f);
            game.space.Add(enemyColBox);
            //enemyColBox.Mass = 2.0f;
            //enemyColBox.IsAffectedByGravity = false;
            //shipColBox.BecomeDynamic(1);          
            //shipColBox.LinearVelocity = new Vector3(0,-100f,0f);
            enemyModel = new EntityModel(enemyColBox, game.Content.Load<Model>("Models/enemyShip1"), Matrix.Identity * Matrix.CreateScale(0.03f), game);
            hit = game.Content.Load<SoundEffect>("Audio/Hit");
            game.Components.Add(enemyModel);
            enemyColBox.Tag = enemyModel;
        }







    }
}