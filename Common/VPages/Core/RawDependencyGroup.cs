/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System;
using System.Collections.Generic;
using VPages.Base;

namespace VPages.Core
{
    public class RawDependencyGroup : IRawDependencyGroup
    {
        public Dictionary<object, IBinding> TaggedBindings { get; private set; }
        public Dictionary<IBinding, Tuple<object[], Type>> DependenciesTags { get; private set; }

        public RawDependencyGroup()
        {
            TaggedBindings = new Dictionary<object, IBinding>();
            DependenciesTags = new Dictionary<IBinding, Tuple<object[], Type>>();
        }

        public void RegisterBindingTag(IBinding binding, object tag)
        {
            if (!TaggedBindings.ContainsKey(tag))
            {
                TaggedBindings.Add(tag, binding);
            }
            else
            {
                throw new Exception("Tag is not unique");
            }
        }

        public void RegisterDependency(IBinding binding, object[] dependentOnTags, Type dependencyHandlerType)
        {
            DependenciesTags.Add(binding, new Tuple<object[], Type>(dependentOnTags, dependencyHandlerType));
        }
    }
}
