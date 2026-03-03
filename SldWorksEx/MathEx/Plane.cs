using System;
using System.Runtime.CompilerServices;

namespace CodeStack.SwEx.MathEx {
    public struct Plane : IEquatable<Plane> {
        public Vector3 Normal;
        public double Distance;

        #region 构造函数
        public Plane(in Vector3 normal, double distance) {
            Normal = normal;
            Distance = distance;
        }

        public Plane(in Vector3 normal, Vector3 point) {
            Normal = Vector3.Normalize(normal);
            Distance = -Vector3.Dot(Normal, point);
        }

        public Plane(Vector3 a, Vector3 b, Vector3 c) {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Normal = Vector3.Cross(ab, ac);
            Normal.Normalize();
            Distance = -Vector3.Dot(Normal, a);
        }
        #endregion

        #region 属性
        public double A => Normal.X;
        public double B => Normal.Y;
        public double C => Normal.Z;
        public double D => Distance;
        #endregion

        #region 方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double DistanceToPoint(Vector3 point) {
            return Vector3.Dot(Normal, point) + Distance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ProjectPoint(Vector3 point) {
            double dist = DistanceToPoint(point);
            return point - Normal * dist;
        }

        public Vector3? IntersectLine(Vector3 linePoint, Vector3 lineDirection) {
            double denom = Vector3.Dot(Normal, lineDirection);
            if(MathHelper.IsZero(denom))
                return null; // 直线与平面平行

            double t = -(Vector3.Dot(Normal, linePoint) + Distance) / denom;
            return linePoint + lineDirection * t;
        }

        public static Plane Normalize(Plane plane) {
            double length = plane.Normal.Length;
            if(MathHelper.IsZero(length))
                return new Plane(Vector3.UnitZ, 0);

            double invLength = 1.0 / length;
            return new Plane(plane.Normal * invLength, plane.Distance * invLength);
        }
        #endregion

        #region 比较与相等性
        public bool Equals(Plane other) => Equals(other, MathHelper.Epsilon);

        public bool Equals(Plane other, double threshold) {
            return Normal.Equals(other.Normal, threshold) &&
                   MathHelper.IsEqual(Distance, other.Distance, threshold);
        }

        public override bool Equals(object obj) => obj is Plane other && Equals(other);

        public override int GetHashCode() {
            return Normal.GetHashCode() ^ Distance.GetHashCode();
        }
        #endregion

        #region 格式化
        public override string ToString() {
            return $"Plane(Normal: {Normal}, Distance: {Distance})";
        }
        #endregion
    }
}