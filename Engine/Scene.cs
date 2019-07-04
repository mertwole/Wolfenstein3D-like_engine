using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Renderer_namespace.Renderer;
using static Engine_namespace.Colliders;

namespace Engine_namespace
{
    public class Scene
    {
        public Player player;
        public Enemy[] enemies;

        public class SceneBlock
        {
            internal AABB aabb;

            public Billboard billboard;
            public Wall wall;
            public Trigger trigger;
        }

        public SceneBlock[,] Map;

        internal void InitMap()// Initializing colliders. Must be called after setuping Map size.
        {
            for (int x = 0; x < Map.GetLength(0); x++)
                for (int y = 0; y < Map.GetLength(1); y++)
                {
                    Map[x, y].aabb = new AABB()
                    {
                        min = new Vector2(x, y),
                        max = new Vector2((x + 1), (y + 1))
                    };

                    if (Map[x, y].billboard != null)
                        Map[x, y].billboard.position = new Vector2(x + 0.5f, y + 0.5f);
                }
        }

        public Enemy CheckShoot()
        {
            Raycasting.RaycastResult wall_result = Raycasting.CastRayInScene(new Raycasting.Ray() { source = player.Position, direction = player.Direction }, this);

            if (wall_result == null)// Bug.
                return null;

            float min_dist = (wall_result.point - player.Position).Length;

            Enemy hittedenemy = null;

            foreach (var enemy in enemies)
            {
                float enemytoplayerlength = (enemy.Position - player.Position).Length;
                float enemy_distance = enemytoplayerlength * Vector2.Dot((enemy.Position - player.Position) / enemytoplayerlength, player.Direction);

                if (enemy_distance < 0)
                    continue;

                Vector2 HitPoint = player.Position + player.Direction * enemy_distance;

                if ((HitPoint - enemy.Position).LengthSquared >= enemy.billboard.width * enemy.billboard.width * 0.25f)// Out of bounds.
                    continue;

                if(enemy_distance < min_dist)
                {
                    min_dist = enemy_distance;
                    hittedenemy = enemy;
                }
            }

            return hittedenemy;
        }

        internal void DrawScene(ref DrawData[] drawData, ref DrawBillboardData[] drawBillboardData, ref int drawBillboardDataLength)
        {
            Raycasting.visible_billboards = new List<Billboard>();

            DrawWalls(ref drawData);

            DrawEnemies();

            DrawBillboards(ref drawBillboardData, ref drawBillboardDataLength);
        }

        void DrawEnemies()
        {
            foreach (var enemy in enemies)
                Raycasting.visible_billboards.Add(enemy.billboard);
        }

        void DrawWalls(ref DrawData[] drawData)
        {
            for (int i = 0; i < player.camera.Resolution_width; i++)
            {
                Raycasting.Ray curr_ray = player.camera.GetRay(i);
                Raycasting.RaycastResult curr_collision = Raycasting.CastRayInScene(curr_ray, this);

                if(curr_collision == null)// Bug.
                {
                    if (i != 0)
                        drawData[i] = drawData[i - 1];

                    continue;
                }

                drawData[i].texture_id = curr_collision.texture_id;
                drawData[i].texture_U = curr_collision.texUcoord;
                Vector2 contact_source_line = curr_collision.point - curr_ray.source;
                drawData[i].scale = player.camera.GetScaleByDistance(contact_source_line.Length * Vector2.Dot(player.camera.Direction, curr_ray.direction));
            }
        }

        void DrawBillboards(ref DrawBillboardData[] drawBillboardData, ref int drawBillboardDataLength)
        {
            drawBillboardDataLength = Raycasting.visible_billboards.Count();

            if (drawBillboardData.Length < drawBillboardDataLength)
                Array.Resize(ref drawBillboardData, Math.Max(drawBillboardData.Length * 2, drawBillboardDataLength));

            for (int i = 0; i < drawBillboardDataLength; i++)
            {
                float distance = (Raycasting.visible_billboards[i].position - player.camera.Position).Length
                    * Vector2.Dot((Raycasting.visible_billboards[i].position - player.camera.Position).Normalized(), player.camera.Direction);
                float scale = player.camera.GetScaleByDistance(distance);

                float half_width = Raycasting.visible_billboards[i].width * 0.5f;

                Vector2 left_point = Raycasting.visible_billboards[i].position + player.camera.Direction.PerpendicularLeft * half_width;
                Vector2 right_point = Raycasting.visible_billboards[i].position + player.camera.Direction.PerpendicularRight * half_width;

                // Cosine of half angle between right and left player.camera rays(for following function).
                float acos = Vector2.Dot(player.camera.Direction, player.camera.GetLeftRay().direction);
                float leftToRightCameraRaysLen = (player.camera.GetRightRay().direction - player.camera.GetLeftRay().direction).Length;

                int GetScreenPosByRay(Vector2 direction)
                {
                    direction.Normalize();
                    float bcos = Vector2.Dot(direction, player.camera.Direction);

                    float CollinearPoint_vectorLength = acos / bcos;
                    Vector2 CollinearPoint_vector = CollinearPoint_vectorLength * direction;

                    float Screenpos;

                    if (Vector2.Dot(CollinearPoint_vector - player.camera.GetLeftRay().direction, player.camera.GetRightRay().direction - CollinearPoint_vector) >= 0)
                        Screenpos = (CollinearPoint_vector - player.camera.GetLeftRay().direction).Length;
                    else if (Vector2.Dot(CollinearPoint_vector - player.camera.GetRightRay().direction, player.camera.GetRightRay().direction - player.camera.GetLeftRay().direction) >= 0)// Right.
                        Screenpos = (CollinearPoint_vector - player.camera.GetLeftRay().direction).Length;
                    else
                        Screenpos = -(CollinearPoint_vector - player.camera.GetLeftRay().direction).Length;

                    return (int)(Screenpos / leftToRightCameraRaysLen * (float)player.camera.Resolution_width);
                }

                drawBillboardData[i] = new DrawBillboardData()
                {
                    texture_id = Raycasting.visible_billboards[i].texture_id,
                    scale = scale,
                    left_visible = GetScreenPosByRay(left_point - player.camera.Position),
                    right_visible = GetScreenPosByRay(right_point - player.camera.Position)
                };
            }

            drawBillboardData.Take(drawBillboardDataLength).OrderBy(x => x.scale).ToArray().CopyTo(drawBillboardData, 0);
        }
    }
}
