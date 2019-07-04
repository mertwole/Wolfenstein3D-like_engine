using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine_namespace
{
    public class Player
    {
        public Player(Colliders.AABB local_aabb, Camera cam, float position_x, float position_y)
        {
            localspace_aabb = local_aabb;
            aabb = localspace_aabb;

            camera = cam;

            Position = new Vector2(position_x, position_y);
        }

        internal Vector2 Position
        {
            set
            {
                pos = value;
                camera.Position = value;
                aabb = new Colliders.AABB() { min = localspace_aabb.min + value, max = localspace_aabb.max + value };
            }
            get
            {
                return pos;
            }
        }
        Vector2 pos;

        internal Vector2 Direction
        {
            set
            {
                dir = value;
                camera.Direction = value;
            }
            get
            {
                return dir;
            }
        }
        Vector2 dir;

        public Camera camera;

        Colliders.AABB localspace_aabb;
        public Colliders.AABB aabb;
    }
}
