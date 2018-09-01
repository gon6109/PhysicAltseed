using System;

namespace Test
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            asd.Engine.Initialize("Test", 640, 480, new asd.EngineOption());
            PhysicAltseed.PhysicalConvert.PxPerMetreRate = 50;
            PhysicAltseed.PhysicalWorld world = new PhysicAltseed.PhysicalWorld(new asd.RectF(-200,-200,1040,880), new asd.Vector2DF(0, 250.0f));

            //asd.GeometryObject2D geometryObject = new asd.GeometryObject2D();
            //PhysicAltseed.PhysicalRectangleShape shape = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Dynamic, world);
            //shape.DrawingArea = new asd.RectF(300, 50, 25, 25);
            //shape.Friction = 0.6f;
            //shape.Restitution = 0.8f;
            //shape.Density = 1.0f;
            //geometryObject.Shape = shape;
            //asd.Engine.AddObject2D(geometryObject);

            asd.GeometryObject2D groundObject = new asd.GeometryObject2D();
            PhysicAltseed.PhysicalRectangleShape ground = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Static, world);
            ground.DrawingArea = new asd.RectF(0, 430, 640, 50);
            ground.Friction = 0.8f;
            ground.Restitution = 0.0f;
            groundObject.Shape = ground;
            asd.Engine.AddObject2D(groundObject);

            asd.Engine.ProfilerIsVisible = true;

            while (asd.Engine.DoEvents())
            {
                world.Update();
                asd.Engine.Update();

                if (asd.Engine.Mouse.LeftButton.ButtonState == asd.MouseButtonState.Push)
                {
                    PhysicAltseed.PhysicalTextureObject2D physicalTextureObject = new PhysicAltseed.PhysicalTextureObject2D(PhysicAltseed.PhysicalShapeType.Dynamic, world);
                    physicalTextureObject.Position = asd.Engine.Mouse.Position;
                    physicalTextureObject.SetTexture("run_g0001.png");
                    physicalTextureObject.CollisionShape.Friction = 0.8f;
                    physicalTextureObject.CollisionShape.Restitution = 0.2f;
                    physicalTextureObject.CollisionShape.Density = 1.0f;
                    asd.Engine.AddObject2D(physicalTextureObject);
                }
            }

            asd.Engine.Terminate();
        }
    }
}
