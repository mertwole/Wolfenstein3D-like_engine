using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine_namespace
{
    static class Raycasting
    {
        const float COMP_ERR = 5 * float.Epsilon;

        internal struct Ray
        {
            public Vector2 source;
            public Vector2 direction;
        }

        // Not struct because must be nullable.
        internal class RaycastResult
        {
            public Vector2 point;
            public int texture_id;
            public float texUcoord;
        }

        internal static List<Billboard> visible_billboards;

        internal static RaycastResult CastRayInScene(Ray ray, Scene scene)
        {
            // Splitting direction onto basises.
            int x_basis = Math.Sign(ray.direction.X);
            if (x_basis == 0) x_basis = 1;
            int y_basis = Math.Sign(ray.direction.Y);
            if (y_basis == 0) y_basis = 1;

            int current_block_x = (int)Math.Floor(ray.source.X);
            int current_block_y = (int)Math.Floor(ray.source.Y);

            void CastInBillboard()
            {
                if (scene.Map[current_block_x, current_block_y].billboard != null && !visible_billboards.Contains(scene.Map[current_block_x, current_block_y].billboard))
                    visible_billboards.Add(scene.Map[current_block_x, current_block_y].billboard);
            }

            while (true)
            {
                RaycastResult collision_xbasis = RayVsAABB(ray, scene.Map[current_block_x + x_basis, current_block_y].aabb, new Vector2(-x_basis, 0));

                if (collision_xbasis != null)
                { 
                    current_block_x += x_basis;

                    CastInBillboard();

                    if (scene.Map[current_block_x, current_block_y].wall != null)
                    {
                        RaycastResult withWall = RayVsAABB(ray, scene.Map[current_block_x, current_block_y].wall.aabb, new Vector2(-x_basis, 0));

                        if (withWall != null)
                        {
                            withWall.texture_id = scene.Map[current_block_x, current_block_y].wall.GetTextureIdBySide(new Vector2(-x_basis, 0));
                            return withWall;
                        }

                        withWall = RayVsAABB(ray, scene.Map[current_block_x, current_block_y].wall.aabb, new Vector2(0, -y_basis));

                        if (withWall != null)
                        {
                            withWall.texture_id = scene.Map[current_block_x, current_block_y].wall.GetTextureIdBySide(new Vector2(0, -y_basis));
                            return withWall;
                        }
                    }

                    continue;
                }

                RaycastResult collision_ybasis = RayVsAABB(ray, scene.Map[current_block_x, current_block_y + y_basis].aabb, new Vector2(0, -y_basis));

                if (collision_ybasis != null)
                {
                    current_block_y += y_basis;

                    CastInBillboard();

                    if (scene.Map[current_block_x, current_block_y].wall != null)
                    {
                        RaycastResult withWall = RayVsAABB(ray, scene.Map[current_block_x, current_block_y].wall.aabb, new Vector2(-x_basis, 0));

                        if (withWall != null)
                        {
                            withWall.texture_id = scene.Map[current_block_x, current_block_y].wall.GetTextureIdBySide(new Vector2(-x_basis, 0));
                            return withWall;
                        }

                        withWall = RayVsAABB(ray, scene.Map[current_block_x, current_block_y].wall.aabb, new Vector2(0, -y_basis));

                        if (withWall != null)
                        {
                            withWall.texture_id = scene.Map[current_block_x, current_block_y].wall.GetTextureIdBySide(new Vector2(0, -y_basis));
                            return withWall;
                        }
                    }

                    continue;
                }

                return null; // bug
            }
        }

        // Null means no intersection.
        static RaycastResult RayVsAABB(Ray ray, Colliders.AABB aabb, Vector2 side)
        {
            // Check intersection with side facing ray source(at example left), if is, there is first intersection
            // ray evaluation is Ray.source + Ray.direction * t
            // (Ray.source + Ray.direction * t).X = aabb.min.X -- intersection evaluation
            // Ray.source.X + Ray.direction.X * t = aabb.min.X
            // t = (aabb.min.X - Ray.source.X) / Ray.direction.X.

            // Check with left border.
            if (side.X == -1)
            {
                if (ray.direction.X <= COMP_ERR && ray.direction.X >= -COMP_ERR)// Ray is parallel to Y axis.
                    return null;

                if (ray.source.X <= aabb.min.X)
                {
                    float t = (aabb.min.X - ray.source.X) / ray.direction.X;
                    Vector2 intersection_point = ray.source + ray.direction * t;
                    if ((intersection_point.Y >= aabb.min.Y) && (intersection_point.Y <= aabb.max.Y))// Valid vertical intersection.
                        return new RaycastResult()
                        {
                            point = intersection_point,
                            texUcoord = (aabb.max.Y - intersection_point.Y) / (aabb.max.Y - aabb.min.Y)
                            // Texture id later.
                        };
                }
            }
            // Check with right border.
            else if (side.X == 1)
            {
                if (ray.direction.X <= COMP_ERR && ray.direction.X >= -COMP_ERR)// Ray is parallel to Y axis.
                    return null;

                if (ray.source.X >= aabb.max.X)
                {
                    float t = (aabb.max.X - ray.source.X) / ray.direction.X;
                    Vector2 intersection_point = ray.source + ray.direction * t;
                    if ((intersection_point.Y >= aabb.min.Y) && (intersection_point.Y <= aabb.max.Y))// Valid vertical intersection.
                        return new RaycastResult()
                        {
                            point = intersection_point,
                            texUcoord = (intersection_point.Y - aabb.min.Y) / (aabb.max.Y - aabb.min.Y)
                            // Texture id later.
                        };
                }
            }
            // Check with bottom border.
            else if (side.Y == -1)
            {
                if (ray.direction.Y <= COMP_ERR && ray.direction.Y >= -COMP_ERR)// Ray is parallel to X axis.
                    return null;

                if (ray.source.Y <= aabb.min.Y)
                {
                    float t = (aabb.min.Y - ray.source.Y) / ray.direction.Y;
                    Vector2 intersection_point = ray.source + ray.direction * t;
                    if ((intersection_point.X >= aabb.min.X) && (intersection_point.X <= aabb.max.X))// Valid horizontal intersection.
                        return new RaycastResult()
                        {
                            point = intersection_point,
                            texUcoord = (intersection_point.X - aabb.min.X) / (aabb.max.X - aabb.min.X)
                            // Texture id later.
                        };
                }
            }
            // Check with top border.
            else
            {
                if (ray.direction.Y <= COMP_ERR && ray.direction.Y >= -COMP_ERR)// Ray is parallel to X axis.
                    return null;

                if (ray.source.Y >= aabb.max.Y)
                {
                    float t = (aabb.max.Y - ray.source.Y) / ray.direction.Y;
                    Vector2 intersection_point = ray.source + ray.direction * t;
                    if ((intersection_point.X >= aabb.min.X) && (intersection_point.X <= aabb.max.X))// Valid horizontal intersection.
                        return new RaycastResult()
                        {
                            point = intersection_point,
                            texUcoord = (aabb.max.X - intersection_point.X) / (aabb.max.X - aabb.min.X)
                            // Texture id later.
                        };
                }
            }

            return null;
        }
    }
}
