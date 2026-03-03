using System;
using VPages.Base;

namespace VPages.Core {
    public abstract class Binding<TDataModel> : IBinding, IDisposable{

        public event Action<IBinding> ModelUpdated;
        public event Action<IBinding> ControlUpdated;
        public IControl Control { get; }
        protected TDataModel DataModel { get; }

        private bool _disposed;

        object IBinding.Model => DataModel;

        protected Binding(IControl control, TDataModel dataModel) {
            Control = control ?? throw new ArgumentNullException(nameof(control));
            if(dataModel == null) throw new ArgumentNullException(nameof(dataModel));

            DataModel = dataModel;

            Control.ValueChanged += OnControlValueChanged;
        }


        private void OnControlValueChanged(IControl sender, object newValue) {
            if(_disposed)
                return;

            try {
                UpdateDataModel();
            } catch(Exception ex) {
                OnBindingError(ex, isModelUpdate: true);
            }
        }

        public void UpdateControl() {
            if(_disposed)
                return;

            try {
                SetUserControlValue();
                ControlUpdated?.Invoke(this);
            } catch(Exception ex) {
                OnBindingError(ex, isModelUpdate: false);
            }
        }

        public void UpdateDataModel() {
            if(_disposed)
                return;

            try {
                SetDataModelValue();
                ModelUpdated?.Invoke(this);
            } catch(Exception ex) {
                OnBindingError(ex, isModelUpdate: true);
            }
        }

        protected abstract void SetUserControlValue();
        protected abstract void SetDataModelValue();

        /// <summary>
        /// Allows derived classes or host frameworks to log or handle binding errors.
        /// </summary>
        protected virtual void OnBindingError(Exception ex, bool isModelUpdate) {
            // Default: swallow. Override to log.
        }

        public virtual void Dispose() {
            if(_disposed)
                return;

            _disposed = true;
            Control.ValueChanged -= OnControlValueChanged;
        }
    }
}
