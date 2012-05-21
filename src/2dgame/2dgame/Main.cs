using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Barebones.Components;
using Meat.Resources;
using Barebones.Dependencies;
using Microsoft.Xna.Framework;
using Barebones.Xna;
using Meat.Rendering;
using Meat.Resources.Factory;
using Barebones.Framework;
using _2dgame.Components;
using Microsoft.Xna.Framework.Graphics;
using _2dgame.EngineComponents;
using Meat.Input;
using Microsoft.Xna.Framework.Audio;

namespace _2dgame
{
    class Main : EngineComponent
    {
        static readonly Vector2 DESIRED_SCREEN_SIZE = new Vector2(800, 480);
        const float IMAGE_SCALE = 0.01f;

        Random m_Rand = new Random();
        EZBake m_EZBakeOven;
        Physics m_Physics;

        public override IEnumerable<Barebones.Dependencies.IDependency> GetDependencies()
        {
            yield break;
        }

        /// <summary>
        /// Pour mieux commencer veuillez commencer par regarder la methode OnInitialise un peu plus bas pour voir
        /// comment créer ce qu'il y a l'écran.
        /// </summary>
        protected override void OnOwnerSet()
        {
            Owner.AddComponent(new ResourceLoader());

            m_EZBakeOven = new EZBake();
            Owner.AddComponent(m_EZBakeOven);

            //create renderer
            //create the projection matrix
            Matrix projection = Matrix.CreateOrthographic(IMAGE_SCALE * DESIRED_SCREEN_SIZE.X, IMAGE_SCALE * DESIRED_SCREEN_SIZE.Y, -1, 1);
            Owner.AddComponent(new RawRenderer(projection, Color.SkyBlue));

            Vector2 minWorld = -0.5f * IMAGE_SCALE * DESIRED_SCREEN_SIZE;
            minWorld.X = -1000;
            Vector2 maxWorld = 0.5f * IMAGE_SCALE * DESIRED_SCREEN_SIZE;
            maxWorld.X = 1000;
            maxWorld.Y = 1000;

            m_Physics = new Physics(Vector2.Zero, minWorld, maxWorld)
                {
                    DebugView = false
                };
            Owner.AddComponent(m_Physics);

            Owner.AddComponent(new CameraMan());

            Owner.AddComponent(new TouchReader());
            Owner.AddComponent(new KeyboardReader());

            Owner.Forum.RegisterListener<InitializeMessage>(OnInitialise);
            Owner.Forum.RegisterListener<CreatedMessage>(OnCreated);

            base.OnOwnerSet();
        }

        void OnCreated(CreatedMessage msg)
        {
            msg.Manager.PreferredBackBufferWidth = (int)DESIRED_SCREEN_SIZE.X;
            msg.Manager.PreferredBackBufferHeight = (int)DESIRED_SCREEN_SIZE.Y;

            msg.Manager.SupportedOrientations = DisplayOrientation.LandscapeLeft |
                                                DisplayOrientation.LandscapeRight;
        }

        //this is like the loading phase, it will let us use these items as handles on entities later on
        void RegisterResources()
        {
            ResourceLoader loader = Owner.GetComponent<ResourceLoader>();
            loader.AddResource(new ContentResource<SoundEffect>("laugh"));
            loader.AddResource(new ContentResource<SoundEffect>("fanfare"));
        }

        void OnInitialise(InitializeMessage msg)
        {
            RegisterResources();

            //add the camera to view the scene
            Entity camera = Owner.CreateEntity();
            camera.AddComponent(new Camera());

            camera.Transform = Matrix.CreateWorld(new Vector3(0, -1, 1), -Vector3.UnitZ, Vector3.UnitY);


            //create carré rouge
            Entity eleve = Owner.CreateEntity();
            camera.AddComponent(new FollowEntity(eleve, 0.5f * Vector3.UnitY, false, true));
            Vector2 bodySize = IMAGE_SCALE * new Vector2(50, 50);
            m_EZBakeOven.MakeSprite(eleve, bodySize, "Eleve", 4, 10);
            eleve.AddComponent(m_Physics.CreateRectangle(0.5f * bodySize, 1.0f, FarseerPhysics.Dynamics.BodyType.Dynamic));
            eleve.AddComponent(new Eleve(2, .1f));


            Vector3 background_translation = -0.5f * Vector3.UnitY;
            CreateBackground(camera, background_translation);

            //add grass in front of everything
            Entity grass = Owner.CreateEntity();
            grass.Transform = Matrix.CreateTranslation(background_translation);
            grass.AddComponent(new FollowEntity(camera, Vector3.Zero, false, true));
            m_EZBakeOven.MakeParallaxSprite(grass, IMAGE_SCALE * new Vector2(800, 600), "grass", 1.0f);

            ResourceLoader loader = Owner.GetComponent<ResourceLoader>();
            loader.ForceLoadAll(); // so as to not have glitches in the first couple seconds while all the items are loaded as they are accessed

            // enter: The Queen!
            loader.GetResource("fanfare").Get<SoundEffect>().Play();
        }

