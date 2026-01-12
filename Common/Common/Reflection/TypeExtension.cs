//**********************
//SwEx - development tools for SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-common/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex
//**********************

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;

namespace CodeStack.SwEx.Common.Reflection {
    /// <summary>
    /// Provides the extension methods for <see cref="Type"/>
    /// </summary>
    public static class TypeExtension {

        static readonly ConcurrentDictionary<ValueTuple<Type, Type>, Attribute> _cache =
          new ConcurrentDictionary<ValueTuple<Type, Type>, Attribute>();

        /// <summary>
        /// Attempts to get the attribute from the type, all parent types and interfaces
        /// </summary>
        /// <typeparam name="TAtt">Type of the attribute</typeparam>
        /// <param name="type">Type to get attribute from</param>
        /// <param name="att">Attribute of the type</param>
        /// <returns>True if attribute exists</returns>
        public static bool TryGetAttribute<TAtt>(this Type type, out TAtt att) where TAtt : Attribute {

            // 1. 缓存命中
            var key = ValueTuple.Create(type, typeof(TAtt));
            if(_cache.TryGetValue(key, out Attribute cached))
                return (att = cached as TAtt) != null;

            // 2. 查当前类型
            att = type.GetCustomAttributes(typeof(TAtt), true)
                .FirstOrDefault() as TAtt;

            // 3. 查接口
            if(att == null) {
                foreach(var iface in type.GetInterfaces()) {
                    var ifaceAtts = iface.GetCustomAttributes(typeof(TAtt), true);
                    if(ifaceAtts.Length == 0) continue;
                    att = (TAtt)ifaceAtts[0];
                    if(att != null) break;
                }
            }

            // 4. 查基类链
            if(att == null) {
                var baseType = type.BaseType;
                while(baseType != null && baseType != typeof(object)) {
                    var baseAtts = baseType.GetCustomAttributes(typeof(TAtt), true);

                    if(baseAtts.Length > 0) {
                        att = baseAtts[0] as TAtt;
                        if(att != null) break;
                    }

                    baseType = baseType.BaseType;
                }
            }

            // 5. 写入缓存
            _cache[key] = att;

            return att != null;
        }

        /// <summary>
        /// Attempts to the attribute from type, all parent types and interfaces
        /// </summary>
        /// <typeparam name="TAtt">Type of the attribute</typeparam>
        /// <param name="type">Type to get attribute from</param>
        /// <returns>Attribute or null if not found</returns>
        public static TAtt TryGetAttribute<TAtt>(this Type type) where TAtt : Attribute
            => type.TryGetAttribute<TAtt>(out var att) ? att : null;

        /// <summary>
        /// Get the specified attribute from the type, all parent types and interfaces
        /// </summary>
        /// <typeparam name="TAtt">Attribute type</typeparam>
        /// <param name="type">Type</param>
        /// <returns>Attribute</returns>
        /// <exception cref="NullReferenceException"/>
        /// <remarks>This method throws an exception if attribute is missing</remarks>
        public static TAtt GetAttribute<TAtt>(this Type type) where TAtt : Attribute
            => type.TryGetAttribute<TAtt>() ??
            throw new InvalidOperationException($"Attribute {typeof(TAtt).Name} not found on {type.FullName}");

        /// <summary>
        /// Checks if this type can be assigned to the generic type
        /// </summary>
        /// <param name="thisType">Type</param>
        /// <param name="genericType">Base generic type (i.e. MyGenericType&lt;&gt;)</param>
        /// <returns>True if type is assignable to generic</returns>
        public static bool IsAssignableToGenericType(this Type thisType, Type genericType)
            => thisType.TryFindGenericType(genericType) != null;

        /// <summary>
        /// Gets the specific arguments of this type in relation to specified generic type
        /// </summary>
        /// <param name="thisType">This type which must be assignable to the specified genericType</param>
        /// <param name="genericType">Generic type</param>
        /// <returns>Arguments</returns>
        /// <remarks>For example this method called on List&lt;string&gt; where the genericType is IEnumerable&lt;&gt; would return string</remarks>
        public static Type[] GetArgumentsOfGenericType(this Type thisType, Type genericType)
            => thisType.TryFindGenericType(genericType)?.GetGenericArguments() ?? Type.EmptyTypes;

        /// <summary>
        /// Finds the specific generic type to a specified base generic type
        /// </summary>
        /// <param name="thisType">This type</param>
        /// <param name="genericType">Base generic type</param>
        /// <returns>Specific generic type or null if not found</returns>
        public static Type TryFindGenericType(this Type thisType, Type genericType) {
            bool canCastFunc(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == genericType;

            while(thisType != null) {
                foreach(var it in thisType.GetInterfaces())
                    if(canCastFunc(it)) return it;

                if(canCastFunc(thisType)) return thisType;

                thisType = thisType.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Returns the COM ProgId of a type
        /// </summary>
        /// <param name="type">Input type</param>
        /// <returns>COM Prog id</returns>
        public static string GetProgId(this Type type)
            => type.TryGetAttribute<ProgIdAttribute>(out var att) ? att.Value : type.FullName;

        /// <summary>
        /// Identifies if type is COM visible
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if type is COM visible</returns>
        public static bool IsComVisible(this Type type) {
            // 1. 类型上显式标注
            var typeAtt = Attribute.GetCustomAttribute(type, typeof(ComVisibleAttribute), false)
                as ComVisibleAttribute;
            if(typeAtt != null)
                return typeAtt.Value;

            // 2. 程序集上标注
            var asmAtt = Attribute.GetCustomAttribute(type.Assembly, typeof(ComVisibleAttribute), false)
                as ComVisibleAttribute;
            if(asmAtt != null)
                return asmAtt.Value;

            // 3. 默认值（CLR 默认是 true）
            return true;
        }

    }
}