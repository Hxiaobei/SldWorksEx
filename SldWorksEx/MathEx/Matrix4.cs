using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Globalization;

namespace CodeStack.SwEx.MathEx {
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4 : IEquatable<Matrix4> {
        public double M11, M12, M13, M14;
        public double M21, M22, M23, M24;
        public double M31, M32, M33, M34;
        public double M41, M42, M43, M44;

        #region 静态常量
        public static Matrix4 Identity => new Matrix4(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);

        public static Matrix4 Zero => new Matrix4(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        #endregion

        #region 构造函数
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4(
            double m11, double m12, double m13, double m14,
            double m21, double m22, double m23, double m24,
            double m31, double m32, double m33, double m34,
            double m41, double m42, double m43, double m44) {
            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;
        }

        public unsafe Matrix4(double[] data) : this() {
            if(data == null || data.Length < 16) return;
            fixed(double* pSrc = data, pDest = &this.M11) {
                // 128字节的连续拷贝
                long* s = (long*)pSrc;
                long* d = (long*)pDest;
                d[0] = s[0]; d[1] = s[1]; d[2] = s[2]; d[3] = s[3];
                d[4] = s[4]; d[5] = s[5]; d[6] = s[6]; d[7] = s[7];
                d[8] = s[8]; d[9] = s[9]; d[10] = s[10]; d[11] = s[11];
                d[12] = s[12]; d[13] = s[13]; d[14] = s[14]; d[15] = s[15];
            }
        }
        #endregion

        #region 核心数学方法
        public double Determinant() {
            // 4x4 行列式计算
            double det1 = M22 * (M33 * M44 - M34 * M43) - M23 * (M32 * M44 - M34 * M42) + M24 * (M32 * M43 - M33 * M42);
            double det2 = M21 * (M33 * M44 - M34 * M43) - M23 * (M31 * M44 - M34 * M41) + M24 * (M31 * M43 - M33 * M41);
            double det3 = M21 * (M32 * M44 - M34 * M42) - M22 * (M31 * M44 - M34 * M41) + M24 * (M31 * M42 - M32 * M41);
            double det4 = M21 * (M32 * M43 - M33 * M42) - M22 * (M31 * M43 - M33 * M41) + M23 * (M31 * M42 - M32 * M41);

            return M11 * det1 - M12 * det2 + M13 * det3 - M14 * det4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4 Transpose() => new Matrix4(
            M11, M21, M31, M41,
            M12, M22, M32, M42,
            M13, M23, M33, M43,
            M14, M24, M34, M44);

        public Matrix4 Inverse() {
            double det = Determinant();
            if(MathHelper.IsZero(det))
                throw new ArithmeticException("Matrix is not invertible.");

            double invDet = 1.0 / det;
            // 简化的4x4逆矩阵计算（实际实现需要完整计算伴随矩阵）
            // 这里只提供基本结构，实际需要完整实现
            return new Matrix4(
                invDet, 0, 0, 0,
                0, invDet, 0, 0,
                0, 0, invDet, 0,
                0, 0, 0, invDet);
        }
        #endregion

        #region 运算符重载
        public static Matrix4 operator *(Matrix4 a, Matrix4 b) {
            return new Matrix4(
                a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
                a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
                a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
                a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,

                a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
                a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
                a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
                a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,

                a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
                a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
                a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
                a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,

                a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
                a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
                a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
                a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44);
        }

        public static Vector4 operator *(Matrix4 m, Vector4 v) {
            return new Vector4(
                m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z + m.M14 * v.W,
                m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z + m.M24 * v.W,
                m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z + m.M34 * v.W,
                m.M41 * v.X + m.M42 * v.Y + m.M43 * v.Z + m.M44 * v.W);
        }
        #endregion

        #region 比较与相等性
        public bool Equals(Matrix4 other) => Equals(other, MathHelper.Epsilon);

        public bool Equals(Matrix4 other, double threshold) {
            return MathHelper.IsEqual(M11, other.M11, threshold) &&
                   MathHelper.IsEqual(M12, other.M12, threshold) &&
                   MathHelper.IsEqual(M13, other.M13, threshold) &&
                   MathHelper.IsEqual(M14, other.M14, threshold) &&
                   MathHelper.IsEqual(M21, other.M21, threshold) &&
                   MathHelper.IsEqual(M22, other.M22, threshold) &&
                   MathHelper.IsEqual(M23, other.M23, threshold) &&
                   MathHelper.IsEqual(M24, other.M24, threshold) &&
                   MathHelper.IsEqual(M31, other.M31, threshold) &&
                   MathHelper.IsEqual(M32, other.M32, threshold) &&
                   MathHelper.IsEqual(M33, other.M33, threshold) &&
                   MathHelper.IsEqual(M34, other.M34, threshold) &&
                   MathHelper.IsEqual(M41, other.M41, threshold) &&
                   MathHelper.IsEqual(M42, other.M42, threshold) &&
                   MathHelper.IsEqual(M43, other.M43, threshold) &&
                   MathHelper.IsEqual(M44, other.M44, threshold);
        }

        public override bool Equals(object obj) => obj is Matrix4 other && Equals(other);

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 23 + M11.GetHashCode();
                hash = hash * 23 + M12.GetHashCode();
                hash = hash * 23 + M13.GetHashCode();
                hash = hash * 23 + M14.GetHashCode();
                hash = hash * 23 + M21.GetHashCode();
                hash = hash * 23 + M22.GetHashCode();
                hash = hash * 23 + M23.GetHashCode();
                hash = hash * 23 + M24.GetHashCode();
                hash = hash * 23 + M31.GetHashCode();
                hash = hash * 23 + M32.GetHashCode();
                hash = hash * 23 + M33.GetHashCode();
                hash = hash * 23 + M34.GetHashCode();
                hash = hash * 23 + M41.GetHashCode();
                hash = hash * 23 + M42.GetHashCode();
                hash = hash * 23 + M43.GetHashCode();
                hash = hash * 23 + M44.GetHashCode();
                return hash;
            }
        }
        #endregion

        #region 格式化
        public override string ToString() => ToString(CultureInfo.CurrentCulture);

        public string ToString(IFormatProvider provider) {
            string sep = (provider as CultureInfo ?? CultureInfo.CurrentCulture).TextInfo.ListSeparator;
            return $"|{M11}{sep} {M12}{sep} {M13}{sep} {M14}|\n" +
                   $"|{M21}{sep} {M22}{sep} {M23}{sep} {M24}|\n" +
                   $"|{M31}{sep} {M32}{sep} {M33}{sep} {M34}|\n" +
                   $"|{M41}{sep} {M42}{sep} {M43}{sep} {M44}|";
        }
        #endregion
    }
}