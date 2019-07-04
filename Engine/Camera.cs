using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Engine_namespace
{
    public class Camera
    {
        // Resolution.
        internal int Resolution_width;

        // Actual size on screen is Real_Size * ConstantSizePlane / Distance
        // when Distance == ConstantSizePLane ActualSize == Realsize.
        public float ConstantSizePlane;
        public Vector2 Viewport;

        internal Vector2 Position;
        internal Vector2 Direction; // Normalized

        public Camera(float Viewport_x, float Viewport_y)
        {
            Viewport = new Vector2(Viewport_x, Viewport_y);
        }

        internal Raycasting.Ray GetRightRay()
        {
            return GetRay(Resolution_width - 1);
        }

        internal Raycasting.Ray GetLeftRay()
        {
            return GetRay(0);
        }

        internal Raycasting.Ray GetRay(int x)
        {
            // Point on ConstantSizePlane.
            Vector2 LookAtPoint = Position + Direction * ConstantSizePlane;

            LookAtPoint -= ((float)x / (float)Resolution_width - 0.5f) * Viewport.X * Direction.PerpendicularLeft;

            return new Raycasting.Ray() { source = Position, direction = (LookAtPoint - Position).Normalized()};
        }

        internal float GetScaleByDistance(float Distance) // Distance is length of projection on view plane.
        {
            return ConstantSizePlane / Distance;
        }
    }
}
