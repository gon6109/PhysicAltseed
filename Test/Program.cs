using System;

namespace Test
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            asd.Engine.Initialize("Test", 640, 480, new asd.EngineOption());
            PhysicAltseed.PhysicalWorld world = new PhysicAltseed.PhysicalWorld(new asd.RectF(new asd.Vector2DF(), asd.Engine.WindowSize.To2DF()), new asd.Vector2DF(0, 98));

            asd.GeometryObject2D geometryObject = new asd.GeometryObject2D();
            PhysicAltseed.PhysicalRectangleShape shape = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Dynamic, world);
            shape.DrawingArea = new asd.RectF(300, 50, 40, 40);
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

            while (asd.Engine.DoEvents())
            {
                world.Update();
                asd.Engine.Update();
            }

            asd.Engine.Terminate();
        }
    }
}
