using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Renderer_namespace
{
    class LoadTextures
    {
        static int tex_array = GL.GenTexture();

        public static void LoadIntoTextureArray(FileStream atlas, int image_width)//loads atlas where textures folowing one by one in line
        {
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, image_width);

            Bitmap bitmap = (Bitmap)Bitmap.FromStream(atlas);
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            GL.DeleteTexture(tex_array);
            tex_array = GL.GenTexture();

            int image_count = bitmap.Width / image_width;          

            GL.BindTexture(TextureTarget.Texture2DArray, tex_array);

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, image_width, bitmap.Height, image_count);

            for (int i = 0; i < image_count; i++)
            {
                var data = bitmap.LockBits(new Rectangle(image_width * i, 0, image_width, bitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, image_width, bitmap.Height, 1, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bitmap.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)All.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)All.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)All.MirroredRepeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

            atlas.Close();
        }

        static int tex = GL.GenTexture();

        public static void LoadIntoTexture(FileStream atlas)
        {
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);

            Bitmap bitmap = (Bitmap)Bitmap.FromStream(atlas);
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            GL.DeleteTexture(tex);
            tex = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, tex);

            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.MirroredRepeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            atlas.Close();
        }
    }
}
