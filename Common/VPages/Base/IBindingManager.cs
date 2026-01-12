/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System.Collections.Generic;

namespace VPages.Base
{
    public interface IBindingManager
    {
        IEnumerable<IBinding> Bindings { get; }
        IDependencyManager Dependency { get; }

        void Load(IEnumerable<IBinding> bindings, IRawDependencyGroup dependencies);
    }
}
