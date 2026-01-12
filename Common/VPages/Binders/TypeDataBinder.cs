using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using VPages.Attributes;
using VPages.Base;
using VPages.Base.Attributes;
using VPages.Core;

namespace VPages.Binders {
    public class TypeDataBinder : IDataModelBinder {
        public void Bind<TDataModel>(
            TDataModel model,
            CreateBindingPageDelegate pageCreator,
            CreateBindingControlDelegate ctrlCreator,
            out IEnumerable<IBinding> bindings,
            out IRawDependencyGroup dependencies) {
            if(model == null) throw new ArgumentNullException(nameof(model));
            if(pageCreator == null) throw new ArgumentNullException(nameof(pageCreator));
            if(ctrlCreator == null) throw new ArgumentNullException(nameof(ctrlCreator));

            var modelType = model.GetType();

            var bindingsList = new List<IBinding>();
            bindings = bindingsList;

            var pageAttSet = GetAttributeSet(modelType, -1);
            OnGetPageAttributeSet(modelType, ref pageAttSet);

            var page = pageCreator(pageAttSet);

            dependencies = new RawDependencyGroup();

            var nextCtrlId = 0;

            TraverseType(
                modelType,
                model,
                parents: Array.Empty<PropertyInfo>(),
                ctrlCreator: ctrlCreator,
                parentCtrl: page,
                bindings: bindingsList,
                dependencies: dependencies,
                nextCtrlId: ref nextCtrlId);

            OnBeforeControlsDataLoad(bindings);
            LoadControlsData(bindings);
        }

        protected virtual void OnGetPageAttributeSet(Type pageType, ref IAttributeSet attSet) {
        }

        protected virtual void OnBeforeControlsDataLoad(IEnumerable<IBinding> bindings) {
        }

        private static void LoadControlsData(IEnumerable<IBinding> bindings) {
            foreach(var binding in bindings)
                binding.UpdateControl();
        }

        private void TraverseType<TDataModel>(
            Type type,
            TDataModel model,
            IReadOnlyList<PropertyInfo> parents,
            CreateBindingControlDelegate ctrlCreator,
            IGroup parentCtrl,
            IList<IBinding> bindings,
            IRawDependencyGroup dependencies,
            ref int nextCtrlId) {
            foreach(var prp in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                if(!prp.CanRead)
                    continue;

                var atts = GetAttributeSet(prp, nextCtrlId);

                if(atts.Has<IIgnoreBindingAttribute>())
                    continue;

                var prpType = prp.PropertyType;

                var ctrl = CreateControl(prpType, atts, parentCtrl, ctrlCreator, ref nextCtrlId);

                var binding = new PropertyInfoBinding<TDataModel>(model, ctrl, prp, parents.ToList());
                bindings.Add(binding);

                RegisterTagDependency(atts, binding, dependencies);
                RegisterBindingDependencies(atts, binding, dependencies);

                if(ctrl is IGroup groupCtrl) {
                    var grpParents = new List<PropertyInfo>(parents) { prp };
                    TraverseType(prpType, model, grpParents, ctrlCreator, groupCtrl, bindings, dependencies, ref nextCtrlId);
                }
            }
        }

        private static IControl CreateControl(
            Type boundType,
            IAttributeSet atts,
            IGroup parentCtrl,
            CreateBindingControlDelegate ctrlCreator,
            ref int nextCtrlId) {
            var ctrl = ctrlCreator(boundType, atts, parentCtrl, out var idRange);
            nextCtrlId += idRange;
            return ctrl;
        }

        private static void RegisterTagDependency(
            IAttributeSet atts,
            IBinding binding,
            IRawDependencyGroup dependencies) {
            if(!atts.Has<IControlTagAttribute>())
                return;

            var tag = atts.Get<IControlTagAttribute>().Tag;
            if(tag != null)
                dependencies.RegisterBindingTag(binding, tag);
        }

        private static void RegisterBindingDependencies(
            IAttributeSet atts,
            IBinding binding,
            IRawDependencyGroup dependencies) {
            if(!atts.Has<IDependentOnAttribute>())
                return;

            var depAtt = atts.Get<IDependentOnAttribute>();
            if(depAtt.Dependencies != null && depAtt.Dependencies.Any())
                dependencies.RegisterDependency(binding, depAtt.Dependencies, depAtt.DependencyHandler);
        }

        private IAttributeSet GetAttributeSet(PropertyInfo prp, int ctrlId) {
            var type = prp.PropertyType;

            var typeAtts = ParseAttributes(type.GetCustomAttributes(true), out var typeName, out var typeDesc, out var typeTag);
            var prpAtts = ParseAttributes(prp.GetCustomAttributes(true), out var prpName, out var prpDesc, out var prpTag);

            var name = !string.IsNullOrEmpty(prpName) ? prpName : (!string.IsNullOrEmpty(typeName) ? typeName : prp.Name);
            var desc = !string.IsNullOrEmpty(prpDesc) ? prpDesc : typeDesc;
            var tag = prpTag ?? typeTag;

            var mergedAtts = prpAtts.Concat(typeAtts).ToArray();

            return CreateAttributeSet(ctrlId, name, desc, type, mergedAtts, tag, prp);
        }

        private IAttributeSet GetAttributeSet(Type type, int ctrlId) {
            var typeAtts = ParseAttributes(type.GetCustomAttributes(true), out var name, out var desc, out var tag);

            if(string.IsNullOrEmpty(name))
                name = type.Name;

            return CreateAttributeSet(ctrlId, name, desc, type, typeAtts.ToArray(), tag);
        }

        private static IEnumerable<IAttribute> ParseAttributes(
            object[] customAtts,
            out string name,
            out string desc,
            out object tag) {
            if(customAtts == null || customAtts.Length == 0) {
                name = null;
                desc = null;
                tag = null;
                return Enumerable.Empty<IAttribute>();
            }

            name = customAtts.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName;
            desc = customAtts.OfType<DescriptionAttribute>().FirstOrDefault()?.Description;
            tag = customAtts.OfType<ControlTagAttribute>().FirstOrDefault()?.Tag;

            return customAtts.OfType<IAttribute>();
        }

        private static IAttributeSet CreateAttributeSet(
            int ctrlId,
            string ctrlName,
            string desc,
            Type boundType,
            IAttribute[] atts,
            object tag,
            MemberInfo boundMemberInfo = null) {
            var attsSet = new AttributeSet(ctrlId, ctrlName, desc, boundType, tag, boundMemberInfo);

            if(atts != null && atts.Length > 0) {
                foreach(var att in atts)
                    attsSet.Add(att);
            }

            return attsSet;
        }
    }
}
