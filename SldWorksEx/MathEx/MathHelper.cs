using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.SwEx.MathEx {
    public static class MathHelper {

        public const double DegToRad = Math.PI / 180.0;

        public const double RadToDeg = 180.0 / Math.PI;

        public const double Tolerance = 1E-12;

        public const double LinearTolerance = 1E-6;

        public const double AngularTolerance = 1E-11;

        public const double HalfPI = 0.5 * Math.PI;

        public const double PI = Math.PI;

        public const double TwoPI = 2 * Math.PI;

        private static double _epsilon = Tolerance;

        public static double Epsilon {
            get => _epsilon;
            set {
                if(value <= 0.0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Epsilon must be positive.");
                _epsilon = value;
            }
        }

        #region 基础判断 (内联优化)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(double number) => IsZero(number, _epsilon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(double number, double threshold)
            // 优化逻辑：Math.Abs 比原始的 >= -threshold && <= threshold 更清晰且在 JIT 中有专门优化
            => Math.Abs(number) <= threshold;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOne(double number) => IsZero(number - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOne(double number, double threshold) => IsZero(number - 1, threshold);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEqual(double a, double b) => IsZero(a - b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEqual(double a, double b, double threshold) => IsZero(a - b, threshold);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(double number) => IsZero(number) ? 0 : Math.Sign(number);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(double number, double threshold) => IsZero(number, threshold) ? 0 : Math.Sign(number);

        #endregion

        /// <summary>
        /// 构建任意轴坐标系 - 提高健壮性
        /// </summary>
        public static Matrix3 ArbitraryAxis(Vector3 axis) {
            axis.Normalize();
            // 如果已经是 Z 轴，直接返回单位阵
            if(Vector3.AreParallel(axis, Vector3.UnitZ))
                return Matrix3.Identity;

            // 改进：根据 CAD 常规做法，寻找最不平行的轴
            Vector3 v = (Math.Abs(axis.X) < 0.707 && Math.Abs(axis.Y) < 0.707)
                        ? Vector3.Cross(Vector3.UnitY, axis)
                        : Vector3.Cross(Vector3.UnitZ, axis);

            v.Normalize();
            Vector3 u = Vector3.Cross(axis, v);
            u.Normalize();

            // 构造旋转矩阵
            return new Matrix3(v.X, u.X, axis.X, v.Y, u.Y, axis.Y, v.Z, u.Z, axis.Z);
        }

        /// <summary>
        /// 点到直线的距离 - 优化了重复的向量减法
        /// </summary>
        public static double PointLineDistance(Vector3 p, Vector3 origin, Vector3 dir) {
            Vector3 diff = p - origin;
            double t = Vector3.Dot(dir, diff);
            Vector3 projection = origin + t * dir;
            return Vector3.Distance(p, projection);
        }

        public static double PointLineDistance(Vector2 p, Vector2 origin, Vector2 dir) {
            double num = Vector2.Dot(dir, p - origin);
            Vector2 vector = origin + num * dir;
            Vector2 vector2 = p - vector;
            return Math.Sqrt(Vector2.Dot(vector2, vector2));
        }

        /// <summary>
        /// 判断点在线段上的位置 (-1: 起点前, 0: 线段内, 1: 终点后)
        /// </summary>
        public static int PointInSegment(Vector3 p, Vector3 start, Vector3 end) {
            Vector3 seg = end - start;
            Vector3 vecP = p - start;
            double dot = Vector3.Dot(seg, vecP);
            if(dot < 0.0) return -1;

            double lenSq = seg.LengthSquared; // 使用属性代替方法，避免内部重复计算
            return dot > lenSq ? 1 : 0;
        }

        public static int PointInSegment(Vector2 p, Vector2 start, Vector2 end) {
            Vector2 vector = end - start;
            Vector2 v = p - start;
            double num = Vector2.Dot(vector, v);
            if(num < 0.0) {
                return -1;
            }
            double num2 = Vector2.Dot(vector, vector);
            if(num > num2) {
                return 1;
            }
            return 0;
        }

    }

}
