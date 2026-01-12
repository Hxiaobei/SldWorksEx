using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using VPages.Base;
using VPages.Core;

namespace VPages.Binders {
    public sealed class PropertyInfoBinding<TDataModel> : Binding<TDataModel>, IDisposable {

        private readonly IReadOnlyList<PropertyInfo> _parents;
        private readonly PropertyInfo _property;
        private INotifyPropertyChanged _notifier;

        public PropertyInfoBinding(
            TDataModel dataModel,
            IControl control,
            PropertyInfo property,
            IReadOnlyList<PropertyInfo> parents)
            : base(control, dataModel) {
            _property = property ?? throw new ArgumentNullException(nameof(property));
            _parents = parents ?? Array.Empty<PropertyInfo>();

            ValidateProperty(_property);
            ValidateParentChain(_parents);

            _notifier = GetCurrentModel(false) as INotifyPropertyChanged;
            if(_notifier != null)
                _notifier.PropertyChanged += OnPropertyChanged;
        }

        public void Dispose() {
            if(_notifier != null)
                _notifier.PropertyChanged -= OnPropertyChanged;

            _notifier = null;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == _property.Name)
                SetUserControlValue();
        }

        protected override void SetDataModelValue() {
            var model = GetCurrentModel(false);
            if(model == null)
                return;

            var rawValue = Control.GetValue();
            var currentValue = _property.GetValue(model);

            var converted = ConvertToTargetType(rawValue, _property.PropertyType);

            if(!Equals(currentValue, converted))
                _property.SetValue(model, converted);
        }

        protected override void SetUserControlValue() {
            var model = GetCurrentModel(false);
            if(model == null)
                return;

            var modelValue = _property.GetValue(model);
            var controlValue = Control.GetValue();

            if(!Equals(modelValue, controlValue))
                Control.SetValue(modelValue);
        }

        private object GetCurrentModel(bool createIfMissing) {
            object current = DataModel;

            foreach(var parent in _parents) {
                var next = parent.GetValue(current);

                if(next == null) {
                    if(!createIfMissing)
                        return null;

                    next = Activator.CreateInstance(parent.PropertyType);
                    parent.SetValue(current, next);
                }

                current = next;
            }

            return current;
        }

        private static object ConvertToTargetType(object value, Type targetType) {
            if(value == null)
                return null;

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if(underlying.IsInstanceOfType(value))
                return value;

            try {
                return Convert.ChangeType(value, underlying);
            } catch(Exception ex) {
                throw new InvalidCastException(
                    $"Cannot convert '{value}' ({value.GetType().Name}) to {targetType.Name}.", ex);
            }
        }

        private static void ValidateProperty(PropertyInfo property) {
            if(!property.CanRead || !property.CanWrite)
                throw new InvalidOperationException($"Property '{property.Name}' must be readable and writable.");

            if(property.GetIndexParameters().Length > 0)
                throw new NotSupportedException("Indexed properties are not supported.");
        }

        private static void ValidateParentChain(IReadOnlyList<PropertyInfo> parents) {
            foreach(var p in parents) {
                if(p.PropertyType.IsValueType)
                    throw new NotSupportedException("Value-type parent chain is not supported.");

                if(!p.CanRead || !p.CanWrite)
                    throw new InvalidOperationException($"Parent property '{p.Name}' must be readable and writable.");
            }
        }
    }
}
