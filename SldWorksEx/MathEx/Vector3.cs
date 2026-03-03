using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace CodeStack.SwEx.MathEx {
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3 : IEquatable<Vector3>, IFormattable {
        public double X, Y, Z;

        // --- 静态常量 ---
        public static Vector3 Zero => new Vector3(0, 0, 0);
        public static Vector3 UnitX => new Vector3(1, 0, 0);
        public static Vector3 UnitY => new Vector3(0, 1, 0);
        public static Vector3 UnitZ => new Vector3(0, 0, 1);
        public static Vector3 NaN => new Vector3(double.NaN, double.NaN, double.NaN);

        #region 构造函数与索引器
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(double value) : this(value, value, value) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(double x, double y, double z) { X = x; Y = y; Z = z; }

        public Vector3(double[] array) {
            if(array == null) throw new ArgumentNullException(nameof(array));
            if(array.Length != 3) throw new ArgumentOutOfRangeException(nameof(array), "数组维度必须为3。");
            X = array[0]; Y = array[1]; Z = array[2];
        }

        public double this[int index] {
            get {
                switch(index) {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
            set {
                switch(index) {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #endregion

        #region 属性
        public double LengthSquared => X * X + Y * Y + Z * Z;
        public double Length => Math.Sqrt(LengthSquared);
        #endregion

        #region 实例方法
        public void Multiply(double a) { X *= a; Y *= a; Z *= a; }
        public void Multiply(in Vector3 v) { X *= v.X; Y *= v.Y; Z *= v.Z; }
        public double[] ToArray() => new[] { X, Y, Z };
        public void Normalize() {
            double len = Length;
            if(MathHelper.IsZero(len)) { this = Zero; } else { double inv = 1.0 / len; X *= inv; Y *= inv; Z *= inv; }
        }
        #endregion

        #region 常用工具方法 (找回并优化)
        public Vector3 Move(in Vector3 dir, double dist) => this + Normalize(dir) * dist;
        // 判断是否包含 NaN
        public static bool IsNaN(in Vector3 u) => double.IsNaN(u.X) || double.IsNaN(u.Y) || double.IsNaN(u.Z);

        // 判断是否为零向量 (联动 MathHelper)
        public static bool IsZero(in Vector3 u) => MathHelper.IsZero(u.X) && MathHelper.IsZero(u.Y) && MathHelper.IsZero(u.Z);
        public static bool IsZero(in Vector3 u, double threshold)
            => MathHelper.IsZero(u.X, threshold) && MathHelper.IsZero(u.Y, threshold) && MathHelper.IsZero(u.Z, threshold);

        // 两点间距离
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Distance(in Vector3 u, in Vector3 v) => (u - v).Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SquareDistance(in Vector3 u, in Vector3 v) => (u - v).LengthSquared;

        // 中点
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 MidPoint(in Vector3 u, in Vector3 v)
            => new Vector3((u.X + v.X) * 0.5, (u.Y + v.Y) * 0.5, (u.Z + v.Z) * 0.5);

        // 舍入
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Round(in Vector3 u, int numDigits)
            => new Vector3(Math.Round(u.X, numDigits), Math.Round(u.Y, numDigits), Math.Round(u.Z, numDigits));

        // 夹角 (弧度)
        public static double AngleBetween(in Vector3 u, in Vector3 v) {
            double den = Math.Sqrt(u.LengthSquared * v.LengthSquared);
            if(MathHelper.IsZero(den)) return 0;
            double cos = Dot(u, v) / den;
            if(cos >= 1.0) return 0;
            if(cos <= -1.0) return Math.PI;
            return Math.Acos(cos);
        }

        // 绕轴旋转 (使用罗德里格旋转公式向量化)
        public static Vector3 RotateAroundAxis(in Vector3 v, in Vector3 axis, double angle) {
            double lenSq = axis.LengthSquared;
            if(MathHelper.IsZero(lenSq)) return v;

            Vector3 k = axis * (1.0 / Math.Sqrt(lenSq)); // 归一化轴
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            // 公式: v_rot = v*cos + (k x v)*sin + k*(k.v)*(1-cos)
            return (v * cos) + (Cross(k, v) * sin) + (k * Dot(k, v) * (1.0 - cos));
        }

        // 投影: 将向量 p 投影到向量 direction 上
        public static Vector3 Project(in Vector3 p, in Vector3 direction) {
            double lenSq = direction.LengthSquared;
            return MathHelper.IsZero(lenSq) ? Zero : direction * (Dot(p, direction) / lenSq);
        }

        #endregion

        #region 核心数学与归一化
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(in Vector3 u, in Vector3 v) => u.X * v.X + u.Y * v.Y + u.Z * v.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cross(in Vector3 u, in Vector3 v)
            => new Vector3(u.Y * v.Z - u.Z * v.Y, u.Z * v.X - u.X * v.Z, u.X * v.Y - u.Y * v.X);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(in Vector3 u) {
            double len = u.Length;
            if(MathHelper.IsZero(len)) return Zero;
            double inv = 1.0 / len;
            return new Vector3(u.X * inv, u.Y * inv, u.Z * inv);
        }
        #endregion

        #region 关系判断 (联动 MathHelper)
        public bool Equals(Vector3 other) => Equals(other, MathHelper.Epsilon);
        public bool Equals(in Vector3 other, double threshold) => MathHelper.IsEqual(X, other.X, threshold) &&
            MathHelper.IsEqual(Y, other.Y, threshold) && MathHelper.IsEqual(Z, other.Z, threshold);

        public static bool ArePerpendicular(in Vector3 u, in Vector3 v) => ArePerpendicular(u, v, MathHelper.Epsilon);
        public static bool ArePerpendicular(in Vector3 u, in Vector3 v, double threshold) => MathHelper.IsZero(Dot(u, v), threshold);

        public static bool AreParallel(in Vector3 u, in Vector3 v) => AreParallel(u, v, MathHelper.Epsilon);

        // 叉积为零则平行
        public static bool AreParallel(in Vector3 u, in Vector3 v, double threshold) => IsZero(Cross(u, v), threshold);

        #endregion

        #region 运算符重载
        public static bool operator ==(in Vector3 u, in Vector3 v) => u.Equals(v);
        public static bool operator !=(in Vector3 u, in Vector3 v) => !u.Equals(v);
        public static Vector3 operator +(in Vector3 u, in Vector3 v) => new Vector3(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        public static Vector3 operator -(in Vector3 u, in Vector3 v) => new Vector3(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        public static Vector3 operator -(in Vector3 u) => new Vector3(-u.X, -u.Y, -u.Z);
        public static Vector3 operator *(in Vector3 u, double a) => new Vector3(u.X * a, u.Y * a, u.Z * a);
        public static Vector3 operator *(double a, in Vector3 u) => new Vector3(u.X * a, u.Y * a, u.Z * a);
        public static Vector3 operator *(in Vector3 u, in Vector3 v) => new Vector3(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
        public static Vector3 operator /(in Vector3 u, double a) {
            double inv = 1.0 / a;
            return new Vector3(u.X * inv, u.Y * inv, u.Z * inv);
        }
        #endregion



        #region 重写
        public override bool Equals(object obj) => obj is Vector3 other && Equals(other);
        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Z.GetHashCode();
                return hash;
            }
        }
        public override string ToString() => ToString(null, CultureInfo.CurrentCulture);
        public string ToString(IFormatProvider provider) => ToString(null, provider);
        public string ToString(string format, IFormatProvider provider) {
            var culture = (provider as CultureInfo) ?? CultureInfo.CurrentCulture;
            string sep = culture.TextInfo.ListSeparator;
            return string.Format("{0}{3} {1}{3} {2}", X.ToString(format, provider), Y.ToString(format, provider), Z.ToString(format, provider), sep);
        }
        #endregion
    }
}