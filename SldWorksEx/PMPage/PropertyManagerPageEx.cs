using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.Common.Base;
using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.PMPage.Attributes;
using CodeStack.SwEx.PMPage.Base;
using CodeStack.SwEx.PMPage.Controls;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VPages.Base;
using CodeStack.SwEx.Common.Diagnostics;

namespace CodeStack.SwEx.PMPage {
    [ModuleInfo("SwEx.PMPage")]
    public class PropertyManagerPageEx<THandler, TModel> :
        IPropertyManagerPageEx<THandler, TModel>, IDisposable, ISwLog
        where THandler : PropertyManagerPageHandlerEx, new() {
        private readonly IconsConverter _iconsConv = new IconsConverter();
        private readonly ILogger _logger;
        private readonly THandler _handler = new THandler();
        private readonly ISldWorks _app;

        private PropertyManagerPageBuilder<THandler> _builder;
        private PropertyManagerPagePageEx<THandler> _activePage;

        public TModel Model { get; private set; }
        public THandler Handler => _handler;
        public IEnumerable<IPropertyManagerPageControlEx> Controls { get; private set; }
        public ILogger Logger => _logger;

        public PropertyManagerPageEx(ISldWorks app) : this(app, null) {
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public PropertyManagerPageEx(ISldWorks app, IPageSpec pageSpec) {
            _app = app;
            _logger = LoggerFactory.Create(this);

            _builder = new PropertyManagerPageBuilder<THandler>(
                app, _iconsConv, _handler, pageSpec, _logger);
        }

        public void Show(TModel model) {
            _logger.Log("Opening page");

            DisposeActivePage();
            _app.IActiveDoc2.ClearSelection2(true);

            _activePage = _builder.CreatePage(model);

            Controls = _activePage.Binding.Bindings
                .Select(b => b.Control)
                .OfType<IPropertyManagerPageControlEx>()
                .ToArray();

            _activePage.Page.Show2(0);
            _activePage.Binding.Dependency.UpdateAll();
        }

        private void DisposeActivePage() {
            if(_activePage == null)
                return;

            foreach(var ctrl in _activePage.Binding.Bindings
                .Select(b => b.Control)
                .OfType<IDisposable>()) {
                ctrl.Dispose();
            }

            _activePage = null;
        }

        public void Dispose() {
            _logger.Log("Disposing page");
            DisposeActivePage();
            _iconsConv.Dispose();
        }
    }
}
