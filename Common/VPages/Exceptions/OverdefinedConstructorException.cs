/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System;

namespace VPages.Exceptions
{
    public class OverdefinedConstructorException : Exception
    {
        internal OverdefinedConstructorException(Type constrType, Type keyType)
            : base($"Constructor of type {constrType.FullName} is already registered for {keyType.FullName}")
        {
        }
    }
}
