using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Barebones.Components;
using Microsoft.Xna.Framework;
using _2dgame.Components;
using FarseerPhysics.Factories;
using Barebones.Dependencies;
using FarseerPhysics;
using FarseerPhysics.DebugViews;
using Meat.Rendering;
using Barebones.Xna;
using FarseerPhysics.Dynamics.Joints;
using Barebones.Framework;
using FarseerPhysics.Common;

namespace _2dgame.EngineComponents
{
    class Physics : EngineComponent, Barebones.Framework.IUpdateable, Barebones.Framework.IDrawable
    {
        World m_World;
        DebugViewXNA m_View;

        RawRenderer m_Renderer;
        Camera m_Camera;

        Vector2 m_min;
        Vector2 m_max;

        public bool DebugView
        {
            get { return m_View.Enabled; }
            set { m_View.Enabled = value; }
        }

        public override IEnumerable<Barebones.Dependencies.IDependency> GetDependencies()
        {
            yield return new Dependency<PhysicsComponent>(item => { }, item => item.DisposePhysics(m_World));
            yield return new Dependency<RawRenderer>(item => m_Renderer = item, item => m_Renderer = null);
            yield return new Dependency<Camera>(item => m_Camera = item, item => m_Camera = null);
        }

        public Physics(Vector2 gravity, Vector2 min, Vector2 max)
        {
            m_World = new World(gravity, new FarseerPhysics.Collision.AABB(ref min, ref max));
            m_View = new DebugViewXNA(m_World);
            m_View.Flags |= DebugViewFlags.PerformanceGraph;
            m_View.AppendFlags(DebugViewFlags.Shape);
            m_View.AppendFlags(DebugViewFlags.DebugPanel);
            m_View.AppendFlags(DebugViewFlags.Joint);
            m_View.AppendFlags(DebugViewFlags.ContactPoints);
            m_View.AppendFlags(DebugViewFlags.ContactNormals);
            m_View.AppendFlags(DebugViewFlags.Controllers);
            m_View.AppendFlags(DebugViewFlags.CenterOfMass);
            m_View.AppendFlags(DebugViewFlags.AABB);

            m_View.DefaultShapeColor = Color.Green;
            m_View.SleepingShapeColor = Color.LightGray;

            m_min = min;
            m_max = max;
        }

        public Physics(Vector2 gravity)
        {
            m_World = new World(gravity);
        }

        protected override void OnOwnerSet()
        {
            Owner.Forum.RegisterListener<LoadContentMessage>(item =>
                {
                    m_View.LoadContent(item.Device, item.Content);
                });

            Entity borderent = Owner.CreateEntity();

            //floor
            Body body = BodyFactory.CreateEdge(m_World, new Vector2(m_min.X, m_min.Y), new Vector2(m_max.X, m_min.Y));
            borderent.CreateChild().AddComponent(new PhysicsComponent(body));

            //right side
            body = BodyFactory.CreateEdge(m_World, new Vector2(m_max.X, m_min.Y), new Vector2(m_max.X, m_max.Y));
            borderent.CreateChild().AddComponent(new PhysicsComponent(body));

            //left side
            body = BodyFactory.CreateEdge(m_World, new Vector2(m_min.X, m_min.Y), new Vector2(m_min.X, m_max.Y));
            borderent.CreateChild().AddComponent(new PhysicsComponent(body));

            //top
            body = BodyFactory.CreateEdge(m_World, new Vector2(m_min.X, m_max.Y), new Vector2(m_max.X, m_max.Y));
            borderent.CreateChild().AddComponent(new PhysicsComponent(body));

            base.OnOwnerSet();
        }

        public PhysicsComponent CreateRectangle(Vector2 size, float density, BodyType bodytype)
        {
            Body body = BodyFactory.CreateRectangle(m_World, size.X, size.Y, density);
            body.BodyType = bodytype;
            return new PhysicsComponent(body);
        }

        public PhysicsComponent CreateCircle(float radius, float density, BodyType bodytype)
        {
            Body body = BodyFactory.CreateCircle(m_World, radius, density);
            body.BodyType = bodytype;
            return new PhysicsComponent(body);
        }

        public PhysicsComponent CreateCapsule(float height, float radius, float density, BodyType bodytype)
        {
            Body body = BodyFactory.CreateCapsule(m_World, height, radius, density);
            body.BodyType = bodytype;
            return new PhysicsComponent(body);
        }

        public PhysicsComponent CreateTriangle(float width, float height, float density, BodyType bodytype)
        {
            Vertices verts = new Vertices(new Vector2[]
            {
                new Vector2(-0.5f * width, 0.5f * height),
                new Vector2(0, -0.5f * height),
                new Vector2(0.5f * width, 0.5f * height)
            });

            Body body = BodyFactory.CreatePolygon(m_World, verts, density);
            body.BodyType = bodytype;
            return new PhysicsComponent(body);
        }

        public PhysicsComponent CreateRoundedRectangle(float width, float height, float xradius, float yradius, int segments, float density, BodyType bodytype)
        {
            Body body = BodyFactory.CreateRoundedRectangle(m_World, width, height, xradius, yradius, segments, density);
            body.BodyType = bodytype;
            return new PhysicsComponent(body);
        }

        public void ConstrainAngle(float angle, float maximpulse, float softness, Entity entity)
        {
            PhysicsComponent physics = entity.GetComponent<PhysicsComponent>();
            if (physics == null)
                throw new InvalidOperationException("Joint needs a physics component to constrain");

            FixedAngleJoint joint = JointFactory.CreateFixedAngleJoint(m_World, physics.GetBody());
            joint.TargetAngle = angle;
            joint.MaxImpulse = maximpulse;
            joint.Softness = softness;

            entity.AddComponent(new JointComponent(joint));
        }

        public void Update(float dt)
        {
            m_World.Step(dt);
        }

        public void Draw()
        {
            Matrix proj = m_Renderer.Projection;
            Matrix view = m_Camera.GetView();

            m_View.RenderDebugData(ref proj, ref view);
        }
    }
}
