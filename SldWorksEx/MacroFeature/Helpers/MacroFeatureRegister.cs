//**********************
//SwEx.MacroFeature - framework for developing macro features in SOLIDWORKS
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-macrofeature/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/macro-feature
//**********************

using CodeStack.SwEx.Common.Base;
using CodeStack.SwEx.Common.Diagnostics;
using CodeStack.SwEx.MacroFeature.Base;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CodeStack.SwEx.MacroFeature.Helpers {
    internal class MacroFeatureRegister<THandler> : IDisposable
        where THandler : class, IMacroFeatureHandler, new() {
        private class ModelDictionary : Dictionary<IModelDoc2, MacroFeatureDictionary> {
        }

        private class MacroFeatureDictionary : Dictionary<IFeature, THandler> {
        }

        private readonly ModelDictionary m_Register;
        private readonly Dictionary<IModelDoc2, MacroFeatureLifecycleManager> m_LifecycleManagers;
        private readonly string m_BaseName;
        private readonly ILogger m_Logger;

        internal MacroFeatureRegister(string baseName, IModule parentModule) {
            m_BaseName = baseName;
            m_Logger = LoggerFactory.Create(parentModule, this.GetType().Name);

            m_Register = new ModelDictionary();
            m_LifecycleManagers = new Dictionary<IModelDoc2, MacroFeatureLifecycleManager>();
        }

        internal THandler EnsureFeatureRegistered(ISldWorks app, IModelDoc2 model, IFeature feat, out bool isNew) {
            isNew = false;


            if(!m_Register.TryGetValue(model, out MacroFeatureDictionary featsDict)) {
                m_Logger.Log($"{model?.GetTitle()} model is not registered in the register");

                featsDict = new MacroFeatureDictionary();
                m_Register.Add(model, featsDict);

                var lcm = new MacroFeatureLifecycleManager(model, m_BaseName, m_Logger);
                lcm.ModelDisposed += OnModelDisposed;
                lcm.FeatureDeleted += OnFeatureDeleted;
                m_LifecycleManagers.Add(model, lcm);
            }

            if(!featsDict.TryGetValue(feat, out THandler handler)) {
                m_Logger.Log($"{feat?.Name} feature in {model?.GetTitle()} model is not registered in the register");

                handler = new THandler();
                featsDict.Add(feat, handler);
                handler.Init(app, model, feat);
                isNew = true;
            }

            return handler;
        }

        private void OnFeatureDeleted(IModelDoc2 model, IFeature feat) {
            UnloadFeatureFromRegister(model, feat, MacroFeatureUnloadReason_e.Deleted);
        }

        private void OnModelDisposed(IModelDoc2 model) {
            UnloadModelFromRegister(model);
        }

        private void UnloadModelFromRegister(IModelDoc2 model) {

            if(m_LifecycleManagers.TryGetValue(model, out MacroFeatureLifecycleManager lcm)) {
                lcm.ModelDisposed -= OnModelDisposed;
                lcm.FeatureDeleted -= OnFeatureDeleted;

                m_LifecycleManagers.Remove(model);
            } else {
                Debug.Assert(false, "Model is not registered");
            }


            if(m_Register.TryGetValue(model, out MacroFeatureDictionary modelDict)) {
                foreach(var handler in modelDict.Values) {
                    handler.Unload(MacroFeatureUnloadReason_e.ModelClosed);
                }

                m_Register.Remove(model);
            } else {
                Debug.Assert(false, "Model is not registered");
            }
        }

        private void UnloadFeatureFromRegister(IModelDoc2 model, IFeature feat, MacroFeatureUnloadReason_e reason) {

            if(m_Register.TryGetValue(model, out MacroFeatureDictionary modelDict)) {

                if(modelDict.TryGetValue(feat, out THandler handler)) {
                    handler.Unload(reason);

                    modelDict.Remove(feat);
                } else {
                    Debug.Assert(false, "Handler is not registered");
                }
            } else {
                Debug.Assert(false, "Model is not registered");
            }
        }

        public void Dispose() {
            foreach(var model in m_Register.Keys) {
                UnloadModelFromRegister(model);
            }

            m_Register.Clear();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
