/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

namespace VPages.Base.Attributes
{
    public interface IControlTagAttribute : IAttribute
    {
        object Tag { get; }
    }
}
