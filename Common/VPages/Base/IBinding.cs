/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System;

namespace VPages.Base
{
    public interface IBinding
    {
        event Action<IBinding> ModelUpdated;
        event Action<IBinding> ControlUpdated;

        IControl Control { get; }
        object Model { get; }

        void UpdateControl();
        void UpdateDataModel();
    }
}
