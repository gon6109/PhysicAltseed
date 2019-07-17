using System;
using System.Linq;

namespace Test
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            asd.Engine.Initialize("Test", 640, 480, new asd.EngineOption());
            PhysicAltseed.PhysicalConvert.PxPerMetreRate = 50;
            PhysicAltseed.PhysicalWorld world = new PhysicAltseed.PhysicalWorld(new asd.RectF(-200,-200,1040,880), new asd.Vector2DF(0, 250.0f));

            asd.GeometryObject2D groundObject = new asd.GeometryObject2D();
            PhysicAltseed.PhysicalRectangleShape ground = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Static, world);
            ground.DrawingArea = new asd.RectF(0, 430, 640, 50);
            ground.Friction = 0.8f;
            ground.Restitution = 0.0f;
            groundObject.Shape = ground;
            ground.CategoryBits = 0x0003;
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
                    physicalTextureObject.CollisionShape.CategoryBits = 0x0001;
                    physicalTextureObject.CollisionShape.MaskBits = 0x0001;
                    physicalTextureObject.Color = new asd.Color(255, 100, 100);
                    asd.Engine.AddObject2D(physicalTextureObject);
                }

                if (asd.Engine.Mouse.RightButton.ButtonState == asd.MouseButtonState.Push)
                {
                    var remove = asd.Engine.CurrentScene.Layers.OfType<asd.Layer2D>().First().Objects.OfType<PhysicAltseed.PhysicalTextureObject2D>().First();
                    remove.CollisionShape.IsActive = false;
                    remove.CollisionShape.Dispose();
                    remove.Dispose();
                }

            }
            asd.Engine.Terminate();
        }
    }
}
