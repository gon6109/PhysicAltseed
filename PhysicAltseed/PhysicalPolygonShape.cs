using System;
using System.Collections.Generic;
using Box2DNet.Collision;
using Box2DNet.Common;
using Box2DNet.Dynamics;
using System.Linq;
using OpenCvSharp;
namespace PhysicAltseed
{
    /// <summary>
    /// 物理対応多角形
    /// </summary>
    public class PhysicalPolygonShape : asd.PolygonShape, PhysicalShape
    {
        BodyDef b2BodyDef;
        List<PolygonDef> b2PolygonDefs;
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
        public PhysicalPolygonShape(PhysicalShapeType shapeType, PhysicalWorld world)
        {
            density = 1.0f;
            restitution = 0.3f;
            angle = 0.0f;
            groupIndex = 0;
            categoryBits = 0x0001;
            maskBits = 0xffff;
            b2BodyDef = new BodyDef();
            b2PolygonDefs = new List<PolygonDef>();
            vertexes = new List<asd.Vector2DF>();

            refWorld = world;
            physicalShapeType = shapeType;
            b2Body = refWorld.B2World.CreateBody(b2BodyDef);
            foreach (var item in b2PolygonDefs) b2Body.CreateFixture(item);
            if (physicalShapeType == PhysicalShapeType.Dynamic) b2Body.SetMassFromShapes();
            world.Add(this);
        }

        ~PhysicalPolygonShape()
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

        /// <summary>
        /// 多角形を構成する頂点を追加する
        /// </summary>
        /// <param name="vertex">追加する頂点</param>
        public new void AddVertex(asd.Vector2DF vertex)
        {
            if (vertexes.FindAll(obj => obj == vertex).Count != 0) return;
            vertexes.Add(vertex);
            base.AddVertex(vertex);
            Reset();
        }

        /// <summary>
        /// 多角形を構成する頂点を全て削除する
        /// </summary>
        public new void ClearVertexes()
        {
            vertexes.Clear();
            base.ClearVertexes();
            Reset();
        }

        public void Reset()
        {
            if (vertexes.Count < 3) return;

            if(b2Body != null)
            {
                refWorld.B2World.DestroyBody(b2Body);
                b2Body.Dispose();
            }

            b2BodyDef = new BodyDef();

            b2BodyDef.Angle = Angle / 180.0f * 3.14f;
            b2BodyDef.Position = PhysicalConvert.Tob2Vector(CenterPosition);

            b2PolygonDefs = DivideToTriangles(vertexes);
            b2Body = refWorld.B2World.CreateBody(b2BodyDef);

            foreach (var item in b2PolygonDefs)
            {
                b2Body.CreateFixture(item);
            }

            if (physicalShapeType == PhysicalShapeType.Dynamic) b2Body.SetMassFromShapes();
        }

        List<PolygonDef> DivideToTriangles(List<asd.Vector2DF> argVertexes)
        {
            if (argVertexes.Count < 3) return null;

            List<PolygonDef> result = new List<PolygonDef>();
            if (argVertexes.Count == 3)
            {
                result.Add(CreatePolygonDef(argVertexes[0], argVertexes[1], argVertexes[2]));
                return result;
            }

            asd.Vector2DF root = new asd.Vector2DF();
            foreach (var item in argVertexes)
            {
                if (root.Length < item.Length) root = item;
            }

            asd.Vector2DF next1, next2;
            next1 = argVertexes.IndexOf(root) != argVertexes.Count - 1 ? argVertexes[argVertexes.IndexOf(root) + 1] : argVertexes[0];
            next2 = argVertexes.IndexOf(root) != 0 ? argVertexes[argVertexes.IndexOf(root) - 1] : argVertexes[argVertexes.Count - 1];

            float cross = asd.Vector2DF.Cross(next1 - root, next2 - root);
            while (true)
            {
                bool isDivideble = true;
                foreach (var item in argVertexes)
                {
                    if (IsContainAtTriangle(root, next1, next2, item)) isDivideble = false;
                }

                if (!isDivideble)
                {
                    do
                    {
                        root = argVertexes.IndexOf(root) != argVertexes.Count - 1 ? argVertexes[argVertexes.IndexOf(root) + 1] : argVertexes[0];
                        next1 = argVertexes.IndexOf(next1) != argVertexes.Count - 1 ? argVertexes[argVertexes.IndexOf(next1) + 1] : argVertexes[0];
                        next2 = argVertexes.IndexOf(next2) != argVertexes.Count - 1 ? argVertexes[argVertexes.IndexOf(next2) + 1] : argVertexes[0];
                    } while (System.Math.Sign(cross) != System.Math.Sign(asd.Vector2DF.Cross(next1 - root, next2 - root)));
                }
                else break;
            }

            result.Add(CreatePolygonDef(root, next1, next2));
            List<asd.Vector2DF> remain = new List<asd.Vector2DF>(argVertexes);
            remain.Remove(root);
            result.AddRange(DivideToTriangles(remain));
            return result;
        }

