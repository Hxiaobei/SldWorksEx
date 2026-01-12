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
    public class ControlTagAttribute : Attribute, IControlTagAttribute
    {
        /// <summary>
        /// Tag associated with the control
        /// </summary>
        public object Tag { get; private set; }

        public ControlTagAttribute(object tag)
        {
            Tag = tag;
        }
    }
}
