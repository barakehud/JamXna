using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Barebones.Components;
using Meat.Input;
using Barebones.Dependencies;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Barebones.Framework;
using _2dgame.EngineComponents;
using Meat.Resources.Factory;

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

            if (m_Keyboard.IsKeyDown(Keys.D))
                vel.X += m_Speed;

            if (m_Keyboard.IsKeyDown(Keys.A))
                vel.X -= m_Speed;

            if (m_Keyboard.IsKeyDown(Keys.W))
                vel.Y += m_Speed;

            if (m_Keyboard.IsKeyDown(Keys.S))
                vel.Y -= m_Speed;

            m_Physics.LinearVelocity = vel;
        }

        private void ShootBullet(Vector2 direction)
        {
            Entity bullet = Owner.Engine.CreateEntity();
            bullet.Transform = Matrix.CreateTranslation(Owner.GetWorldTranslation() + new Vector3(direction, 0));
            bullet.AddComponent(Owner.Engine.GetComponent<Physics>().CreateCircle(0.1f, 1.0f, FarseerPhysics.Dynamics.BodyType.Dynamic));
            bullet.AddComponent(new BulletComponent());
            bullet.GetComponent<PhysicsComponent>().ApplyImpulse(direction * 0.3f); //Tweak vitesse de la balle
            Owner.Engine.GetComponent<EZBake>().MakeSprite(bullet, 0.01f * new Vector2(16, 16), "BULLET");
        }

        void OnKeyPressed(KeyPressed msg)
        {
            if (msg.Key == Keys.Space)
            {
                m_Physics.ApplyImpulse(Vector2.UnitY * m_JumpForce);
            }

            if (msg.Key == Keys.Right)
                ShootBullet(Vector2.UnitX);

            if (msg.Key == Keys.Left)
                ShootBullet(-Vector2.UnitX);

            if (msg.Key == Keys.Up)
                ShootBullet(Vector2.UnitY);

            if (msg.Key == Keys.Down)
                ShootBullet(-Vector2.UnitY);
        }

        void OnCollision(CollisionMsg msg) {
            //logique de collision
        }

    }
}
