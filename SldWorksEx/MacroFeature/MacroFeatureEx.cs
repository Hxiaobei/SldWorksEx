using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.Common.Base;
using CodeStack.SwEx.Common.Diagnostics;
using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.Common.Reflection;
using CodeStack.SwEx.MacroFeature.Attributes;
using CodeStack.SwEx.MacroFeature.Base;
using CodeStack.SwEx.MacroFeature.Data;
using CodeStack.SwEx.MacroFeature.Helpers;
using CodeStack.SwEx.MacroFeature.Icons;
using CodeStack.SwEx.Properties;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace CodeStack.SwEx.MacroFeature
{
    [ModuleInfo("SwEx.MacroFeature")]
    public abstract class MacroFeatureEx : ISwComFeature, ISwLog
    {
        private readonly string m_Provider;
        public ILogger Logger { get; }

        protected MacroFeatureEx()
        {
            // Provider
            if (GetType().TryGetAttribute<OptionsAttribute>(out var opt))
                m_Provider = opt.Provider;

            Logger = LoggerFactory.Create(this);
            TryCreateIcons();
        }

        private void TryCreateIcons()
        {
            var iconsConverter = new IconsConverter(MacroFeatureIconInfo.GetLocation(this.GetType()), false);

            MacroFeatureIcon regIcon = null;
            MacroFeatureIcon highIcon = null;
            MacroFeatureIcon suppIcon = null;

            if (this.GetType().TryGetAttribute<FeatureIconAttribute>(out var featIco))
            {
                regIcon = featIco.Regular;
                highIcon = featIco.Highlighted;
                suppIcon = featIco.Suppressed;
            }

            if (regIcon == null)
            {
                Image icon = null;

                if (this.GetType().TryGetAttribute<Common.Attributes.IconAttribute>(out var ico))
                {
                    icon = ico.Icon;
                }
                if (icon == null) icon = Resources.default_icon;

                regIcon = new MasterIcon(MacroFeatureIconInfo.RegularName) { Icon = icon };
            }
            if (highIcon == null) highIcon = regIcon.Clone(MacroFeatureIconInfo.HighlightedName);
            if (suppIcon == null) suppIcon = regIcon.Clone(MacroFeatureIconInfo.SuppressedName);


            //Creation of icons may fail if user doesn't have write permissions or icon is locked
            try
            {
                iconsConverter.ConvertIcon(regIcon, true);
                iconsConverter.ConvertIcon(suppIcon, true);
                iconsConverter.ConvertIcon(highIcon, true);
                iconsConverter.ConvertIcon(regIcon, false);
                iconsConverter.ConvertIcon(suppIcon, false);
                iconsConverter.ConvertIcon(highIcon, false);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void SetProvider(ISldWorks app, IFeature feat)
        {
            if (string.IsNullOrEmpty(m_Provider)) return;
            if (!app.IsVersionNewerOrEqual(16)) return;
            if (feat.GetDefinition() is IMacroFeatureData data && data.Provider != m_Provider)
                data.Provider = m_Provider;
        }

        #region ISwComFeature 接口重新定义


        // ---------------------------
        // ISwComFeature 接口实现
        // ---------------------------

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Edit(object app, object model, object feature)
            => OnEditDefinition((ISldWorks)app, (IModelDoc2)model, (IFeature)feature);

        //TODO: regenerate method is called twice when feature edited and new parameters applied
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Regenerate(object app, object model, object feature)
        {
            var swApp = (ISldWorks)app;
            var swFeat = (IFeature)feature;

            SetProvider(swApp, swFeat);
            return OnRebuild(swApp, (IModelDoc2)model, swFeat)?.GetResult();
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Security(object app, object model, object feature)
             => OnUpdateState((ISldWorks)app, (IModelDoc2)model, (IFeature)feature);


        // ---------------------------
        // 可重写的核心回调
        // ---------------------------

        protected virtual bool OnEditDefinition(ISldWorks app, IModelDoc2 model, IFeature feature)
            => true;

        protected virtual RebuildResult OnRebuild(ISldWorks app, IModelDoc2 model, IFeature feature)
            => null;

        protected virtual swMacroFeatureSecurityOptions_e OnUpdateState(ISldWorks app, IModelDoc2 model, IFeature feature)
            => swMacroFeatureSecurityOptions_e.swMacroFeatureSecurityByDefault;
        #endregion
    }


    /// <inheritdoc cref="MacroFeatureEx"/>
    /// <summary>Represents macro feature which stores additional user parameters</summary>
    /// <typeparam name="TParams">Type of class representing parameters data model</typeparam>
    [ClassInterface(ClassInterfaceType.None)]
    [ComDefaultInterface(typeof(ISwComFeature))]
    public abstract class MacroFeatureEx<TParams> : MacroFeatureEx
        where TParams : class, new()
    {
        private readonly MacroFeatureParametersParser m_ParamsParser;

        /// <summary>
        /// Base constructor. Should be called from the derived class as it contains required initialization
        /// </summary>
        public MacroFeatureEx() { m_ParamsParser = new MacroFeatureParametersParser(this.GetType()); }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected sealed override RebuildResult OnRebuild(ISldWorks app, IModelDoc2 model, IFeature feature)
        {
            Logger.Log("Rebuilding. Getting parameters");

            var featDef = feature.GetDefinition() as IMacroFeatureData;

            var parameters = m_ParamsParser.GetParameters<TParams>(feature, featDef, model,
                out var dispDims, out var dispDimParams, out var editBodies, out var state);

            Logger.Log("Rebuilding. Generating bodies");

            var rebuildRes = OnRebuild(app, model, feature, parameters);

            Logger.Log("Rebuilding. Updating dimensions");

            UpdateDimensions(app, model, feature, rebuildRes, dispDims, dispDimParams, parameters);

            Logger.Log("Rebuilding. Releasing dimensions");

            if (dispDims != null)
            {
                for (int i = 0; i < dispDims.Length; i++)
                {
                    dispDims[i] = null;
                }
            }

            return rebuildRes;
        }

        /// <inheritdoc cref="MacroFeatureEx.OnRebuild(ISldWorks, IModelDoc2, IFeature)"/>
        /// <param name="parameters">Current instance of parameters of this macro feature</param>
        protected virtual RebuildResult OnRebuild(ISldWorks app, IModelDoc2 model, IFeature feature, TParams parameters)
            => null;


        /// <summary>
        /// Override this function to configure the dimensions of macro feature (i.e. position, style, etc.)
        /// </summary>
        /// <param name="app">Pointer to application</param>
        /// <param name="model">Pointer to current model</param>
        /// <param name="feature">Pointer to macro feature</param>
        /// <param name="dims">Pointer to dimensions of macro feature</param>
        /// <param name="parameters">Current instance of parameters (including the values of dimensions)</param>
        /// <remarks>Use the <see cref="DimensionDataExtension.SetOrientation(DimensionData, Vector3, Vector3)"/>
        /// helper method to set the dimension orientation and position based on its values</remarks>
        protected virtual void OnSetDimensions(ISldWorks app, IModelDoc2 model, IFeature feature,
            RebuildResult rebuildResult, DimensionDataCollection dims, TParams parameters)
        {
            OnSetDimensions(app, model, feature, dims, parameters);
        }

        /// <inheritdoc cref="OnSetDimensions(ISldWorks, IModelDoc2, IFeature, RebuildResult, DimensionDataCollection, TParams)"/>
        protected virtual void OnSetDimensions(ISldWorks app, IModelDoc2 model, IFeature feature,
            DimensionDataCollection dims, TParams parameters)
        { }

        /// <summary>
        /// Returns the current instance of parameters data model for the feature
        /// </summary>
        /// <param name="feat">Pointer to feature</param>
        /// <param name="featData">Pointer to feature data</param>
        /// <param name="model">Pointer to model</param>
        /// <returns>Current instance of parameters</returns>
        protected TParams GetParameters(IFeature feat, IMacroFeatureData featData, IModelDoc2 model)
        {
            IDisplayDimension[] dispDims;
            var parameters = GetParameters(feat, featData, model, out dispDims, out IBody2[] editBodies);

            return parameters;
        }

        /// <summary>
        /// Assigns the instance of data model to the macro feature parameters
        /// </summary>
        /// <param name="model">Pointer to model</param>
        /// <param name="feat">Pointer to feature</param>
        /// <param name="featData">Pointer to feature data</param>
        /// <param name="parameters">Parameters data model</param>
        /// <remarks>Call this method before calling the <see href="http://help.solidworks.com/2016/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.ifeature~modifydefinition.html">IFeature::ModifyDefinition</see></remarks>
        protected void SetParameters(IModelDoc2 model, IFeature feat, IMacroFeatureData featData, TParams parameters)
        {
            SetParameters(model, feat, featData, parameters, out OutdateState_e state);
        }

        /// <inheritdoc cref="SetParameters(IModelDoc2, IFeature, IMacroFeatureData, TParams)"/>
        /// <param name="state">Current state of the parameters</param>
        protected void SetParameters(IModelDoc2 model, IFeature feat, IMacroFeatureData featData, TParams parameters,
            out OutdateState_e state)
           => m_ParamsParser.SetParameters(model, feat, featData, parameters, out state);


        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected TParams GetParameters(IFeature feat, IMacroFeatureData featData, IModelDoc2 model,
            out IDisplayDimension[] dispDims, out IBody2[] editBodies)
            => m_ParamsParser.GetParameters<TParams>(feat, featData, model, out dispDims,
                out string[] dispDimParams, out editBodies, out OutdateState_e state);


        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected void ParseParameters(TParams parameters,
            out string[] paramNames, out int[] paramTypes,
            out string[] paramValues, out object[] selection,
            out int[] dimTypes, out double[] dimValues, out IBody2[] editBodies)
        {
            m_ParamsParser.Parse(parameters, out paramNames, out paramTypes,
                out paramValues, out selection, out dimTypes, out dimValues, out editBodies);
        }

        private void UpdateDimensions(ISldWorks app, IModelDoc2 model, IFeature feature, RebuildResult rebuildRes,
            IDisplayDimension[] dispDims, string[] dispDimParams, TParams parameters)
        {

            using (var dimsColl = new DimensionDataCollection(dispDims, dispDimParams))
                if (dimsColl.Any()) OnSetDimensions(app, model, feature, rebuildRes, dimsColl, parameters);

        }
    }


    /// <inheritdoc cref="MacroFeatureEx{TParams}"/>
    /// <summary>Represents macro feature which stores additional user parameters and provides per feature handler.
    /// This version of macro feature is useful where it is required to track the lifecycle of
    /// an individual feature as <see cref="MacroFeatureEx"/> behaves as a service and it creates
    /// one instance per application session</summary>
    /// <typeparam name="THandler">Handler of macro feature</typeparam>
    [ClassInterface(ClassInterfaceType.None)]
    [ComDefaultInterface(typeof(ISwComFeature))]
    public abstract class MacroFeatureEx<TParams, THandler> : MacroFeatureEx<TParams>
        where THandler : class, IMacroFeatureHandler, new()
        where TParams : class, new()
    {
        private MacroFeatureRegister<THandler> m_Register;

        public MacroFeatureEx() : base()
        {
            m_Register = new MacroFeatureRegister<THandler>(
                MacroFeatureInfo.GetBaseName(this.GetType()), this);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected override sealed bool OnEditDefinition(ISldWorks app, IModelDoc2 model, IFeature feature)
        {
            return OnEditDefinition(GetHandler(app, model, feature));
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected override sealed swMacroFeatureSecurityOptions_e OnUpdateState(ISldWorks app, IModelDoc2 model, IFeature feature)
        {
            return OnUpdateState(GetHandler(app, model, feature));
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected override sealed RebuildResult OnRebuild(ISldWorks app, IModelDoc2 model, IFeature feature, TParams parameters)
        {
            return OnRebuild(GetHandler(app, model, feature), parameters);
        }

        /// <inheritdoc cref="MacroFeatureEx.OnUpdateState(ISldWorks, IModelDoc2, IFeature)"/>
        /// <param name="handler">Pointer to the macro feature handler of the feature being updated</param>
        protected virtual swMacroFeatureSecurityOptions_e OnUpdateState(THandler handler)
        {
            return swMacroFeatureSecurityOptions_e.swMacroFeatureSecurityByDefault;
        }

        /// <inheritdoc cref="MacroFeatureEx{TParams}.OnRebuild(ISldWorks, IModelDoc2, IFeature, TParams)"/>
        /// <param name="handler">Pointer to the macro feature handler of the feature being rebuilt</param>
        protected virtual RebuildResult OnRebuild(THandler handler, TParams parameters)
        {
            return null;
        }

        /// <inheritdoc cref="MacroFeatureEx.OnEditDefinition(ISldWorks, IModelDoc2, IFeature)"/>
        /// <param name="handler">Pointer to the macro feature handler of the feature being edited</param>
        protected virtual bool OnEditDefinition(THandler handler)
        {
            return true;
        }

        private THandler GetHandler(ISldWorks app, IModelDoc2 model, IFeature feature)
        {
            var handler = m_Register.EnsureFeatureRegistered(app, model, feature, out bool isNew);

            Logger.Log($"Getting macro feature handler. New={isNew}");

            return handler;
        }
    }
}
