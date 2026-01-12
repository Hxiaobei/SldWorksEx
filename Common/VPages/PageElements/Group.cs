/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using VPages.Base;

namespace VPages.PageElements
{
    public abstract class Group<TVal> : Control<TVal>, IGroup
    {
#pragma warning disable CS0067
        protected override event ControlValueChangedDelegate<TVal> ValueChanged;
#pragma warning restore CS0067

        protected Group(int id, object tag) : base(id, tag)
        {
        }

        protected override TVal GetSpecificValue()
        {
            return default(TVal);
        }

        protected override void SetSpecificValue(TVal value)
        {
        }
    }

    public abstract class Group : Group<object>
    {
        protected Group(int id, object tag) : base(id, tag)
        {
        }
    }
}
