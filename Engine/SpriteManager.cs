using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renderer_namespace;

namespace Engine_namespace
{
    public static class SpriteManager
    {
        public static int billboard_width;
        public static float[] sprite_positions
        {
            set
            {
                Renderer.wall_textures_meta = new Renderer.WallTextureMetadata[value.Length];
                for (int i = 0; i < value.Length; i++)
                    Renderer.wall_textures_meta[i] = new Renderer.WallTextureMetadata
                    {
                        left = value[i],
                        width = ((i == value.Length - 1) ? 1 : value[i + 1]) - value[i]
                    };
            }
        }

        public static void LoadWallSpriteSheet(FileStream file)
        {
            Renderer.LoadWalls(file);
        }

        public static void LoadBillboardSpriteSheet(FileStream file)
        {
            Renderer.LoadBillbards(file, billboard_width);
        }
    }
}
