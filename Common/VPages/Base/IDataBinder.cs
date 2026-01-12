/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System;
using System.Collections.Generic;

namespace VPages.Base
{
    public delegate IControl CreateBindingControlDelegate(Type dataType, IAttributeSet atts, IGroup parent, out int idRange);
    public delegate IPage CreateBindingPageDelegate(IAttributeSet atts);

    public interface IDataModelBinder
    {
        void Bind<TDataModel>(TDataModel model, CreateBindingPageDelegate pageCreator,
            CreateBindingControlDelegate ctrlCreator,
            out IEnumerable<IBinding> bindings, out IRawDependencyGroup dependencies);
    }
}
