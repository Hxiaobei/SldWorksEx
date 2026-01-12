/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using VPages.Base;

namespace VPages.Constructors
{
    public abstract class PageConstructor<TPage> : IPageConstructor<TPage>
        where TPage : IPage
    {
        TPage IPageConstructor<TPage>.Create(IAttributeSet atts)
        {
            return Create(atts);
        }

        protected abstract TPage Create(IAttributeSet atts);
    }
}
