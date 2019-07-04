using System;
using System.Drawing;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System.IO;

namespace Renderer_namespace
{
    public static class Renderer
    {
        static Window window;

        static void OpenWindow()
        {
            window = new Window(WindowParameters.Width, WindowParameters.Height, WindowParameters.Name);
            window.WindowState = WindowParameters.windowState;
            WindowParameters.Width = window.Width;
            WindowParameters.Height = window.Height;
            window.Run(WindowParameters.framerate);           
        }

        public struct WindowParameters
        {
            public static int Width;
            public static int Height;
            public static string Name;
            public static int framerate;
            public static WindowState windowState;
        }

        public struct DrawData
        {
            public int texture_id;
            public float scale;
            public float texture_U;
        }

        public struct DrawBillboardData
        {
            public int texture_id;
            public float scale;
            public int left_visible;
            public int right_visible;
        }

        public static DrawData[] drawData;
        public static DrawBillboardData[] drawBillboardData = new DrawBillboardData[1024];
        public static int drawBillboardDataLength = 0;

        public delegate void drawFrameCallback();
        public static drawFrameCallback FrameDrawCallback;

        public delegate void postSetupCallback();
        public static postSetupCallback PostSetupCallback;

        public enum KeyAction { Pressed, Released };
        public struct KeyboardInput
        {
            public KeyAction Action;
            public int Keycode;
        }
        public delegate void keyboardInputCallback(KeyboardInput input);
        public static keyboardInputCallback KeyboardInputCallback;

        public static Thread rendering_thread = new Thread(OpenWindow);

        public static void SetFloorAndCeilingColor(Color floor, Color ceiling)
        {
            Window.Environment_Color = new Window.EnvironmentColor() { floor_color = floor, ceiling_color = ceiling };
        }

        public struct WallTextureMetadata
        {
            public float left;
            public float width;
        }

        public static WallTextureMetadata[] wall_textures_meta;

        public static void LoadWalls(FileStream file)
        {
            LoadTextures.LoadIntoTexture(file);
        }       

        public static void LoadBillbards(FileStream file, int billoard_width)
        {
            LoadTextures.LoadIntoTextureArray(file, billoard_width); 
        }
    }

    class Window : GameWindow
    { 
        public Window(int width, int height, string name) : base(width, height, GraphicsMode.Default, name)
        {
            VSync = VSyncMode.On;

            Renderer.drawData = new Renderer.DrawData[this.Width];
        }

        protected override void OnResize(EventArgs E)
        {
            base.OnResize(E);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Renderer.drawData = new Renderer.DrawData[this.Width];
        }

        internal struct EnvironmentColor
        {
            public Color ceiling_color;
            public Color floor_color;
        }

        public static EnvironmentColor Environment_Color
        {
            set
            {
                GL.Uniform3(GL.GetUniformLocation(render_shader, "top_color"), new Vector3(value.ceiling_color.R, value.ceiling_color.G, value.ceiling_color.B) / 256f);
                GL.Uniform3(GL.GetUniformLocation(render_shader, "bottom_color"), new Vector3(value.floor_color.R, value.floor_color.G, value.floor_color.B) / 256f);
            }
        }

        internal static int render_shader;
        static int VAO, VBO;
        static int draw_data_buffer;
        static int draw_billboard_data_buffer;

        protected override void OnLoad(EventArgs E)
        {
            base.OnLoad(E);
            // shader compilation
            render_shader = CompileShaders.Compile(new StreamReader("frag_shader.glsl"), new StreamReader("vert_shader.glsl"));
            GL.UseProgram(render_shader);
            // VAO&VBO initialization
            #region initialization
            float[] quad_vertices =
            {//  vert    tex
                        -1, -1,  0, 0,
                        -1,  1,  0, 1,
                         1,  1,  1, 1,
                         1, -1,  1, 0
                };

            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, quad_vertices.Length * sizeof(float), quad_vertices, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
                GL.EnableVertexAttribArray(1);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            GL.BindVertexArray(0);
            #endregion

            draw_data_buffer = GL.GenBuffer();
            draw_billboard_data_buffer = GL.GenBuffer();

            GL.Uniform2(GL.GetUniformLocation(render_shader, "screen_resolution"), new Vector2(this.Width, this.Height));

            Renderer.PostSetupCallback();
        }

        // Translate local U coordinate into global.
        void SetupDrawData()
        {
            for (int i = 0; i < Renderer.drawData.Length; i++)
            {
                int tex_id = Renderer.drawData[i].texture_id;
                Renderer.drawData[i].texture_U = Renderer.wall_textures_meta[tex_id].left + Renderer.drawData[i].texture_U * Renderer.wall_textures_meta[tex_id].width;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            base.OnRenderFrame(E);

            Renderer.FrameDrawCallback();

            SetupDrawData();

            GL.BindVertexArray(VAO);
            {
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, draw_data_buffer);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(float) * 3 * this.Width, Renderer.drawData, BufferUsageHint.StreamDraw);

                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, draw_billboard_data_buffer);
                GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(float) * 4 * Renderer.drawBillboardDataLength, Renderer.drawBillboardData, BufferUsageHint.StreamDraw);

                GL.Uniform1(GL.GetUniformLocation(render_shader, "draw_billboard_data_length"), Renderer.drawBillboardDataLength);

                GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            }
            GL.BindVertexArray(0);

            SwapBuffers();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            Renderer.KeyboardInputCallback(new Renderer.KeyboardInput() { Action = Renderer.KeyAction.Pressed, Keycode = (int)e.Key});
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            Renderer.KeyboardInputCallback(new Renderer.KeyboardInput() { Action = Renderer.KeyAction.Released, Keycode = (int)e.Key});
        }
    }
}
