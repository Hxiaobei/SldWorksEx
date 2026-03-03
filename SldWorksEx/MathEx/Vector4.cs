using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Threading;

namespace CodeStack.SwEx.MathEx {
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4 : IEquatable<Vector4>, IFormattable {
        public double X, Y, Z,W;

        #region 静态常量
        public static Vector4 Zero => new Vector4(0, 0, 0, 0);
        public static Vector4 UnitX => new Vector4(1, 0, 0, 0);
        public static Vector4 UnitY => new Vector4(0, 1, 0, 0);
        public static Vector4 UnitZ => new Vector4(0, 0, 1, 0);
        public static Vector4 UnitW => new Vector4(0, 0, 0, 1);
        public static Vector4 NaN => new Vector4(double.NaN, double.NaN, double.NaN, double.NaN);
        #endregion

        #region 构造函数
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4(double x, double y, double z, double w) { X = x; Y = y; Z = z; W = w; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4(double value) : this(value, value, value, value) { }

        public Vector4(double[] array) {
            if(array == null) throw new ArgumentNullException(nameof(array));
            if(array.Length != 4) throw new ArgumentOutOfRangeException(nameof(array), "Dimension must be 4.");
            X = array[0]; Y = array[1]; Z = array[2]; W = array[3];
        }

        public double this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                switch(index) {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    case 3: return W;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                switch(index) {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
        #endregion

        #region 属性与核心计算
        public double LengthSquared => X * X + Y * Y + Z * Z + W * W;
        public double Length => Math.Sqrt(LengthSquared);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(Vector4 u, Vector4 v) => u.X * v.X + u.Y * v.Y + u.Z * v.Z + u.W * v.W;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Distance(Vector4 u, Vector4 v) => Math.Sqrt(SquareDistance(u, v));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SquareDistance(Vector4 u, Vector4 v) {
            double dx = u.X - v.X;
            double dy = u.Y - v.Y;
            double dz = u.Z - v.Z;
            double dw = u.W - v.W;
            return dx * dx + dy * dy + dz * dz + dw * dw;
        }
        #endregion

        #region 实例方法
        public void Normalize() {
            double len = Length;
            if(MathHelper.IsZero(len)) { X = Y = Z = W = 0; } 
            else { double inv = 1.0 / len; X *= inv; Y *= inv; Z *= inv; W *= inv; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Normalize(Vector4 u) {
            double len = u.Length;
            if(MathHelper.IsZero(len)) return Zero;
            double inv = 1.0 / len;
            return new Vector4(u.X * inv, u.Y * inv, u.Z * inv, u.W * inv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 Round(int digits) => new Vector4(Math.Round(X, digits), Math.Round(Y, digits), Math.Round(Z, digits), Math.Round(W, digits));
        #endregion

        #region 运算符与比较
        public static bool operator ==(Vector4 u, Vector4 v) => u.Equals(v);
        public static bool operator !=(Vector4 u, Vector4 v) => !u.Equals(v);
        public static Vector4 operator +(Vector4 u, Vector4 v) => new Vector4(u.X + v.X, u.Y + v.Y, u.Z + v.Z, u.W + v.W);
        public static Vector4 operator -(Vector4 u, Vector4 v) => new Vector4(u.X - v.X, u.Y - v.Y, u.Z - v.Z, u.W - v.W);
        public static Vector4 operator -(Vector4 u) => new Vector4(-u.X, -u.Y, -u.Z, -u.W);
        public static Vector4 operator *(Vector4 u, double a) => new Vector4(u.X * a, u.Y * a, u.Z * a, u.W * a);
        public static Vector4 operator *(double a, Vector4 u) => new Vector4(u.X * a, u.Y * a, u.Z * a, u.W * a);
        public static Vector4 operator *(Vector4 u, Vector4 v) => new Vector4(u.X * v.X, u.Y * v.Y, u.Z * v.Z, u.W * v.W);
        public static Vector4 operator /(Vector4 u, double a) {
            double inv = 1.0 / a;
            return new Vector4(u.X * inv, u.Y * inv, u.Z * inv, u.W * inv);
        }

        public bool Equals(Vector4 other) => Equals(other, MathHelper.Epsilon);
        public bool Equals(Vector4 other, double threshold) =>
            MathHelper.IsEqual(X, other.X, threshold) &&
            MathHelper.IsEqual(Y, other.Y, threshold) &&
            MathHelper.IsEqual(Z, other.Z, threshold) &&
            MathHelper.IsEqual(W, other.W, threshold);

        public override bool Equals(object obj) => obj is Vector4 other && Equals(other);
        public override int GetHashCode() {
            unchecked {
                int hash = X.GetHashCode();
                hash = (hash * 397) ^ Y.GetHashCode();
                hash = (hash * 397) ^ Z.GetHashCode();
                hash = (hash * 397) ^ W.GetHashCode();
                return hash;
            }
        }
        #endregion

        #region 格式化
        public double[] ToArray() => new double[] { X, Y, Z, W };
        public override string ToString() => ToString(null, CultureInfo.CurrentCulture);
        public string ToString(string format, IFormatProvider provider) {
            var culture = (provider as CultureInfo) ?? CultureInfo.CurrentCulture;
            string sep = culture.TextInfo.ListSeparator;
            return string.Format("{0}{4} {1}{4} {2}{4} {3}",
                X.ToString(format, provider), Y.ToString(format, provider),
                Z.ToString(format, provider), W.ToString(format, provider), sep);
        }
        #endregion
    }
}