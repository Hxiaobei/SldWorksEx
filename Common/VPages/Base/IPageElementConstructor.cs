/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

namespace VPages.Base
{
    public interface IPageElementConstructor<TGroup, TPage>
        where TGroup : IGroup
        where TPage : IPage
    {
        IControl Create(TPage page, IAttributeSet atts, ref int idRange);
        IControl Create(TGroup group, IAttributeSet atts, ref int idRange);
    }
}
