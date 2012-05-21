using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Barebones.Components;
using Meat.Input;
using Barebones.Dependencies;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace _2dgame.Components
{
    class Eleve : EntityComponent, Barebones.Framework.IUpdateable
    {
        KeyboardReader m_Keyboard;
        PhysicsComponent m_Physics;

        readonly float m_Speed;
        readonly float m_JumpForce;

        public Eleve(float speed, float jumpforce)
        {
            m_Speed = speed;
            m_JumpForce = jumpforce;
        }

        public override IEnumerable<Barebones.Dependencies.IDependency> GetDependencies()
        {
            yield return new Dependency<KeyboardReader>( item => m_Keyboard = item );
            yield return new Dependency<PhysicsComponent>(item => m_Physics = item);
        }

        protected override void OnOwnerSet()
        {
            Owner.Engine.Forum.RegisterListener<KeyPressed>(OnKeyPressed);

            base.OnOwnerSet();
        }

        public void Update(float dt)
        {
            Vector2 vel = m_Physics.LinearVelocity;
            vel.X = 0;
            vel.Y = 0;

            if (m_Keyboard.IsKeyDown(Keys.D) || m_Keyboard.IsKeyDown(Keys.Right))
                vel.X += m_Speed;

            if (m_Keyboard.IsKeyDown(Keys.A) || m_Keyboard.IsKeyDown(Keys.Left))
                vel.X -= m_Speed;

            if (m_Keyboard.IsKeyDown(Keys.W) || m_Keyboard.IsKeyDown(Keys.Up))
                vel.Y += m_Speed;

            if (m_Keyboard.IsKeyDown(Keys.S) || m_Keyboard.IsKeyDown(Keys.Down))
                vel.Y -= m_Speed;



            m_Physics.LinearVelocity = vel;
        }

        void OnKeyPressed(KeyPressed msg)
        {
            if (msg.Key == Keys.Space)
            {
                m_Physics.ApplyImpulse(Vector2.UnitY * m_JumpForce);
            }
        }
    }
}
