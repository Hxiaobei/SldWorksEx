/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System;
using System.Collections.Generic;

namespace VPages.Base
{
    public interface IRawDependencyGroup
    {
        Dictionary<object, IBinding> TaggedBindings { get; }
        Dictionary<IBinding, Tuple<object[], Type>> DependenciesTags { get; }

        void RegisterBindingTag(IBinding binding, object tag);
        void RegisterDependency(IBinding binding, object[] dependentOnTags, Type dependencyHandlerType);
    }
}
