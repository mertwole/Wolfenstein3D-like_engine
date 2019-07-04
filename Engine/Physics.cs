using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using static Engine_namespace.Scene;

namespace Engine_namespace
{
    static class Physics
    {
        public static void ResolveCollisions(Scene scene)
        {
            // Enemies.
            for (int i = 0; i < scene.enemies.Length; i++)
            {
                if (scene.enemies[i].Position.X + 0.5f < scene.player.Position.X - (scene.player.aabb.max.X - scene.player.aabb.min.X) * 0.5f)
                    continue;

                if (scene.enemies[i].Position.X - 0.5f > scene.player.Position.X + (scene.player.aabb.max.X - scene.player.aabb.min.X) * 0.5f)
                    break;

                scene.player.Position += Collisions.AABBvsAABB(scene.player.aabb, scene.enemies[i].aabb);
            }

            // Walls.
            List<Colliders.AABB> cancollide = new List<Colliders.AABB>(0);

            int PLayerMinPosX = (int)Math.Floor(scene.player.aabb.min.X);
            int PLayerMinPosY = (int)Math.Floor(scene.player.aabb.min.Y);

            SceneBlock[] possible_blocks = new SceneBlock[4]
            {
                scene.Map[PLayerMinPosX, PLayerMinPosY],
                scene.Map[PLayerMinPosX + 1, PLayerMinPosY],
                scene.Map[PLayerMinPosX, PLayerMinPosY + 1],
                scene.Map[PLayerMinPosX + 1, PLayerMinPosY + 1]
            };

            // Check triggers.
            for(int i = 0; i < 4; i++)
            {
                if (possible_blocks[i].trigger != null && Collisions.AABBvsAABBbool(scene.player.aabb, possible_blocks[i].trigger.aabb))
                {
                    possible_blocks[i].trigger.OntriggerEnter();
                    possible_blocks[i].trigger = null;
                }
            }

            bool[] walls = new bool[4]
            {
                possible_blocks[0].wall != null,// leftbottom
                possible_blocks[1].wall != null,// rightbottom
                possible_blocks[2].wall != null,// lefttop
                possible_blocks[3].wall != null // righttop
            };

            for (int i = 0; i < 4; i++)
                if (walls[i]) cancollide.Add(possible_blocks[i].wall.aabb);

            if(cancollide.Count == 2)// Boudary case.
            {
                if((walls[2] && walls[0]) || (walls[1] && walls[3]))// Column exists.
                {
                    // Ignore Y
                    scene.player.Position += new Vector2(Collisions.AABBvsAABB(scene.player.aabb, cancollide[0]).X, 0);
                    scene.player.Position += new Vector2(Collisions.AABBvsAABB(scene.player.aabb, cancollide[1]).X, 0);

                    return;
                }

                if ((walls[0] && walls[1]) || (walls[2] && walls[3]))// Raw exists.
                {
                    // Ignore X
                    scene.player.Position += new Vector2(0, Collisions.AABBvsAABB(scene.player.aabb, cancollide[0]).Y);
                    scene.player.Position += new Vector2(0, Collisions.AABBvsAABB(scene.player.aabb, cancollide[1]).Y);

                    return;
                }
            }

            foreach (var collider in cancollide)
                scene.player.Position += Collisions.AABBvsAABB(scene.player.aabb, collider);           
        }   
    }
}