        bool IsContainAtTriangle(asd.Vector2DF vertex1, asd.Vector2DF vertex2, asd.Vector2DF vertex3, asd.Vector2DF vector)
        {
            float c1 = asd.Vector2DF.Cross(vertex2 - vertex1, vector - vertex2);
            float c2 = asd.Vector2DF.Cross(vertex3 - vertex2, vector - vertex3);
            float c3 = asd.Vector2DF.Cross(vertex1 - vertex3, vector - vertex1);

            if ((c1 > 0 && c2 > 0 && c3 > 0) || (c1 < 0 && c2 < 0 && c3 < 0)) return true;

            return false;
        }

        PolygonDef CreatePolygonDef(asd.Vector2DF vertex1, asd.Vector2DF vertex2, asd.Vector2DF vertex3)
        {
            PolygonDef b2PolygonDef = new PolygonDef();
            var sortedVertexes = new List<asd.Vector2DF>();
            sortedVertexes.Add(vertex1);
            sortedVertexes.Add(vertex2);
            sortedVertexes.Add(vertex3);
            sortedVertexes.Sort((a, b) => a.Degree.CompareTo(b.Degree));
            b2PolygonDef.VertexCount = 3;
            for (int i = 0; i < 3; i++)
            {
                b2PolygonDef.Vertices[i] = PhysicalConvert.Tob2Vector(sortedVertexes[i]);
            }

            b2PolygonDef.Density = Density;
            b2PolygonDef.Restitution = Restitution;
            b2PolygonDef.Friction = Friction;
            b2PolygonDef.Filter = new FilterData()
            {
                GroupIndex = GroupIndex,
                CategoryBits = CategoryBits,
                MaskBits = MaskBits
            };
            return b2PolygonDef;
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
            base.ClearVertexes();
            foreach (var item in vertexes)
            {
                asd.Vector2DF temp = item;
                temp.Degree += Angle;
                base.AddVertex(CenterPosition + temp);
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

        /// <summary>
        /// 画像の透過情報からポリゴンに変換
        /// </summary>
        /// <param name="path">画像へのパス</param>
        /// <param name="accuracy">精度</param>
        public void ImageToPolygon(string path, float accuracy = 0.01f)
        {
            ClearVertexes();
            using (var image = Cv2.ImRead(path, ImreadModes.Unchanged))
            {
                for (int i = 0; i < image.Rows; i++)
                {
                    for (int l = 0; l < image.Cols; l++)
                    {
                        Vec4b px = image.At<Vec4b>(i, l);
                        px[0] = px[3];
                        px[1] = px[3];
                        px[2] = px[3];
                        image.Set<Vec4b>(i, l, px);
                    }
                }
                var newImage = image.CvtColor(ColorConversionCodes.RGBA2GRAY);
                Cv2.Threshold(newImage, newImage, 0.0, 255.0, ThresholdTypes.Otsu);

                Point[][] countours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(newImage, out countours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxTC89L1);
                float area = 0;
                Point[] approx = null;
                foreach (Point[] item in countours)
                {
                    Point[] temp;
                    temp = Cv2.ApproxPolyDP(item, accuracy * Cv2.ArcLength(item, true), true);
                    if (area < Cv2.ContourArea(temp)) approx = temp;
                }

                if (approx == null) return;
                foreach (var item in approx)
                {
                    AddVertex(new asd.Vector2DF(item.X, item.Y) - new asd.Vector2DF(image.Cols, image.Rows) / 2.0f);
                }
            }
        }
    }
}
