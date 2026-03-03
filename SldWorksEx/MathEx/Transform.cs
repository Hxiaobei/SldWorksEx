using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
namespace CodeStack.SwEx.MathEx {
    public struct Transform : IEquatable<Transform> {
        public Matrix3 Rotation;
        public Vector3 Trans;

        public Transform(in Matrix3 matrix, in Vector3 transVec) {
            this.Rotation = matrix; this.Trans = transVec;
        }

        /// <summary>
        /// 创建从轴向量构造的变换
        /// </summary>
        public Transform(in Vector3 xVec, in Vector3 yVec, in Vector3 zVec, in Vector3 transVec) {
            var normalizedX = Vector3.Normalize(xVec);
            var normalizedY = Vector3.Normalize(yVec);
            var normalizedZ = Vector3.Normalize(zVec);

            this.Rotation = new Matrix3(
                normalizedX.X, normalizedY.X, normalizedZ.X,
                normalizedX.Y, normalizedY.Y, normalizedZ.Y,
                normalizedX.Z, normalizedY.Z, normalizedZ.Z
            );
            this.Trans = transVec;
        }

        /// <summary>
        /// 变换点坐标,切勿用于变换方向向量
        /// </summary>
        /// <param name="a"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public Vector3 TransPoint(in Vector3 pt) => Rotation * pt + Trans;

        /// <summary>
        /// 向量方向变换，仅变换方向，不进行平移
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public Vector3 TransVector(in Vector3 vec) => Rotation * vec;

        public void ScaleLocally(double sc) => Rotation.Scale(sc);

        public void ScaleTransform(double factor) {
            Rotation.Scale(factor);
            Trans.Multiply(factor);
        }

        public Transform MoveLocally(in Vector3 localOffset)
            => new Transform(Rotation, Trans + Rotation * localOffset);
        public Transform MoveTransform(in Vector3 translation)
            => new Transform(Rotation, Trans + translation);

        public Transform Inverse() {
            var invMatrix = Rotation.Inverse();
            var invTransVec = invMatrix * (-Trans);
            return new Transform(invMatrix, invTransVec);
        }

        public static Transform FromCoordinateSystems(
           in Vector3 sourceX, in Vector3 sourceY, in Vector3 sourceOrigin,
           in Vector3 targetX, in Vector3 targetY, in Vector3 targetOrigin) {
            // 直接计算变换矩阵，避免中间Transform构造
            var sourceRot = CreateRotationMatrix(sourceX, sourceY);
            var targetRot = CreateRotationMatrix(targetX, targetY);

            // 对于正交矩阵，转置=逆
            // R = R_target * R_source^T
            var rotation = targetRot * sourceRot.Transpose();
            // t = O_target - R * O_source
            var translation = targetOrigin - rotation * sourceOrigin;

            return new Transform(rotation, translation);

            Matrix3 CreateRotationMatrix(in Vector3 xAxis, in Vector3 yAxis) {
                var zAxis = Vector3.Cross(xAxis, yAxis);
                return new Matrix3(
                    xAxis.X, yAxis.X, zAxis.X,
                    xAxis.Y, yAxis.Y, zAxis.Y,
                    xAxis.Z, yAxis.Z, zAxis.Z);
            }
        }

        /// <summary>
        /// 创建从局部坐标系到世界坐标系的变换
        /// </summary>
        public static Transform FromLocalToWorld(in Vector3 xAxis, in Vector3 yAxis, in Vector3 origin) {
            var zAxis = Vector3.Cross(xAxis, yAxis);

            // 构建旋转矩阵（局部基向量在世界坐标系中的表示）
            var rotation = new Matrix3(
                xAxis.X, yAxis.X, zAxis.X,
                xAxis.Y, yAxis.Y, zAxis.Y,
                xAxis.Z, yAxis.Z, zAxis.Z
            );

            return new Transform(rotation, origin);
        }

        public static Transform Translation(in Vector3 translation)
            => new Transform(Matrix3.Identity, translation);

        public static Transform RotationX(double angle)
            => new Transform(Matrix3.RotationX(angle), Vector3.Zero);

        public static Transform RotationY(double angle)
            => new Transform(Matrix3.RotationY(angle), Vector3.Zero);

        public static Transform RotationZ(double angle)
            => new Transform(Matrix3.RotationZ(angle), Vector3.Zero);


        /// <summary>
        /// 变换点坐标,切勿用于变换方向向量
        /// </summary>
        /// <param name="a"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Vector3 operator *(in Transform a, Vector3 pt)
            => a.Rotation * pt + a.Trans;

        /// <summary>
        /// 变换矩阵变换
        /// </summary>
        /// <param name="a"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Transform operator *(in Matrix3 m, in Transform a)
            => new Transform(m * a.Rotation, a.Trans);

        /// <summary>
        /// 变换组合：先应用b变换，再应用a变换
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Transform operator *(in Transform a, in Transform b) {
            var newMatrix = a.Rotation * b.Rotation;
            var newTranslation = a.Rotation * b.Trans + a.Trans;
            return new Transform(newMatrix, newTranslation);
        }

        public static bool Equals(in Transform a, in Transform b)
            => a.Rotation.Equals(b.Rotation) && a.Trans.Equals(b.Trans);

        public static bool Equals(in Transform a, in Transform b, double threshold)
            => a.Rotation.Equals(b.Rotation, threshold) && a.Trans.Equals(b.Trans, threshold);

        public bool Equals(Transform other)
            => Rotation.Equals(other.Rotation) && Trans.Equals(other.Trans);

        public bool Equals(in Transform other, double threshold)
            => Rotation.Equals(other.Rotation, threshold) && Trans.Equals(other.Trans, threshold);

        public static bool operator !=(in Transform a, in Transform b)
            => !Equals(a, b);

        public static bool operator ==(in Transform a, in Transform b)
            => Equals(a, b);

        public override int GetHashCode() => Rotation.GetHashCode() ^ Trans.GetHashCode();

        public override bool Equals(object obj) => (obj is Transform other) && Equals(other);

        public override string ToString() => $"{Rotation}\n[{Trans}]";
    }
}