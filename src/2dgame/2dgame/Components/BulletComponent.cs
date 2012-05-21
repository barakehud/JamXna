using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Barebones.Components;
using Barebones.Framework;

namespace _2dgame.Components
{
    class BulletComponent : EntityComponent
    {
        public override IEnumerable<Barebones.Dependencies.IDependency> GetDependencies()
        {
            yield break;
        }

        protected override void OnOwnerSet()
        {
            Owner.Forum.RegisterListener<CollisionMsg>(OnCollision);

            base.OnOwnerSet();
        }

        void OnCollision(CollisionMsg msg)
        {
            Entity target = msg.First == Owner ? msg.Second : msg.First;

            Owner.Dispose();
        }
    }
}
