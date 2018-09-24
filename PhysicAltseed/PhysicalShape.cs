using System;
using System.Collections.Generic;
using Box2DNet.Collision;
using Box2DNet.Common;
using Box2DNet.Dynamics;
namespace PhysicAltseed
{
    /// <summary>
    /// Altseed-Box2D変換系
    /// </summary>
    public class PhysicalConvert
    {
        public static float PxPerMetreRate = 25.0f;
        static public asd.Vector2DF ToAsdVector(Vec2 b2vector, bool isMetre = true)
        {
            if (!isMetre) return new asd.Vector2DF(b2vector.X, b2vector.Y);
            return new asd.Vector2DF(b2vector.X, b2vector.Y) * PxPerMetreRate;
        }

        static public Vec2 Tob2Vector(asd.Vector2DF asdVector, bool isMetre = true)
        {
            if (!isMetre) return new Vec2(asdVector.X, asdVector.Y);
            asd.Vector2DF temp = asdVector / PxPerMetreRate;
            return new Vec2(temp.X, temp.Y);
        }

        static public float Tob2Single(float asdSingle, bool isMetre = true)
        {
            if (!isMetre) return asdSingle;
            return asdSingle / PxPerMetreRate;
        }

        static public float ToAsdSingle(float b2Single, bool isMetre = true)
        {
            if (!isMetre) return b2Single;
            return b2Single * PxPerMetreRate;
        }
    }

    /// <summary>
    /// 物理対応図形
    /// </summary>
    public interface PhysicalShape
    {
        Body B2Body { get; }

        /// <summary>
        /// 角度
        /// </summary>
        float Angle { get; set; }

        /// <summary>
        /// 密度
        /// </summary>
        float Density { get; set; }

        /// <summary>
        /// 反発係数
        /// </summary>
        float Restitution { get; set; }

        /// <summary>
        /// 摩擦係数
        /// </summary>
        float Friction { get; set; }

        /// <summary>
        /// 速度
        /// </summary>
        asd.Vector2DF Velocity { get; set; }

        /// <summary>
        /// 角速度
        /// </summary>
        float AngularVelocity { get; set; }

        /// <summary>
        /// 削除する
        /// </summary>
        void Destroy();

        void SyncB2body();

        /// <summary>
        /// 力を加える
        /// </summary>
        /// <param name="vector">力を加える方向</param>
        /// <param name="position">力を加えるローカル位置</param>
        void SetForce(asd.Vector2DF vector, asd.Vector2DF position);

        /// <summary>
        /// 衝撃を加える
        /// </summary>
        /// <param name="vector">衝撃を加える方向</param>
        /// <param name="position">衝撃を加えるローカル位置</param>
        void SetImpulse(asd.Vector2DF vector, asd.Vector2DF position);

        /// <summary>
        /// 衝突判定
        /// </summary>
        /// <param name="shape">衝突判定対象</param>
        bool GetIsCollidedWith(asd.Shape shape);

        /// <summary>
        /// 衝突判定
        /// </summary>
        /// <param name="shape">衝突判定対象</param>
        bool GetIsCollidedWith(PhysicalShape shape);

        /// <summary>
        /// 衝突判定
        /// </summary>
        /// <param name="shape">衝突判定対象</param>
        /// <param name="points">衝突点</param>
        bool GetIsCollidedWith(PhysicalShape shape, out List<asd.Vector2DF> points);

        /// <summary>
        /// 物理シミュレーションをするか否か
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// 衝突判定グループ
        /// </summary>
        short GroupIndex { get; set; }

        /// <summary>
        /// 衝突判定カテゴリー
        /// </summary>
        ushort CategoryBits { get; set; }

        /// <summary>
        /// どのカテゴリーと衝突するか
        /// </summary>
        ushort MaskBits { get; set; }
    }

    /// <summary>
    /// 物理形状タイプ
    /// </summary>
    public enum PhysicalShapeType
    {
        Static,
        Dynamic,
        Kinematic
    }
}