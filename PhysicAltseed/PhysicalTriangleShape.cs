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
        /// <summary>
        /// 角度
        /// </summary>
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
        /// <summary>
        /// グローバル座標
        /// </summary>
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
        /// <summary>
        /// 密度
        /// </summary>
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
        /// <summary>
        /// 反発係数
        /// </summary>
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
        /// <summary>
        /// 摩擦係数
        /// </summary>
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

        /// <summary>
        /// 速度
        /// </summary>
        public asd.Vector2DF Velocity
        {
            get
            {
                if (!IsActive) return new asd.Vector2DF();
                return PhysicalConvert.ToAsdVector(b2Body.GetLinearVelocity());
            }
            set
            {
                if (!IsActive) return;
                if (physicalShapeType == PhysicalShapeType.Kinematic) b2Body.SetPosition(PhysicalConvert.Tob2Vector(value * refWorld.TimeStep) + b2Body.GetPosition());
                else b2Body.SetLinearVelocity(PhysicalConvert.Tob2Vector(value));
            }
        }

        /// <summary>
        /// 角速度
        /// </summary>
        public float AngularVelocity
        {
            get
            {
                if (!IsActive) return 0;
                return b2Body.GetAngularVelocity() * 180.0f / 3.14f;
            }
            set
            {
                if (!IsActive) return;
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
            vertexes.Add(new asd.Vector2DF(0, -1));
            vertexes.Add(new asd.Vector2DF(1, 0));
            vertexes.Add(new asd.Vector2DF(0, 1));
            refWorld = world;
            physicalShapeType = shapeType;
            b2Body = refWorld.B2World.CreateBody(b2BodyDef);
            b2Body.CreateFixture(b2PolygonDef);
            if (physicalShapeType == PhysicalShapeType.Dynamic) b2Body.SetMassFromShapes();
            world.Add(this);
        }

        /// <summary>
        /// 削除する
        /// </summary>
        public void Destroy()
        {
            refWorld.Destroy(this);
            Dispose();
        }

        /// <summary>
        /// 三角形のローカル座標を設定する
        /// </summary>
        /// <param name="point">ローカル座標</param>
        /// <param name="index">インデックス</param>
        public new void SetPointByIndex(asd.Vector2DF point, int index)
        {
            if (index < 0 || index > 2) return;
            vertexes[index] = point;
            base.SetPointByIndex(CenterPosition + point, index);
            Reset();
        }

        /// <summary>
        /// 三角形のローカル座標を取得する
        /// </summary>
        /// <param name="index">インデックス</param>
        public new asd.Vector2DF GetPointByIndex(int index)
        {
            if (index < 0 || index > 2) return new asd.Vector2DF();
            return vertexes[index];
        }

        public void Reset()
        {
            if (b2Body != null) refWorld.B2World.DestroyBody(b2Body);

            b2BodyDef = new BodyDef();
            b2PolygonDef = new PolygonDef();

            b2BodyDef.Angle = Angle / 180.0f * 3.14f;
            b2BodyDef.Position = PhysicalConvert.Tob2Vector(CenterPosition);

            var sortedVertexes = new List<asd.Vector2DF>();
            foreach (var item in vertexes)
            {
                sortedVertexes.Add(item);
            }
            sortedVertexes.Sort((a, b) => a.Degree.CompareTo(b.Degree));
            b2PolygonDef.VertexCount = 3;
            for (int i = 0; i < 3; i++)
            {
                b2PolygonDef.Vertices[i] = PhysicalConvert.Tob2Vector(sortedVertexes[i]);
            }

            b2PolygonDef.Density = Density;
            b2PolygonDef.Restitution = Restitution;
            b2PolygonDef.Friction = Friction;

            b2Body = refWorld.B2World.CreateBody(b2BodyDef);
            b2Body.CreateFixture(b2PolygonDef);

            if (physicalShapeType == PhysicalShapeType.Dynamic) b2Body.SetMassFromShapes();
        }

        /// <summary>
        /// 力を加える
        /// </summary>
        /// <param name="vector">力を加える方向</param>
        /// <param name="position">力を加えるローカル位置</param>
        public void SetForce(asd.Vector2DF vector, asd.Vector2DF position)
        {
            if (!IsActive) return;
            b2Body.ApplyForce(PhysicalConvert.Tob2Vector(vector), PhysicalConvert.Tob2Vector(CenterPosition + position));
        }

        /// <summary>
        /// 衝撃を加える
        /// </summary>
        /// <param name="vector">衝撃を加える方向</param>
        /// <param name="position">衝撃を加えるローカル位置</param>
        public void SetImpulse(asd.Vector2DF vector, asd.Vector2DF position)
        {
            if (!IsActive) return;
            b2Body.ApplyImpulse(PhysicalConvert.Tob2Vector(vector), PhysicalConvert.Tob2Vector(CenterPosition + position));
        }

        public void SyncB2body()
        {
            if (!IsActive) return;
            centerPosition = PhysicalConvert.ToAsdVector(b2Body.GetPosition());
            angle = b2Body.GetAngle() * 180.0f / 3.14f;
            for (int i = 0; i < 3; i++)
            {
                asd.Vector2DF temp = vertexes[i];
                temp.Degree += Angle;
                base.SetPointByIndex(CenterPosition + temp, i);
            }
        }

        /// <summary>
        /// 衝突判定
        /// </summary>
        /// <param name="shape">衝突判定対象</param>
        public bool GetIsCollidedWith(PhysicalShape shape)
        {
            if (!IsActive) return false;
            List<asd.Vector2DF> points;
            return refWorld.GetIsCollided(this, shape, out points);
        }

        /// <summary>
        /// 衝突判定
        /// </summary>
        /// <param name="shape">衝突判定対象</param>
        /// <param name="points">衝突点</param>
        public bool GetIsCollidedWith(PhysicalShape shape, out List<asd.Vector2DF> points)
        {
            if (!IsActive)
            {
                points = new List<asd.Vector2DF>();
                return false;
            }
            return refWorld.GetIsCollided(this, shape, out points);
        }

        /// <summary>
        /// 物理シミュレーションをするか否か
        /// </summary>
        public bool IsActive
        {
            get
            {
                return b2Body != null;
            }
            set
            {
                if (value)
                {
                    b2Body.GetWorld().DestroyBody(b2Body);
                    b2Body = null;
                }
                else Reset();
            }
        }
    }
}
