/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System.Collections.Generic;
using VPages.Base;

namespace VPages.Core
{
    public class BindingManager : IBindingManager
    {
        public IEnumerable<IBinding> Bindings { get; private set; }
        public IDependencyManager Dependency { get; private set; }

        public void Load(IEnumerable<IBinding> bindings, IRawDependencyGroup dependencies)
        {
            Bindings = bindings;
            Dependency = new DependencyManager();
            Dependency.Init(dependencies);
        }
    }
}