        private void CreateBackground(Entity camera, Vector3 translation)
        {
            Vector2 backsize = IMAGE_SCALE * new Vector2(800, 600);
            Vector3 delta_follow = Vector3.Zero;
            Matrix background_translation = Matrix.CreateTranslation(translation);

            Entity clouds = Owner.CreateEntity();
            clouds.Transform = background_translation;
            clouds.AddComponent(new FollowEntity(camera, delta_follow, false, true));
            m_EZBakeOven.MakeParallaxSprite(clouds, backsize, "clouds", 0.3f);

            Entity sun = Owner.CreateEntity();
            sun.Transform = background_translation;
            sun.AddComponent(new FollowEntity(camera, delta_follow, false, true));

            Entity sunjoint = sun.CreateChild();
            sunjoint.Transform = Matrix.CreateTranslation(new Vector3(-2, 1, 0));

            Vector2 sunsize = IMAGE_SCALE * new Vector2(300, 289);
            Entity sunray2 = sunjoint.CreateChild();
            sunray2.AddComponent(new RotatingComponent(-6));
            m_EZBakeOven.MakeSprite(sunray2, sunsize, "sunray2");

            Entity sunray1 = sunjoint.CreateChild();
            sunray1.AddComponent(new RotatingComponent(10));
            m_EZBakeOven.MakeSprite(sunray1, sunsize, "sunray1");

            Entity sunbody = sunjoint.CreateChild();
            sunbody.AddComponent(new RotatingComponent(180));
            m_EZBakeOven.MakeSprite(sunbody, sunsize, "sunbody");

            Entity sunface = sunjoint.CreateChild();
            m_EZBakeOven.MakeSprite(sunface, sunsize, "sunface");

            Entity backmountains = Owner.CreateEntity();
            backmountains.Transform = background_translation;
            backmountains.AddComponent(new FollowEntity(camera, delta_follow, false, true));
            m_EZBakeOven.MakeParallaxSprite(backmountains, backsize, "bumps_back", 0.5f);

            Entity trees = Owner.CreateEntity();
            trees.Transform = background_translation;
            trees.AddComponent(new FollowEntity(camera, delta_follow, false, true));
            m_EZBakeOven.MakeParallaxSprite(trees, backsize, "trees", 0.75f);

            Entity frontmountains = Owner.CreateEntity();
            frontmountains.Transform = background_translation;
            frontmountains.AddComponent(new FollowEntity(camera, delta_follow, false, true));
            m_EZBakeOven.MakeParallaxSprite(frontmountains, backsize, "bumps_front", 0.9f);
        }

        private void CreateBeefeater(Vector3 pos)
        {
            float beefeater_distance = 1.05f;

            Entity beefeater = Owner.CreateEntity();
            beefeater.Transform = Matrix.CreateScale(0.25f) * Matrix.CreateTranslation(pos);

            Entity beef_legs_joint = beefeater.CreateChild();
            beef_legs_joint.Transform = Matrix.CreateTranslation(-beefeater_distance * Vector3.UnitY);

            Entity beef_legs = beef_legs_joint.CreateChild();
            beef_legs.AddComponent(new LeftRightComponent(-10, 10, 1, -beefeater_distance * Vector3.UnitY));
            m_EZBakeOven.MakeSprite(beef_legs, IMAGE_SCALE * new Vector2(105, 219), "beefeater_legs");

            Entity beef_body_joint = beefeater.CreateChild();
            beef_body_joint.Transform = Matrix.CreateTranslation(beefeater_distance * Vector3.UnitY);

            Entity beef_body = beef_body_joint.CreateChild();
            beef_body.AddComponent(new LeftRightComponent(-10, 10, -1, beefeater_distance * Vector3.UnitY));
            m_EZBakeOven.MakeSprite(beef_body, IMAGE_SCALE * new Vector2(180, 351), "beefeater_body");
        }
    }
}
