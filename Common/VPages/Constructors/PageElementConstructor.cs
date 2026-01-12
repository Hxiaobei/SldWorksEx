/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using VPages.Base;

namespace VPages.Constructors
{
    public abstract class PageElementConstructor<TElem, TGroup, TPage> : IPageElementConstructor<TGroup, TPage>
            where TGroup : IGroup
            where TPage : IPage
            where TElem : IControl
    {
        protected abstract TElem Create(TPage page, IAttributeSet atts);
        protected abstract TElem Create(TGroup group, IAttributeSet atts);

        protected virtual TElem Create(TPage page, IAttributeSet atts, ref int idRange)
        {
            return Create(page, atts);
        }

        protected virtual TElem Create(TGroup group, IAttributeSet atts, ref int idRange)
        {
            return Create(group, atts);
        }

        IControl IPageElementConstructor<TGroup, TPage>.Create(TPage page, IAttributeSet atts, ref int idRange)
        {
            return Create(page, atts, ref idRange);
        }

        IControl IPageElementConstructor<TGroup, TPage>.Create(TGroup group, IAttributeSet atts, ref int idRange)
        {
            return Create(group, atts, ref idRange);
        }
    }
}
