using System;
using System.Runtime.InteropServices;
using Xunit;
using SolidWorks.Interop.sldworks;

namespace SldWorksEx.Tests {
    public class SwComExtensionsTests {
        #region ConvertSw<T>

        [Fact]
        public void ConvertSw_Null_ReturnsEmptyArray() {
            object input = null;
            var result = input.ConvertSw<string>();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ConvertSw_TypedArray_ReturnsSameArray() {
            var expected = new string[] { "a", "b", "c" };
            object input = expected;
            var result = input.ConvertSw<string>();
            Assert.Same(expected, result);
        }

        [Fact]
        public void ConvertSw_ObjectArray_CastsEachElement() {
            object[] input = new object[] { "hello", "world" };
            var result = ((object)input).ConvertSw<string>();
            Assert.Equal(2, result.Length);
            Assert.Equal("hello", result[0]);
            Assert.Equal("world", result[1]);
        }

        [Fact]
        public void ConvertSw_InvalidType_ThrowsInvalidCastException() {
            object input = 42;
            Assert.Throws<InvalidCastException>(() => input.ConvertSw<string>());
        }

        [Fact]
        public void ConvertSw_EmptyObjectArray_ReturnsEmptyTypedArray() {
            object[] input = Array.Empty<object>();
            var result = ((object)input).ConvertSw<string>();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region ToSw<T>

        [Fact]
        public void ToSw_MatchingType_ReturnsCast() {
            object input = "test";
            var result = input.ToSw<string>();
            Assert.Equal("test", result);
        }

        [Fact]
        public void ToSw_NonMatchingType_ReturnsNull() {
            object input = 42;
            var result = input.ToSw<string>();
            Assert.Null(result);
        }

        [Fact]
        public void ToSw_Null_ReturnsNull() {
            object input = null;
            var result = input.ToSw<string>();
            Assert.Null(result);
        }

        [Fact]
        public void ToSw_DispatchWrapper_UnwrapsCorrectly() {
            var wrapped = new DispatchWrapper("inner_value");
            var result = ((object)wrapped).ToSw<string>();
            Assert.Equal("inner_value", result);
        }

        #endregion

        #region ForceComCleanup

        [Fact]
        public void ForceComCleanup_DoesNotThrow() {
            // Purely verifying it doesn't crash
            SwComExtensions.ForceComCleanup();
        }

        #endregion
    }
}
