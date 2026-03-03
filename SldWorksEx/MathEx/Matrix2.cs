
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Globalization;

    namespace CodeStack.SwEx.MathEx {
        [StructLayout(LayoutKind.Sequential)]
        public struct Matrix2 : IEquatable<Matrix2> {
            // 直接公开字段，追求极致性能
            public double M11, M12, M21, M22;

            #region 静态常量
            public static Matrix2 Zero => new Matrix2(0, 0, 0, 0);
            public static Matrix2 Identity => new Matrix2(1, 0, 0, 1);
            #endregion

            #region 构造函数与索引
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Matrix2(double m11, double m12, double m21, double m22) {
                M11 = m11; M12 = m12;
                M21 = m21; M22 = m22;
            }

            public double this[int row, int col] {
                get {
                    if(row == 0) return col == 0 ? M11 : M12;
                    if(row == 1) return col == 0 ? M21 : M22;
                    throw new ArgumentOutOfRangeException();
                }
                set {
                    if(row == 0) {
                        if(col == 0) M11 = value;
                        else M12 = value;
                    } else if(row == 1) {
                        if(col == 0) M21 = value;
                        else M22 = value;
                    } else throw new ArgumentOutOfRangeException();
                }
            }
            #endregion

            #region 核心数学方法

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public double Determinant() => M11 * M22 - M12 * M21;

            public Matrix2 Inverse() {
                double det = Determinant();
                if(MathHelper.IsZero(det)) throw new ArithmeticException("Matrix is not invertible.");
                double invDet = 1.0 / det;
                return new Matrix2(M22 * invDet, -M12 * invDet, -M21 * invDet, M11 * invDet);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Matrix2 Transpose() => new Matrix2(M11, M21, M12, M22);

            // 静态工厂方法
            public static Matrix2 Rotation(double angle) {
                double cos = Math.Cos(angle);
                double sin = Math.Sin(angle);
                return new Matrix2(cos, -sin, sin, cos);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Matrix2 Scale(double x, double y) => new Matrix2(x, 0, 0, y);

            #endregion

            #region 运算符重载 (使用 Lambda 简化)

            public static Matrix2 operator +(Matrix2 a, Matrix2 b) => new Matrix2(a.M11 + b.M11, a.M12 + b.M12, a.M21 + b.M21, a.M22 + b.M22);
            public static Matrix2 operator -(Matrix2 a, Matrix2 b) => new Matrix2(a.M11 - b.M11, a.M12 - b.M12, a.M21 - b.M21, a.M22 - b.M22);

            public static Matrix2 operator *(Matrix2 a, Matrix2 b) => new Matrix2(
                a.M11 * b.M11 + a.M12 * b.M21, a.M11 * b.M12 + a.M12 * b.M22,
                a.M21 * b.M11 + a.M22 * b.M21, a.M21 * b.M12 + a.M22 * b.M22
            );

            public static Vector2 operator *(Matrix2 a, Vector2 v) => new Vector2(
                a.M11 * v.X + a.M12 * v.Y,
                a.M21 * v.X + a.M22 * v.Y
            );

            public static Matrix2 operator *(Matrix2 a, double s) => new Matrix2(a.M11 * s, a.M12 * s, a.M21 * s, a.M22 * s);

            #endregion

            #region 比较与相等性
            public bool Equals(Matrix2 other) => Equals(other, MathHelper.Epsilon);

            public bool Equals(Matrix2 other, double threshold) =>
                MathHelper.IsEqual(M11, other.M11, threshold) &&
                MathHelper.IsEqual(M12, other.M12, threshold) &&
                MathHelper.IsEqual(M21, other.M21, threshold) &&
                MathHelper.IsEqual(M22, other.M22, threshold);

            public override bool Equals(object obj) => obj is Matrix2 other && Equals(other);

            public override int GetHashCode() {
                unchecked {
                    int hash = M11.GetHashCode();
                    hash = (hash * 397) ^ M12.GetHashCode();
                    hash = (hash * 397) ^ M21.GetHashCode();
                    hash = (hash * 397) ^ M22.GetHashCode();
                    return hash;
                }
            }
            #endregion

            #region 格式化
            public override string ToString() => ToString(CultureInfo.CurrentCulture);

            public string ToString(IFormatProvider provider) {
                string sep = (provider as CultureInfo ?? CultureInfo.CurrentCulture).TextInfo.ListSeparator;
                return $"|{M11.ToString(provider)}{sep} {M12.ToString(provider)}|\n|{M21.ToString(provider)}{sep} {M22.ToString(provider)}|";
            }
            #endregion
        }
    }

