/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System;

namespace VPages.Exceptions
{
    public class ConstructorNotFoundException : Exception
    {
        internal ConstructorNotFoundException(Type type, string message = "") 
            : base($"Constructor for type {type.FullName} is not found. {message}")
        {
        }
    }
}
