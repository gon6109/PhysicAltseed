using System;
using System.Collections.Generic;
using Box2DNet.Collision;
using Box2DNet.Common;
using Box2DNet.Dynamics;
using System.Linq;

namespace PhysicAltseed
{
    /// <summary>
    /// 物理演算を適用させるワールド
    /// </summary>
    public class PhysicalWorld
    {
        World b2World;
        List<PhysicalShape> physicalShapes;
        CollisionController collisionController;
        public World B2World
        {
            get
            {
                return b2World;
            }
        }

        /// <summary>
        /// 1ステップあたりの時間（秒）
        /// </summary>
        public float TimeStep { get; set; }

        /// <summary>
        /// 速度更新頻度
        /// </summary>
        public int VelocityItetions { get; set; }

        /// <summary>
        /// 位置更新頻度
        /// </summary>
        public int PositionIterations { get; set; }

        /// <summary>
        /// ワールドを初期化
        /// </summary>
        /// <param name="worldRect">適用範囲</param>
        /// <param name="gravity">重力</param>
        public PhysicalWorld(asd.RectF worldRect, asd.Vector2DF gravity)
        {
            physicalShapes = new List<PhysicalShape>();
            collisionController = new CollisionController(this);
            AABB aabb = new AABB();
            aabb.LowerBound = PhysicalConvert.Tob2Vector(worldRect.Position);
            aabb.UpperBound = PhysicalConvert.Tob2Vector(worldRect.Vertexes[2]);
            b2World = new World(aabb, PhysicalConvert.Tob2Vector(gravity), true);
            b2World.SetContactListener(collisionController);
            b2World.SetContactFilter(new ContactFilter());
            TimeStep = 1.0f / 60.0f;
            VelocityItetions = 8;
            PositionIterations = 1;
        }

        ~PhysicalWorld()
        {
            b2World.Dispose();
        }

        public bool GetIsCollided(PhysicalShape shape1, PhysicalShape shape2, out List<asd.Vector2DF> points)
        {
            points = new List<asd.Vector2DF>();

            Body shape1B2Body = null, shape2B2Body = null;
            shape1B2Body = shape1.B2Body;
            shape2B2Body = shape2.B2Body;

            foreach (var item in collisionController.CollisionShapes)
            {
                if ((item.BodyA == shape1B2Body && item.BodyB == shape2B2Body) || (item.BodyB == shape1B2Body && item.BodyA == shape2B2Body))
                {
                    points = item.Points;
                    return true;
                }
            }
            return false;
        }

        public void Add(PhysicalShape shape)
        {
            physicalShapes.Add(shape);
        }

        public void Destroy(PhysicalShape shape)
        {
            if (shape.IsActive)
                B2World.DestroyBody(shape.B2Body);
            physicalShapes.Remove(shape);
        }

        /// <summary>
        /// 物理演算を1ステップ実行する
        /// </summary>
        public void Update()
        {
            b2World.Step(TimeStep, VelocityItetions, PositionIterations);
            foreach (var item in physicalShapes.OfType<asd.Shape>().Where(obj => obj.IsReleased).OfType<PhysicalShape>().ToList())
            {
                physicalShapes.Remove(item);
            }
            foreach (var item in physicalShapes.Where(obj => obj.B2Body != null))
            {
                item.SyncB2body();
            }
        }
    }

    public class CollisionData
    {
        public Body BodyA;
        public Body BodyB;
        public List<asd.Vector2DF> Points;
    }

    public class CollisionController : IContactListener
    {
        List<CollisionData> collisionShapes;
        PhysicalWorld refWorld;

        public List<CollisionData> CollisionShapes
        {
            get
            {
                return collisionShapes;
            }
        }

        public CollisionController(PhysicalWorld world)
        {
            refWorld = world;
            collisionShapes = new List<CollisionData>();
        }

        void IContactListener.BeginContact(Contact contact)
        {
            if (!contact.AreTouching) return;
            CollisionData temp = new CollisionData();
            WorldManifold worldManifold;
            contact.GetWorldManifold(out worldManifold);
            temp.BodyA = contact.FixtureA.Body;
            temp.BodyB = contact.FixtureB.Body;
            temp.Points = new List<asd.Vector2DF>();
            for (int i = 0; i < contact.Manifold.PointCount; i++)
            {
                temp.Points.Add(PhysicalConvert.ToAsdVector(worldManifold.Points[i]));
            }
            collisionShapes.Add(temp);
        }

        void IContactListener.PostSolve(Contact contact, ContactImpulse impulse)
        {
            CollisionData temp = new CollisionData();
            foreach (var item in collisionShapes)
            {
                if (item.BodyA == contact.FixtureA.Body && item.BodyB == contact.FixtureB.Body)
                {
                    temp = item;
                    break;
                }
            }
            temp.Points.Clear();
            WorldManifold worldManifold;
            contact.GetWorldManifold(out worldManifold);
            for (int i = 0; i < contact.Manifold.PointCount; i++)
            {
                temp.Points.Add(PhysicalConvert.ToAsdVector(worldManifold.Points[i]));
            }
        }

        void IContactListener.PreSolve(Contact contact, Manifold oldManifold)
        {

        }

        void IContactListener.EndContact(Contact contact)
        {
            CollisionData temp = new CollisionData();
            foreach (var item in collisionShapes)
            {
                if (item.BodyA == contact.FixtureA.Body && item.BodyB == contact.FixtureB.Body)
                {
                    temp = item;
                    break;
                }
            }
            collisionShapes.Remove(temp);
        }
    }
}
