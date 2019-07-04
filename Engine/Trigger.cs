using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine_namespace
{
    public class Trigger
    {
        public Trigger(Colliders.AABB aabb, onTriggerEnter OnTriggerEnter)
        {
            this.aabb = aabb;
            this.OntriggerEnter = OnTriggerEnter;
        }

        internal Colliders.AABB aabb;

        public delegate void onTriggerEnter();
        internal onTriggerEnter OntriggerEnter;
    }
}
