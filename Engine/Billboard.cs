using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine_namespace
{
    public class Billboard
    {
        public Billboard(int texture_id, float width)
        {
            this.texture_id = texture_id;
            this.width = width;
        }

        internal int texture_id;
        internal float width;

        internal Vector2 position;
    }
}
