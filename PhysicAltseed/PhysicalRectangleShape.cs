﻿using System;
using System.Collections.Generic;
using Box2DNet.Collision;
using Box2DNet.Common;
using Box2DNet.Dynamics;
namespace PhysicAltseed
{
    /// <summary>
    /// 物理対応四角形
    /// </summary>
    public class PhysicalRectangleShape : asd.RectangleShape, PhysicalShape
    {
        BodyDef b2BodyDef;
        PolygonDef b2PolygonDef;
        Body b2Body;
        PhysicalShapeType physicalShapeType;
        PhysicalWorld refWorld;

        public Body B2Body
        {
            get
            {
                return b2Body;
            }
        }

        /// <summary>
        /// 描画範囲
        /// </summary>
        public new asd.RectF DrawingArea
        {
            get
            {
                return base.DrawingArea;
            }
            set
            {
                base.DrawingArea = value;
                Reset();
            }
        }

        /// <summary>
        /// DrawingArea.Sizeの半分のベクトル
        /// </summary>
        public new asd.Vector2DF CenterPosition
        {
            get
            {
                return base.CenterPosition;
            }
            private set
            {
                base.CenterPosition = value;
            }
        }

        /// <summary>
        /// 角度
        /// </summary>
        public new float Angle
        {
            get
            {
                return base.Angle;
            }
            set
            {
                base.Angle = value;
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
        public PhysicalRectangleShape(PhysicalShapeType shapeType, PhysicalWorld world)
        {
            density = 1;
            restitution = 0;
            friction = 0.6f;
            groupIndex = 0;
            categoryBits = 0x0001;
            maskBits = 0xffff;
            b2BodyDef = new BodyDef();
            b2PolygonDef = new PolygonDef();
            refWorld = world;
            physicalShapeType = shapeType;
            b2BodyDef.Position = PhysicalConvert.Tob2Vector(new asd.Vector2DF());
            b2PolygonDef.SetAsBox(PhysicalConvert.Tob2Single(1) / 2.0f, PhysicalConvert.Tob2Single(1) / 2.0f);
            b2Body = refWorld.B2World.CreateBody(b2BodyDef);
            b2Body.CreateFixture(b2PolygonDef);
            if (physicalShapeType == PhysicalShapeType.Dynamic) b2Body.SetMassFromShapes();
            world.Add(this);
        }

        ~PhysicalRectangleShape()
        {
            b2Body?.Dispose();
        }

        /// <summary>
        /// 削除する
        /// </summary>
        public new void Dispose()
        {
            refWorld.Destroy(this);
            base.Dispose();
        }

        public void Reset()
        {
            if (b2Body != null)
            {
                refWorld.B2World.DestroyBody(b2Body);
                b2Body.Dispose();
            }
            b2BodyDef = new BodyDef();
            b2PolygonDef = new PolygonDef();
            b2BodyDef.Position = PhysicalConvert.Tob2Vector(DrawingArea.Size / 2.0f + DrawingArea.Position);
            CenterPosition = DrawingArea.Size / 2.0f;
            b2BodyDef.Angle = Angle / 180.0f * 3.14f;
            b2PolygonDef.SetAsBox(PhysicalConvert.Tob2Single(DrawingArea.Width) / 2.0f, PhysicalConvert.Tob2Single(DrawingArea.Height) / 2.0f);
            b2PolygonDef.Density = Density;
            b2PolygonDef.Restitution = Restitution;
            b2PolygonDef.Friction = Friction;
            b2PolygonDef.Filter.GroupIndex = GroupIndex;
            b2PolygonDef.Filter.CategoryBits = CategoryBits;
            b2PolygonDef.Filter.MaskBits = MaskBits;
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
            b2Body.ApplyForce(PhysicalConvert.Tob2Vector(vector), PhysicalConvert.Tob2Vector(DrawingArea.Position + position));
        }

        /// <summary>
        /// 衝撃を加える
        /// </summary>
        /// <param name="vector">衝撃を加える方向</param>
        /// <param name="position">衝撃を加えるローカル位置</param>
        public void SetImpulse(asd.Vector2DF vector, asd.Vector2DF position)
        {
            if (!IsActive) return;
            b2Body.ApplyImpulse(PhysicalConvert.Tob2Vector(vector), PhysicalConvert.Tob2Vector(DrawingArea.Position + position));
        }

        public void SyncB2body()
        {
            if (!IsActive) return;
            asd.Vector2DF move = PhysicalConvert.ToAsdVector(b2Body.GetPosition()) - (DrawingArea.Size / 2.0f + DrawingArea.Position);
            base.DrawingArea = new asd.RectF(DrawingArea.Position + move, DrawingArea.Size);
            base.Angle = b2Body.GetAngle() * 180.0f / 3.14f;
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
                    if (IsActive == false) Reset();
                }
                else
                {
                    if (IsActive == true) b2Body.GetWorld().DestroyBody(b2Body);
                    b2Body?.Dispose();
                    b2Body = null;
                }
            }
        }

        short groupIndex;
        ushort categoryBits;
        ushort maskBits;

        /// <summary>
        /// 衝突判定グループ
        /// </summary>
        public short GroupIndex
        {
            get => groupIndex;
            set
            {
                groupIndex = value;
                Reset();
            }
        }

        /// <summary>
        /// 衝突判定カテゴリー
        /// </summary>
        public ushort CategoryBits
        {
            get => categoryBits;
            set
            {
                categoryBits = value;
                Reset();
            }
        }

        /// <summary>
        /// どのカテゴリーと衝突するか
        /// </summary>
        public ushort MaskBits
        {
            get => maskBits;
            set
            {
                maskBits = value;
                Reset();
            }
        }
    }
}
