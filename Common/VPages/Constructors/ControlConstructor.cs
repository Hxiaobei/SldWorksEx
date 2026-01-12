/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using VPages.Base;

namespace VPages.Constructors
{
    public abstract class ControlConstructor<TControl, TGroup, TPage> : PageElementConstructor<TControl, TGroup, TPage>
        where TControl : IControl
        where TPage : IPage
        where TGroup : IGroup
    {
    }
}
