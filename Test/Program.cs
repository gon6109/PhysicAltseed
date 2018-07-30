using System;

namespace Test
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            asd.Engine.Initialize("Test", 640, 480, new asd.EngineOption());
            PhysicAltseed.PhysicalConvert.PxPerMetreRate = 50;
            PhysicAltseed.PhysicalWorld world = new PhysicAltseed.PhysicalWorld(new asd.RectF(new asd.Vector2DF(), asd.Engine.WindowSize.To2DF()), new asd.Vector2DF(0, 250.0f));

            asd.GeometryObject2D geometryObject = new asd.GeometryObject2D();
            PhysicAltseed.PhysicalRectangleShape shape = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Dynamic, world);
            shape.DrawingArea = new asd.RectF(300, 50, 25, 25);
            shape.Friction = 0.6f;
            shape.Restitution = 0.8f;
            shape.Density = 1.0f;
            geometryObject.Shape = shape;
            asd.Engine.AddObject2D(geometryObject);

            asd.GeometryObject2D groundObject = new asd.GeometryObject2D();
            PhysicAltseed.PhysicalRectangleShape ground = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Static, world);
            ground.DrawingArea = new asd.RectF(0, 430, 640, 50);
            ground.Friction = 0.6f;
            ground.Restitution = 0.0f;
            groundObject.Shape = ground;
            asd.Engine.AddObject2D(groundObject);

            // 動的フォントを生成する。
            var font = asd.Engine.Graphics.CreateDynamicFont("", 32, new asd.Color(255, 255, 255, 255), 1, new asd.Color(255, 255, 255, 255));

            // FPSを表示するためのオブジェクトを生成する。
            var obj = new asd.TextObject2D();
            obj.Font = font;

            // オブジェクトをエンジンに追加する。
            asd.Engine.AddObject2D(obj);
            int c = 0;

            while (asd.Engine.DoEvents())
            {
                if (asd.Engine.Keyboard.GetKeyState(asd.Keys.Left) == asd.KeyState.Hold) shape.SetForce(new asd.Vector2DF(25, 0), shape.CenterPosition);//Velocity = new asd.Vector2DF(-25, shape.Velocity.Y);
                world.Update();
                asd.Engine.Update();

                if (c % 200 == 0) shape.IsActive = true;
                else if (c % 200 == 100) shape.IsActive = false;

                if (asd.Engine.Mouse.LeftButton.ButtonState == asd.MouseButtonState.Push)
                {
                    asd.GeometryObject2D geometryObject2 = new asd.GeometryObject2D();
                    PhysicAltseed.PhysicalRectangleShape shape2 = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Dynamic, world);
                    shape2.DrawingArea = new asd.RectF(asd.Engine.Mouse.Position, new asd.Vector2DF(25, 25));
                    shape2.Friction = 0.6f;
                    shape2.Restitution = 0.8f;
                    shape2.Density = 1.0f;
                    geometryObject2.Shape = shape2;
                    asd.Engine.AddObject2D(geometryObject2);
                }

                // 現在のFPSを取得する。
                float fps = asd.Engine.CurrentFPS;

                // 表示する文字列を生成する。
                var str = "FPS : " + fps;

                // 文字列をオブジェクトに設定する。
                obj.Text = str;
                c++;
            }

            asd.Engine.Terminate();
        }
    }
}
