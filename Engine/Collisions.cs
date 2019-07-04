using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine_namespace
{
    class Collisions
    {
        public static Vector2 AABBvsAABB(Colliders.AABB _dynamic, Colliders.AABB _static)// Returns vector on that _dynamic must be translated to avoid collision.
        {
            float Xoverlay, Yoverlay;

            Xoverlay = _dynamic.min.X <= _static.min.X ? -Math.Max(0, _dynamic.max.X - _static.min.X) : Math.Max(0, _static.max.X - _dynamic.min.X);
            Yoverlay = _dynamic.min.Y <= _static.min.Y ? -Math.Max(0, _dynamic.max.Y - _static.min.Y) : Math.Max(0, _static.max.Y - _dynamic.min.Y);

            if (Math.Abs(Xoverlay) > Math.Abs(Yoverlay))
                return new Vector2(0, Yoverlay);
            else
                return new Vector2(Xoverlay, 0);
        }

        public static bool AABBvsAABBbool(Colliders.AABB _dynamic, Colliders.AABB _static)
        {
            return (_dynamic.min.X <= _static.min.X ? (_dynamic.max.X > _static.min.X) : (_static.max.X > _dynamic.min.X)) ||
                    (_dynamic.min.Y <= _static.min.Y ? (_dynamic.max.Y > _static.min.Y) : (_static.max.Y > _dynamic.min.Y));
        }
    }
}
