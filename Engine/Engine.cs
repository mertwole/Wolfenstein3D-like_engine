using Renderer_namespace;
using System;
using System.Drawing;
using OpenTK;
using System.Collections.Generic;
using System.IO;

namespace Engine_namespace
{
    public static class Engine
    {
        static Scene scene;

        public static FileStream DefaultWallTexture;
        public static FileStream DefaultBillboardTexture;

        public struct WindowCreationParams
        {
            public int Width;
            public int Height;
            public int Framerate;
            public bool Fullscreen;
        }

        public static void Init(Scene _scene, WindowCreationParams _params)
        {
            scene = _scene;

            // Setup callbacks.
            Renderer.FrameDrawCallback = OnRenderFrame;
            Renderer.PostSetupCallback = PostSetup;
            Renderer.KeyboardInputCallback = KeyboardInput;
            // Setup window parameters.
            Renderer.WindowParameters.Width = _params.Width;
            Renderer.WindowParameters.Height = _params.Height;
            Renderer.WindowParameters.Name = "";
            Renderer.WindowParameters.framerate = _params.Framerate;
            Renderer.WindowParameters.windowState = _params.Fullscreen ? WindowState.Fullscreen : WindowState.Normal;

            // Start thread in that rendering will executing.    
            Renderer.rendering_thread.Start();          
        }

        static void PostSetup()
        {
            Renderer.SetFloorAndCeilingColor(Color.FromArgb(255, 42, 42, 42) , Color.FromArgb(255, 157, 165, 128));

            scene.player.camera.Resolution_width = Renderer.WindowParameters.Width;

            scene.InitMap();

            SpriteManager.LoadWallSpriteSheet(DefaultWallTexture);
            SpriteManager.LoadBillboardSpriteSheet(DefaultBillboardTexture);
        }

        static void OnRenderFrame()
        {
            tickCallback();

            // Move player.
            for (int i = 0; i < 10; i++)
            {
                float pos_x = scene.player.Position.X;
                float pos_y = scene.player.Position.Y;
                playerMove(ref pos_x, ref pos_y, scene.player.Direction.X, scene.player.Direction.Y);
                scene.player.Position = new Vector2(pos_x, pos_y);

                Physics.ResolveCollisions(scene);
            }

            // Rotate player.
            playerRotate(ref RotationAngle);
            scene.player.Direction = new Vector2((float)Math.Cos(RotationAngle), (float)Math.Sin(RotationAngle));

            scene.DrawScene(ref Renderer.drawData, ref Renderer.drawBillboardData, ref Renderer.drawBillboardDataLength);         
        }

        static float RotationAngle = 0;

        public delegate void PlayerMoveCallback(ref float x, ref float y, float Direction_X, float Direction_Y);
        static PlayerMoveCallback playerMove;
        public static void SetPlayerMoveCallback(PlayerMoveCallback callback)
        {   playerMove = callback;  }

        public delegate void PlayerRotateCallback(ref float Rotation);
        static PlayerRotateCallback playerRotate;
        public static void SetPlayerRotateCallback(PlayerRotateCallback callback)
        {   playerRotate = callback;    }

        public delegate void TickCallback();
        static TickCallback tickCallback;
        public static void SetTickCallback(TickCallback callback)
        { tickCallback = callback; }


        // Input.
        public delegate void KeyCallback(int key);

        static KeyCallback press;
        static KeyCallback release;

        public static void SetKeyReleaseCallback(KeyCallback callback)
        {
            release = callback;
        }

        public static void SetKeyPressCallback(KeyCallback callback)
        {
            press = callback;
        }
        
        static void KeyboardInput(Renderer.KeyboardInput input)
        {
            if (input.Action == Renderer.KeyAction.Pressed)
                press(input.Keycode);
            else
                release(input.Keycode);
        }
    }
}
