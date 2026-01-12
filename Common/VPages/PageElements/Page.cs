/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using VPages.Base;
using VPages.Core;

namespace VPages.PageElements
{
    public abstract class Page : Group, IPage
    {
        private IBindingManager m_Binding;

        public Page() : base(-1, null)
        {
        }

        public IBindingManager Binding
        {
            get
            {
                return m_Binding ?? (m_Binding = new BindingManager());
            }
        }
    }
}
