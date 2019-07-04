using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine_namespace
{
    public class Enemy
    {
        public Enemy(float pos_x, float pos_y, Colliders.AABB aabb, Billboard billboard)
        {
            local_aabb = aabb;
            this.billboard = billboard;
            Position = new Vector2(pos_x, pos_y);
        }

        public void Move(float new_pos_x, float new_pos_y)
        {
            Position = new Vector2(new_pos_x, new_pos_y);
        }

        public Billboard billboard;
        internal Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                aabb = new Colliders.AABB(local_aabb.min + position, local_aabb.max + position);
                billboard.position = value;
            }
        }
        Vector2 position;
        Colliders.AABB local_aabb;
        internal Colliders.AABB aabb { get; private set; }

        public bool CanHitPlayer(Scene scene)
        {
            Raycasting.RaycastResult result = Raycasting.CastRayInScene(
                new Raycasting.Ray()
                {
                    source = position,
                    direction = (scene.player.Position - position).Normalized()
                }
                , scene);

            if (result == null) // Bug.
                return false;

            if (Vector2.Dot(result.point - position, scene.player.Position - result.point) >= 0) // Collision point lies between enemy and player.
                return false;

            return true;    
        }
    }
}
