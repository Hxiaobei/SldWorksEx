/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace VPages.Base
{
    public interface IAttributeSet
    {
        int Id { get; }
        object Tag { get; }
        string Name { get; }
        string Description { get; }
        Type BoundType { get; }
        MemberInfo BoundMemberInfo { get; }

        bool Has<TAtt>() where TAtt : IAttribute;

        TAtt Get<TAtt>() where TAtt : IAttribute;

        IEnumerable<TAtt> GetAll<TAtt>() where TAtt : IAttribute;

        void Add<TAtt>(TAtt att) where TAtt : IAttribute;
    }
}
