using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Globalization;

namespace CodeStack.SwEx.MathEx {
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2 : IEquatable<Vector2>, IFormattable {
        public double X, Y;

        #region 常量
        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 UnitX => new Vector2(1, 0);
        public static Vector2 UnitY => new Vector2(0, 1);
        public static Vector2 NaN => new Vector2(double.NaN, double.NaN);
        #endregion

        #region 构造函数与索引
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2(double x, double y) { X = x; Y = y; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2(double value) : this(value, value) { }

        public Vector2(double[] array) {
            if(array == null) throw new ArgumentNullException(nameof(array));
            if(array.Length != 2) throw new ArgumentOutOfRangeException(nameof(array), "Dimension must be two.");
            X = array[0]; Y = array[1];
        }

        public double this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                switch(index) {
                    case 0: return X;
                    case 1: return Y;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                switch(index) {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #endregion

        #region 属性
        public double LengthSquared => X * X + Y * Y;
        public double Length => Math.Sqrt(LengthSquared);
        #endregion

        #region 静态几何方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(Vector2 u, Vector2 v) => u.X * v.X + u.Y * v.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Cross(Vector2 u, Vector2 v) => u.X * v.Y - u.Y * v.X;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Distance(Vector2 u, Vector2 v) => Math.Sqrt(SquareDistance(u, v));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SquareDistance(Vector2 u, Vector2 v) {
            double dx = u.X - v.X;
            double dy = u.Y - v.Y;
            return dx * dx + dy * dy;
        }

        /// <summary>
        /// 逆时针 90 度垂直向量
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Perpendicular(Vector2 u) => new Vector2(-u.Y, u.X);

        /// <summary>
        /// 旋转向量
        /// </summary>
        public static Vector2 Rotate(Vector2 u, double angle) {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);
            return new Vector2(u.X * cos - u.Y * sin, u.X * sin + u.Y * cos);
        }

        /// <summary>
        /// 极坐标位移
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Polar(Vector2 u, double distance, double angle)
            => u + new Vector2(Math.Cos(angle), Math.Sin(angle)) * distance;

        /// <summary>
        /// 向量与 X 轴正方向夹角 (0 到 2PI)
        /// </summary>
        public static double Angle(Vector2 u) {
            double ang = Math.Atan2(u.Y, u.X);
            return ang < 0.0 ? ang + MathHelper.TwoPI : ang;
        }

        public static double AngleBetween(Vector2 u, Vector2 v) {
            double den = Math.Sqrt(u.LengthSquared * v.LengthSquared);
            if(MathHelper.IsZero(den)) return 0.0;
            double cos = Dot(u, v) / den;
            return Math.Acos(cos < -1.0 ? -1.0 : (cos > 1.0 ? 1.0 : cos));
        }
        #endregion

        #region 归一化与比较
        public void Normalize() {
            double len = Length;
            if(MathHelper.IsZero(len)) { X = Y = 0; } else { double inv = 1.0 / len; X *= inv; Y *= inv; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Normalize(Vector2 u) {
            double len = u.Length;
            if(MathHelper.IsZero(len)) return Zero;
            double inv = 1.0 / len;
            return new Vector2(u.X * inv, u.Y * inv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2 other) => Equals(other, MathHelper.Epsilon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2 other, double threshold)
            => MathHelper.IsEqual(X, other.X, threshold) && MathHelper.IsEqual(Y, other.Y, threshold);

        public override bool Equals(object obj) => obj is Vector2 other && Equals(other);

        public override int GetHashCode() {
            unchecked { return (X.GetHashCode() * 397) ^ Y.GetHashCode(); }
        }
        #endregion

        #region 运算符重载 (Lambda)
        public static bool operator ==(Vector2 u, Vector2 v) => u.Equals(v);
        public static bool operator !=(Vector2 u, Vector2 v) => !u.Equals(v);
        public static Vector2 operator +(Vector2 u, Vector2 v) => new Vector2(u.X + v.X, u.Y + v.Y);
        public static Vector2 operator -(Vector2 u, Vector2 v) => new Vector2(u.X - v.X, u.Y - v.Y);
        public static Vector2 operator -(Vector2 u) => new Vector2(-u.X, -u.Y);
        public static Vector2 operator *(Vector2 u, double a) => new Vector2(u.X * a, u.Y * a);
        public static Vector2 operator *(double a, Vector2 u) => new Vector2(u.X * a, u.Y * a);
        public static Vector2 operator *(Vector2 u, Vector2 v) => new Vector2(u.X * v.X, u.Y * v.Y);
        public static Vector2 operator /(Vector2 u, double a) { double inv = 1.0 / a; return new Vector2(u.X * inv, u.Y * inv); }
        #endregion

        #region 格式化
        public double[] ToArray() => new[] { X, Y };
        public override string ToString() => ToString(null, CultureInfo.CurrentCulture);
        public string ToString(string format, IFormatProvider provider) {
            var culture = (provider as CultureInfo) ?? CultureInfo.CurrentCulture;
            string sep = culture.TextInfo.ListSeparator;
            return string.Format("{0}{2} {1}", X.ToString(format, provider), Y.ToString(format, provider), sep);
        }
        #endregion
    }
}