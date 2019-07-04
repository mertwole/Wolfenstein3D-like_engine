using System;
using System.Collections.Generic;
using System.IO;
using Engine_namespace;

namespace test
{
    class Program
    {
        static Scene scene;

        [STAThread]
        static void Main()
        {
            Engine.SetKeyPressCallback(OnKeyPress);
            Engine.SetKeyReleaseCallback(OnKeyRelease);
            Engine.SetPlayerMoveCallback(OnPlayerMove);
            Engine.SetPlayerRotateCallback(OnPlayerRotate);
            Engine.SetTickCallback(OnTick);

            scene = new Scene();

            scene.Map = new Scene.SceneBlock[18, 18];

            for (int x = 0; x < scene.Map.GetLength(0); x++)
                for (int y = 0; y < scene.Map.GetLength(1); y++)
                {
                    scene.Map[x, y] = new Scene.SceneBlock();
                    scene.Map[x, y].wall = new Wall() { TextureId = 0, aabb = new Colliders.AABB(x, y, x + 1, y + 1)};
                }

            for (int x = 1; x < scene.Map.GetLength(0) - 2; x++)
                for (int y = 1; y < scene.Map.GetLength(1) - 2; y++)
                {
                    scene.Map[x, y].wall = null;
                }

            scene.Map[6, 6].wall = new Wall() { TextureId = 1, aabb = new Colliders.AABB(6.2f, 6.2f, 6.8f, 6.8f) };
            scene.Map[6, 7].wall = new Wall() { TextureId = 0, aabb = new Colliders.AABB(6, 7, 6 + 1, 7 + 1) };
            scene.Map[7, 6].wall = new Wall() { TextureId = 0, aabb = new Colliders.AABB(7, 6, 7 + 1, 6 + 1) };
            scene.Map[7, 7].wall = new Wall() { TextureId = 0, aabb = new Colliders.AABB(7.2f, 7.2f, 7.8f, 7.8f) };

            var billboard = new Billboard(1, 1);
            scene.enemies = new Enemy[] { new Enemy(10, 10, new Colliders.AABB(-0.3f, -0.3f, 0.3f, 0.3f), billboard) };

            Camera camera = new Camera(16f / 9f, 1);
            camera.ConstantSizePlane = 1;

            scene.player = new Player(new Colliders.AABB(-0.3f, -0.3f, 0.3f, 0.3f), camera, 4, 4);

            Engine.WindowCreationParams _params = new Engine.WindowCreationParams()
            {
                Framerate = 30,
                Fullscreen = false,
                Width = 1600, // Any >0 when fullscreen.
                Height = 900 // Any >0 when fullscreen.
            };

            // Sprites.
            Engine.DefaultWallTexture = new FileStream(@"textures\static.png", FileMode.Open);
            Engine.DefaultBillboardTexture = new FileStream(@"textures\dynamic.png", FileMode.Open);
            SpriteManager.billboard_width = 64;
            SpriteManager.sprite_positions = new float[] { 0, 64f / 102f };

            Engine.Init(scene, _params);
        }

        static int a = 0;

        static void OnTick()
        {
            if (scene.CheckShoot() != null)
                Console.WriteLine((a++).ToString() + " shoot!");

            if (scene.enemies[0].CanHitPlayer(scene))
                Console.WriteLine((a++).ToString() + " killed!");
        }

        static float MoveDirection = 0;
        const float MoveSpeed = 0.03f;

        static float RotationDirection = 0;
        const float RotationStep = 3 * (float)Math.PI / 180;

        static bool switch_textures;

        static void OnKeyPress(int key)
        {
            if (key == 45) // UpArrow.
                MoveDirection = 1;
            else if (key == 46) // DownArrow.
                MoveDirection = -1;

            if (key == 47) // LeftArrow.
                RotationDirection = 1;
            else if (key == 48) // RightArrow.
                RotationDirection = -1;

            if (key == 1)
            {
                switch_textures = !switch_textures;

                if (switch_textures)
                    SpriteManager.LoadWallSpriteSheet(new FileStream(@"textures\static1.png", FileMode.Open));
                else
                    SpriteManager.LoadWallSpriteSheet(new FileStream(@"textures\static.png", FileMode.Open));
            }
        }

        static void OnKeyRelease(int key)
        {
            if (key == 45 && MoveDirection == 1)
                MoveDirection = 0;

            if (key == 46 && MoveDirection == -1)
                MoveDirection = 0;

            if (key == 47 && RotationDirection == 1) // LeftArrow.
                RotationDirection = 0;
            else if (key == 48 && RotationDirection == -1) // RightArrow.
                RotationDirection = 0;
        }

        static void OnPlayerMove(ref float x, ref float y, float Direction_X, float Direction_Y)
        {
            x += Direction_X * MoveDirection * MoveSpeed;
            y += Direction_Y * MoveDirection * MoveSpeed;
        }

        static void OnPlayerRotate(ref float angle)
        {
            angle += RotationStep * RotationDirection;
        }
    }
}