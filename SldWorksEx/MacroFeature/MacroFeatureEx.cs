using System;
using System.ComponentModel;
using System.Drawing;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.Common.Base;
using CodeStack.SwEx.Common.Diagnostics;
using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.Common.Reflection;
using CodeStack.SwEx.MacroFeature.Attributes;
using CodeStack.SwEx.MacroFeature.Base;
using CodeStack.SwEx.MacroFeature.Icons;
using CodeStack.SwEx.Properties;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;

namespace CodeStack.SwEx.MacroFeature {
    [ModuleInfo("SwEx.MacroFeature")]
    public abstract class MacroFeatureEx : ISwComFeature, IModule {
        private readonly string m_Provider;
        public ILogger Logger { get; }

        protected MacroFeatureEx() {
            // Provider
            if(GetType().TryGetAttribute<OptionsAttribute>(out var opt))
                m_Provider = opt.Provider;

            Logger = LoggerFactory.Create(this);
            TryCreateIcons();
        }

        private void TryCreateIcons() {
            var iconsConverter = new IconsConverter(MacroFeatureIconInfo.GetLocation(this.GetType()), false);

            MacroFeatureIcon regIcon = null;
            MacroFeatureIcon highIcon = null;
            MacroFeatureIcon suppIcon = null;

            if(this.GetType().TryGetAttribute<FeatureIconAttribute>(out var featIco)) {
                regIcon = featIco.Regular;
                highIcon = featIco.Highlighted;
                suppIcon = featIco.Suppressed;
            }

            if(regIcon == null) {
                Image icon = null;

                if(this.GetType().TryGetAttribute<Common.Attributes.IconAttribute>(out var ico)) {
                    icon = ico.Icon;
                }
                if(icon == null) icon = Resources.default_icon;

                regIcon = new MasterIcon(MacroFeatureIconInfo.RegularName) { Icon = icon };
            }
            if(highIcon == null) highIcon = regIcon.Clone(MacroFeatureIconInfo.HighlightedName);
            if(suppIcon == null) suppIcon = regIcon.Clone(MacroFeatureIconInfo.SuppressedName);


            //Creation of icons may fail if user doesn't have write permissions or icon is locked
            try {
                iconsConverter.ConvertIcon(regIcon, true);
                iconsConverter.ConvertIcon(suppIcon, true);
                iconsConverter.ConvertIcon(highIcon, true);
                iconsConverter.ConvertIcon(regIcon, false);
                iconsConverter.ConvertIcon(suppIcon, false);
                iconsConverter.ConvertIcon(highIcon, false);
            } catch(Exception ex) {
                Logger.Log(ex);
            }
        }

        private void SetProvider(ISldWorks app, IFeature feat) {
            if(string.IsNullOrEmpty(m_Provider))
                return;

            if(!app.IsVersionNewerOrEqual(16))
                return;

            if(feat.GetDefinition() is IMacroFeatureData data
                && data.Provider != m_Provider)
                data.Provider = m_Provider;
        }

        // ---------------------------
        // ISwComFeature 接口实现
        // ---------------------------

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Edit(object app, object model, object feature)
            => OnEditDefinition(app as ISldWorks, model as IModelDoc2, feature as IFeature);


        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Regenerate(object app, object model, object feature) {
            var swApp = app as ISldWorks;
            var swModel = model as IModelDoc2;
            var swFeat = feature as IFeature;

            SetProvider(swApp, swFeat);

            var res = OnRebuild(swApp, swModel, swFeat);
            return res?.GetResult();
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Security(object app, object model, object feature)
            => OnUpdateState(app as ISldWorks, model as IModelDoc2, feature as IFeature);


        // ---------------------------
        // 可重写的核心回调
        // ---------------------------

        protected virtual bool OnEditDefinition(ISldWorks app, IModelDoc2 model, IFeature feature)
            => true;

        protected virtual MacroFeatureRebuildResult OnRebuild(ISldWorks app, IModelDoc2 model, IFeature feature)
            => null;

        protected virtual swMacroFeatureSecurityOptions_e OnUpdateState(ISldWorks app, IModelDoc2 model, IFeature feature)
            => swMacroFeatureSecurityOptions_e.swMacroFeatureSecurityByDefault;
    }
}
