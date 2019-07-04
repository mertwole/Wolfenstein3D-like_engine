using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine_namespace
{
    public class Colliders
    {      
        public struct AABB
        {
            public AABB(float min_x, float min_y, float max_x, float max_y)
            {
                min = new Vector2(min_x, min_y);
                max = new Vector2(max_x, max_y);
            }

            internal AABB(Vector2 _min, Vector2 _max)
            {
                min = _min;
                max = _max;
            }

            internal Vector2 min;
            internal Vector2 max;          
        }
    }
}
