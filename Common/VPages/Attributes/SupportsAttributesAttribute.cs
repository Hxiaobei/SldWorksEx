/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System;
using VPages.Base;

namespace VPages.Attributes
{
    public class SupportsAttributesAttribute : Attribute, IAttribute
    {
        public Type[] Types { get; private set; }

        public SupportsAttributesAttribute(params Type[] types)
        {
            Types = types;
        }
    }
}
