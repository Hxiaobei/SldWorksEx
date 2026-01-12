using System;
using System.Runtime.InteropServices;

namespace SolidWorks.Interop.sldworks {

    /// <summary>
    /// Provides extension methods for converting objects to arrays of a specified reference type.
    /// </summary>
    /// <remarks>This static class contains methods intended to simplify the conversion of objects, such as
    /// arrays or collections, to arrays of a given reference type. The methods are designed for use as extension
    /// methods and are typically called on objects that may represent arrays or collections at runtime.</remarks>
    public static class SwComExtensions {

        /// <summary>
        /// Converts the specified object to an array of type <typeparamref name="T"/>. Returns an empty array if the
        /// object is null.
        /// </summary>
        /// <remarks>If <paramref name="swObject"/> is an object array, each element is cast to
        /// <typeparamref name="T"/>. If any element cannot be cast, a runtime exception may occur.</remarks>
        /// <typeparam name="T">The reference type to which each element in the array will be cast.</typeparam>
        /// <param name="swObject">The object to convert. Can be null, an array of <typeparamref name="T"/>, or an array of objects that can be
        /// cast to <typeparamref name="T"/>.</param>
        /// <returns>An array of type <typeparamref name="T"/> containing the converted elements. Returns an empty array if
        /// <paramref name="swObject"/> is null.</returns>
        /// <exception cref="InvalidCastException">Thrown if <paramref name="swObject"/> is not null, not an array of <typeparamref name="T"/>, and cannot be
        /// cast to an array of <typeparamref name="T"/>.</exception>
        public static T[] ConvertSw<T>(this object swObject) where T : class {
            switch(swObject) {
                case null:
                    return Array.Empty<T>();
                case T[] typed:
                    return typed;
                case object[] objArray:
                    return Array.ConvertAll(objArray, o => (T)o);
                default:
                    throw new InvalidCastException($"{swObject.GetType().Name} != {typeof(T).Name}[]");
            }
        }

        public static T ToSw<T>(this object swObject) where T : class {
            if(swObject is T t)
                return t;

            if(swObject is DispatchWrapper w)
                return w.WrappedObject as T;

            return null;
        }

        public static void ForceComCleanup() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

    }
}
