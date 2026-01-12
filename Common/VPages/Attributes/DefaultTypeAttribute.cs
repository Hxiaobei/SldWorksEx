/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System;
using VPages.Base.Attributes;

namespace VPages.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DefaultTypeAttribute : Attribute, IDefaultTypeAttribute
    {
        public Type Type { get; private set; }
        
        public DefaultTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
