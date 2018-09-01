using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicAltseed
{
    /// <summary>
    /// 物理対応TextureObject2D
    /// </summary>
    public class PhysicalTextureObject2D : asd.TextureObject2D
    {

        /// <summary>
        /// 制御用Shape
        /// </summary>
        public PhysicalPolygonShape CollisionShape { get; private set; }

        /// <summary>
        /// テクスチャ
        /// </summary>
        public new asd.Texture2D Texture
        {
            get => base.Texture;
            private set
            {
                base.Texture = value;
                base.CenterPosition = base.Texture.Size.To2DF() / 2;
            }
        }

        /// <summary>
        /// 表示する座標
        /// </summary>
        public new asd.Vector2DF Position
        {
            get => base.Position;
            set
            {
                CollisionShape.CenterPosition = value;
                base.Position = value;
            }
        }

        /// <summary>
        /// 回転の中心座標(ローカル)
        /// </summary>
        public new asd.Vector2DF CenterPosition { get => base.CenterPosition; }

        /// <summary>
        /// 回転
        /// </summary>
        public new float Angle
        {
            get => base.Angle;
            set
            {
                CollisionShape.Angle = value;
                base.Angle = value;
            }
        }

        private new bool TurnLR { get; set; }
        private new bool TurnUL { get; set; }

        asd.GeometryObject2D geometryObject;

        bool isCollisionShapeVisible;
        /// <summary>
        /// 衝突用Shapeを表示する
        /// </summary>
        public bool IsCollisionShapeVisible
        {
            get => isCollisionShapeVisible;
            set
            {
                isCollisionShapeVisible = value;
                if (value)
                {
                    AddChild(geometryObject, asd.ChildManagementMode.RegistrationToLayer | asd.ChildManagementMode.Disposal, asd.ChildTransformingMode.Nothing);
                }
                else
                {
                    if (geometryObject.Parent == null) return;
                    RemoveChild(geometryObject);
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shapeType">物理形状タイプ</param>
        /// <param name="world">登録するワールド</param>
        public PhysicalTextureObject2D(PhysicalShapeType shapeType, PhysicalWorld world)
        {
            CollisionShape = new PhysicalPolygonShape(shapeType, world);
            Position = new asd.Vector2DF();
            geometryObject = new asd.GeometryObject2D();
            geometryObject.Shape = CollisionShape;
            geometryObject.Color = new asd.Color(0, 0, 255, 150);
            isCollisionShapeVisible = false;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            base.Position = CollisionShape.CenterPosition;
            base.Angle = CollisionShape.Angle;
        }

        /// <summary>
        /// テクスチャを設定する
        /// </summary>
        /// <param name="path">テクスチャへのパス</param>
        /// <param name="accuracy">ポリゴンの精度</param>
        public void SetTexture(string path, float accuracy = 0.01f)
        {
            Texture = asd.Engine.Graphics.CreateTexture2D(path);
            CollisionShape.ImageToPolygon(path, accuracy);
        }
    }
}
