using CodeStack.SwEx.AddIn.Core;
using SolidWorks.Interop.sldworks;

namespace CodeStack.SwMsgTs.Documents {
    class DocumentEventBinder {
        private readonly ISldWorks _app;
        private readonly DocumentEventHandlers _handlers;

        public DocumentEventBinder(ISldWorks app) {
            _app = app;
            _handlers = new DocumentEventHandlers(app);
        }

        public void Bind(DocumentHandler handler) {
            handler.Activated += _handlers.OnActivated;
            handler.Initialized += _handlers.OnInitialized;
            handler.ConfigurationChange += _handlers.OnConfigurationChanged;
            handler.CustomPropertyModify += _handlers.OnCustomPropertyModified;
            handler.ItemModify += _handlers.OnItemModified;
            handler.Save += _handlers.OnSave;
            handler.Selection += _handlers.OnSelection;
            handler.Rebuild += _handlers.OnRebuild;
            handler.DimensionChange += _handlers.OnDimensionChange;
            handler.Destroyed += OnDestroyed;
        }

        private void OnDestroyed(DocumentHandler handler) {
            handler.Activated -= _handlers.OnActivated;
            handler.Initialized -= _handlers.OnInitialized;
            handler.ConfigurationChange -= _handlers.OnConfigurationChanged;
            handler.CustomPropertyModify -= _handlers.OnCustomPropertyModified;
            handler.ItemModify -= _handlers.OnItemModified;
            handler.Save -= _handlers.OnSave;
            handler.Selection -= _handlers.OnSelection;
            handler.Rebuild -= _handlers.OnRebuild;
            handler.DimensionChange -= _handlers.OnDimensionChange;
            handler.Destroyed -= OnDestroyed;

            _handlers.OnDestroyed(handler);
        }
    }
}
