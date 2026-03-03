using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace CodeStack.SwEx.MathEx {
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix3 : IEquatable<Matrix3> {
        // 公开字段：直接访问，零开销
        public double M11, M12, M13;
        public double M21, M22, M23;
        public double M31, M32, M33;

        #region 静态常量
        public static Matrix3 Identity => new Matrix3(1, 0, 0, 0, 1, 0, 0, 0, 1);
        public static Matrix3 Zero => new Matrix3(0, 0, 0, 0, 0, 0, 0, 0, 0);
        #endregion

        #region 构造函数与索引
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3(double m11, double m12, double m13, double m21, double m22, double m23, double m31, double m32, double m33) {
            M11 = m11; M12 = m12; M13 = m13;
            M21 = m21; M22 = m22; M23 = m23;
            M31 = m31; M32 = m32; M33 = m33;
        }

        // 方便对接 SolidWorks API 的数组构造函数
        public Matrix3(double[] elements) {
            if(elements == null || elements.Length < 9) throw new ArgumentException("Array too small.");
            M11 = elements[0]; M12 = elements[1]; M13 = elements[2];
            M21 = elements[3]; M22 = elements[4]; M23 = elements[5];
            M31 = elements[6]; M32 = elements[7]; M33 = elements[8];
        }
        #endregion

        #region 核心数学方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Determinant() =>
            M11 * (M22 * M33 - M23 * M32) -
            M12 * (M21 * M33 - M23 * M31) +
            M13 * (M21 * M32 - M22 * M31);

        public Matrix3 Inverse() {
            double det = Determinant();
            if(MathHelper.IsZero(det)) throw new ArithmeticException("Matrix is not invertible.");
            double invDet = 1.0 / det;
            return new Matrix3(
                (M22 * M33 - M23 * M32) * invDet, (M13 * M32 - M12 * M33) *
                invDet, (M12 * M23 - M13 * M22) * invDet,
                (M23 * M31 - M21 * M33) * invDet, (M11 * M33 - M13 * M31) *
                invDet, (M13 * M21 - M11 * M23) * invDet,
                (M21 * M32 - M22 * M31) * invDet, (M12 * M31 - M11 * M32) *
                invDet, (M11 * M22 - M12 * M21) * invDet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3 Transpose() => new Matrix3(M11, M21, M31, M12, M22, M32, M13, M23, M33);
        #endregion

        #region 旋转、缩放、反射 (Factory Methods)
        public static Matrix3 RotationX(double angle) {
            double c = Math.Cos(angle), s = Math.Sin(angle);
            return new Matrix3(1, 0, 0, 0, c, -s, 0, s, c);
        }

        public static Matrix3 RotationY(double angle) {
            double c = Math.Cos(angle), s = Math.Sin(angle);
            return new Matrix3(c, 0, s, 0, 1, 0, -s, 0, c);
        }

        public static Matrix3 RotationZ(double angle) {
            double c = Math.Cos(angle), s = Math.Sin(angle);
            return new Matrix3(c, -s, 0, s, c, 0, 0, 0, 1);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(double sc) {
            M11 *= sc; M12 *= sc; M13 *= sc;
            M21 *= sc; M22 *= sc; M23 *= sc;
            M31 *= sc; M32 *= sc; M33 *= sc;
        }

        public static Matrix3 Reflection(Vector3 normal) {
            Vector3 v = Vector3.Normalize(normal);
            double x = v.X, y = v.Y, z = v.Z;
            return new Matrix3(
                1 - 2 * x * x, -2 * x * y, -2 * x * z,
                -2 * x * y, 1 - 2 * y * y, -2 * y * z,
                -2 * x * z, -2 * y * z, 1 - 2 * z * z);
        }

        #endregion

        #region 运算符重载
        public static Matrix3 operator *(in Matrix3 a, in Matrix3 b) => new Matrix3(
            a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31, a.M11 * b.M12 + a.M12 * b.M22 +
            a.M13 * b.M32, a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
            a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31, a.M21 * b.M12 + a.M22 * b.M22 +
            a.M23 * b.M32, a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
            a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31, a.M31 * b.M12 + a.M32 * b.M22 +
            a.M33 * b.M32, a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33);

        public static Vector3 operator *(in Matrix3 a, in Vector3 v) => new Vector3(
            a.M11 * v.X + a.M12 * v.Y + a.M13 * v.Z,
            a.M21 * v.X + a.M22 * v.Y + a.M23 * v.Z,
            a.M31 * v.X + a.M32 * v.Y + a.M33 * v.Z);

        #endregion

        #region 比较与格式化
        public bool Equals(Matrix3 other) => Equals(other, MathHelper.Epsilon);
        public bool Equals(in Matrix3 other, double tol) =>
            MathHelper.IsEqual(M11, other.M11, tol) &&
            MathHelper.IsEqual(M12, other.M12, tol) &&
            MathHelper.IsEqual(M13, other.M13, tol) &&
            MathHelper.IsEqual(M21, other.M21, tol) &&
            MathHelper.IsEqual(M22, other.M22, tol) &&
            MathHelper.IsEqual(M23, other.M23, tol) &&
            MathHelper.IsEqual(M31, other.M31, tol) &&
            MathHelper.IsEqual(M32, other.M32, tol) &&
            MathHelper.IsEqual(M33, other.M33, tol);

        public override bool Equals(object obj) => obj is Matrix3 other && Equals(other);
        public override int GetHashCode() {
            unchecked {
                int h = M11.GetHashCode(); h = (h * 397) ^ M12.GetHashCode(); h = (h * 397) ^ M13.GetHashCode();
                h = (h * 397) ^ M21.GetHashCode(); h = (h * 397) ^ M22.GetHashCode(); h = (h * 397) ^ M23.GetHashCode();
                h = (h * 397) ^ M31.GetHashCode(); h = (h * 397) ^ M32.GetHashCode(); h = (h * 397) ^ M33.GetHashCode();
                return h;
            }
        }

        public override string ToString() => ToString(CultureInfo.CurrentCulture);
        public string ToString(IFormatProvider provider) {
            string s = (provider as CultureInfo ?? CultureInfo.CurrentCulture).TextInfo.ListSeparator;
            return $"|{M11}{s} {M12}{s} {M13}|\n|{M21}{s} {M22}{s} {M23}|\n|{M31}{s} {M32}{s} {M33}|";
        }
        #endregion
    }
}