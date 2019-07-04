using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine_namespace
{
    public class Wall
    {
        public Colliders.AABB aabb;

        public int TextureId { set { texture_id = value; } }
        int texture_id;

        internal int GetTextureIdBySide(Vector2 side)
        {
            return texture_id;
        }
    }
}
