/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System;
using System.Linq;

namespace VPages.Exceptions
{
    public class InvalidParentControlException : Exception
    {
        internal InvalidParentControlException(Type parentType, params Type[] supportedParents)
            : base($"{parentType.FullName} is not supported as a parent control. Only {string.Join(", ", supportedParents.Select(t => t.FullName).ToArray())} are supported")
        {
        }
    }
}
