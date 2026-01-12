/*********************************************************************
vPages
Copyright(C) 2018 www.xarial.net
Product URL: https://www.xarial.net/products/developers/vpages
License: https://github.com/xarial/vpages/blob/master/LICENSE
*********************************************************************/

using System;
using System.Linq;
using System.Diagnostics;
using VPages.Base;

namespace VPages.PageElements
{
    public delegate void ControlValueChangedDelegate<TVal>(Control<TVal> sender, TVal newValue);

    public abstract class Control<TVal> : IControl
    {
        protected abstract event ControlValueChangedDelegate<TVal> ValueChanged;

        private ControlObjectValueChangedDelegate m_ValueChangedHandler;

        event ControlObjectValueChangedDelegate IControl.ValueChanged
        {
            add
            {
                this.ValueChanged += OnValueChanged;
                m_ValueChangedHandler += value;
            }
            remove
            {
                this.ValueChanged -= OnValueChanged;
                m_ValueChangedHandler -= value;
            }
        }

        private void OnValueChanged(Control<TVal> sender, TVal newValue)
        {
            if (m_ValueChangedHandler != null)
            {
                m_ValueChangedHandler.Invoke(sender, newValue);
            }
            else
            {
                Debug.Assert(false, "Generic event handler and specific event handler should be synchronised");
            }
        }

        public void SetValue(object value) {
            if(value is TVal v) {
                SetSpecificValue(v);
                return;
            }

            try {
                var converted = (TVal)Convert.ChangeType(value, typeof(TVal));
                SetSpecificValue(converted);
            } catch(Exception ex) {
                throw new InvalidCastException(
                    $"Cannot convert value '{value}' ({value?.GetType().Name}) to {typeof(TVal).Name}.", ex);
            }
        }


        public int Id { get; private set; }

        protected Control(int id, object tag)
        {
            Id = id;
            Tag = tag;
        }

        public object Tag { get; private set; }

        protected abstract TVal GetSpecificValue();
        protected abstract void SetSpecificValue(TVal value);

        object IControl.GetValue()
        {
            return GetSpecificValue();
        }

        public virtual void Dispose()
        {
        }
    }
}
