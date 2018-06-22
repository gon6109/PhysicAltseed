using System;
using System.Collections.Generic;
using Box2DNet.Collision;
using Box2DNet.Common;
using Box2DNet.Dynamics;
namespace PhysicAltseed
{
    /// <summary>
    /// 物理対応三角形
    /// </summary>
    public class PhysicalTriangleShape : asd.TriangleShape, PhysicalShape
    {
        BodyDef b2BodyDef;
        PolygonDef b2PolygonDef;
        Body b2Body;
        PhysicalShapeType physicalShapeType;
        PhysicalWorld refWorld;
        private List<asd.Vector2DF> vertexes;

        public Body B2Body
        {
            get
            {
                return b2Body;
            }
        }

        private float angle;
        public float Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
                Reset();
            }
        }

        private asd.Vector2DF centerPosition;
        public asd.Vector2DF CenterPosition
        {
            get
            {
                return centerPosition;
            }
            set
            {
                centerPosition = value;
                Reset();
            }
        }

        float density;
        public float Density
        {
            get
            {
                return density;
            }
            set
            {
                density = value;
                Reset();
            }
        }

        float restitution;
        public float Restitution
        {
            get
            {
                return restitution;
            }
            set
            {
                restitution = value;
                Reset();
            }
        }

        float friction;
        public float Friction
        {
            get
            {
                return friction;
            }
            set
            {
                friction = value;
                Reset();
            }
        }

        public asd.Vector2DF Velocity
        {
            get
            {
                return PhysicalConvert.ToAsdVector(b2Body.GetLinearVelocity());
            }
            set
            {
                if (physicalShapeType == PhysicalShapeType.Kinematic) b2Body.SetPosition(PhysicalConvert.Tob2Vector(value * refWorld.TimeStep) + b2Body.GetPosition());
                else b2Body.SetLinearVelocity(PhysicalConvert.Tob2Vector(value));
            }
        }

        public float AngularVelocity
        {
            get
            {
                return b2Body.GetAngularVelocity() * 180.0f / 3.14f;
            }
            set
            {
                if (physicalShapeType == PhysicalShapeType.Kinematic) b2Body.SetAngle(value / 180.0f * 3.14f + b2Body.GetAngle());
                else b2Body.SetAngularVelocity(value / 180.0f * 3.14f);
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="shapeType">物理形状タイプ</param>
        /// <param name="world">登録するワールド</param>
        public PhysicalTriangleShape(PhysicalShapeType shapeType, PhysicalWorld world)
        {
            density = 1.0f;
            restitution = 0.3f;
            angle = 0.0f;
            b2BodyDef = new BodyDef();
            b2PolygonDef = new PolygonDef();
            vertexes = new List<asd.Vector2DF>();
            b2PolygonDef.VertexCount = 3;
            vertexes.Add(new asd.Vector2DF(0, 0));
            vertexes.Add(new asd.Vector2DF(1, 0));
            vertexes.Add(new asd.Vector2DF(0, 1));
            refWorld = world;
            physicalShapeType = shapeType;
            b2Body = refWorld.B2World.CreateBody(b2BodyDef);
            b2Body.CreateFixture(b2PolygonDef);
            if (physicalShapeType == PhysicalShapeType.Dynamic) b2Body.SetMassFromShapes();
            world.Add(this);
        }

        public new void SetPointByIndex(asd.Vector2DF point, int index)
        {
            if (index < 0 || index > 2) return;
            vertexes[index] = point;
            base.SetPointByIndex(CenterPosition + point, index);
            Reset();
        }

        public new asd.Vector2DF GetPointByIndex(int index)
        {
            if (index < 0 || index > 2) return new asd.Vector2DF();
            return vertexes[index];
        }

        public void Reset()
        {
            refWorld.B2World.DestroyBody(b2Body);
            b2BodyDef = new BodyDef();
            b2PolygonDef = new PolygonDef();
            b2BodyDef.Angle = Angle / 180.0f * 3.14f;
            b2BodyDef.Position = PhysicalConvert.Tob2Vector(CenterPosition);
            vertexes.Sort((a, b) => a.Degree.CompareTo(b.Degree));
            b2PolygonDef.VertexCount = 3;
            for (int i = 0; i < 3; i++)
            {
                b2PolygonDef.Vertices[i] = PhysicalConvert.Tob2Vector(vertexes[i]);
            }
            b2PolygonDef.Density = Density;
            b2PolygonDef.Restitution = Restitution;
            b2PolygonDef.Friction = Friction;
            b2Body = refWorld.B2World.CreateBody(b2BodyDef);
            b2Body.CreateFixture(b2PolygonDef);
            if (physicalShapeType == PhysicalShapeType.Dynamic) b2Body.SetMassFromShapes();
        }

        public void SetForce(asd.Vector2DF vector, asd.Vector2DF position)
        {
            b2Body.ApplyForce(PhysicalConvert.Tob2Vector(vector, false), PhysicalConvert.Tob2Vector(CenterPosition + position));
        }

        public void SetImpulse(asd.Vector2DF vector, asd.Vector2DF position)
        {
            b2Body.ApplyImpulse(PhysicalConvert.Tob2Vector(vector, false), PhysicalConvert.Tob2Vector(CenterPosition + position));
        }

        public void SyncB2body()
        {
            centerPosition = PhysicalConvert.ToAsdVector(b2Body.GetPosition());
            angle = b2Body.GetAngle() * 180.0f / 3.14f;
            for (int i = 0; i < 3; i++)
            {
                asd.Vector2DF temp = vertexes[i];
                temp.Degree += Angle;
                base.SetPointByIndex(CenterPosition + temp, i);
            }
        }

        public bool GetIsCollidedWith(PhysicalShape shape)
        {
            List<asd.Vector2DF> points;
            return refWorld.GetIsCollided(this, shape, out points);
        }

        public bool GetIsCollidedWith(PhysicalShape shape, out List<asd.Vector2DF> points)
        {
            return refWorld.GetIsCollided(this, shape, out points);
        }
    }
}
