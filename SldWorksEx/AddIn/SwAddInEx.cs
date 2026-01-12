//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Delegates;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Exceptions;
using CodeStack.SwEx.AddIn.Helpers;
using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.Common.Base;
using CodeStack.SwEx.Common.Diagnostics;
using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.Common.Reflection;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CodeStack.SwEx.AddIn {
    /// <inheritdoc/>
    [ComVisible(true)]
    public abstract class SwAddInEx : ISwAddin, ISwAddInEx {
        #region Registration

        private static RegistrationHelper m_RegHelper;

        private static RegistrationHelper GetRegistrationHelper(Type moduleType)
            => m_RegHelper ?? (m_RegHelper = new RegistrationHelper(LoggerFactory.Create(moduleType)));

        private static void RegisterOrUnregister(Type t, bool register) {
            if(t.TryGetAttribute<AutoRegisterAttribute>() is object) {
                var helper = GetRegistrationHelper(t);
                if(register) helper.Register(t);
                else helper.Unregister(t);
            }
        }
        /// <summary>
        /// COM Registration entry function
        /// </summary>
        /// <param name="t">Type</param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type t) => RegisterOrUnregister(t, true);

        /// <summary>
        /// COM Unregistration entry function
        /// </summary>
        /// <param name="t">Type</param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t) => RegisterOrUnregister(t, false);

        #endregion

        private class TabCommandInfo {
            internal swDocumentTypes_e DocType { get; private set; }
            internal int CmdId { get; private set; }
            internal swCommandTabButtonTextDisplay_e TextType { get; private set; }

            internal TabCommandInfo(swDocumentTypes_e docType, int cmdId,
                swCommandTabButtonTextDisplay_e textType) {
                DocType = docType;
                CmdId = cmdId;
                TextType = textType;
            }
        }

        private const string SUB_GROUP_SEPARATOR = "\\";

        /// <summary>
        /// Pointer to SOLIDWORKS application
        /// </summary>
        protected ISldWorks App { get; private set; }

        /// <summary>
        /// Pointer to command group which holding the add-in commands
        /// </summary>
        protected ICommandManager CmdMgr { get; private set; }

        /// <summary>
        /// Add-ins cookie (id)
        /// </summary>
        protected int AddInCookie { get; private set; }

        public ILogger Logger { get; }

        private readonly Dictionary<ICommandGroupSpec, CommandGroup> m_CommandGroups;
        private readonly Dictionary<string, ICommandSpec> m_Commands;
        private readonly List<ITaskPaneHandler> m_TaskPanes;
        private readonly List<IDisposable> m_DocsHandlers;

        public SwAddInEx() {
            Logger = LoggerFactory.Create(this);
            m_Commands = new Dictionary<string, ICommandSpec>();
            m_CommandGroups = new Dictionary<ICommandGroupSpec, CommandGroup>();
            m_TaskPanes = new List<ITaskPaneHandler>();
            m_DocsHandlers = new List<IDisposable>();
        }

        /// <summary>SOLIDWORKS add-in entry function</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ConnectToSW(object ThisSW, int cookie) {
            Logger.Log("Loading add-in");
            try {
                App = (ISldWorks)ThisSW;
                AddInCookie = cookie;

                App.SetAddinCallbackInfo(0, this, AddInCookie);
                CmdMgr = App.GetCommandManager(AddInCookie);

                return OnConnect();
            } catch(Exception ex) {
                Logger.Log(ex);
                throw;
            }
        }

        /// <summary>
        /// Command click callback
        /// </summary>
        /// <param name="cmdId">Command tag</param>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnCommandClick(string cmdId) {
            Logger.Log($"Command clicked: {cmdId}");

            if(m_Commands.TryGetValue(cmdId, out ICommandSpec cmd))
                cmd.OnClick();
            else
                Debug.Assert(false, "All callbacks must be registered");
        }

        /// <summary>
        /// Command enable callback
        /// </summary>
        /// <param name="cmdId">Command tag</param>
        /// <returns>State</returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int OnCommandEnable(string cmdId) {

            if(m_Commands.TryGetValue(cmdId, out ICommandSpec cmd)) {
                return (int)cmd.OnEnable();
            } else {
                Debug.Assert(false, "All callbacks must be registered");
            }

            return (int)CommandItemEnableState_e.DeselectDisable;
        }

        /// <summary>
        /// SOLIDWORKS unload add-in callback
        /// </summary>
        /// <returns></returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DisconnectFromSW() {
            Logger.Log("Unloading add-in");

            try {
                foreach(var grp in m_CommandGroups.Keys) {
                    Logger.Log($"Removing group: {grp.Id}");
                    CmdMgr.RemoveCommandGroup(grp.Id);
                }

                m_CommandGroups.Clear();

                m_TaskPanes.ForEach(tp => tp.Delete());
                m_TaskPanes.Clear();

                m_DocsHandlers.ForEach(d => d.Dispose());
                m_DocsHandlers.Clear();

                var res = OnDisconnect();

                if(Marshal.IsComObject(CmdMgr))
                    Marshal.ReleaseComObject(CmdMgr);

                if(Marshal.IsComObject(App))
                    Marshal.ReleaseComObject(App);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return res;
            } catch(Exception ex) {
                Logger.Log(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual bool OnConnect() => true;

        /// <inheritdoc/>
        public virtual bool OnDisconnect() => true;


        /// <inheritdoc/>
        /// <exception cref="GroupIdAlreadyExistsException"/>
        /// <exception cref="InvalidMenuToolbarOptionsException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="CallbackNotSpecifiedException"/>
        public CommandGroup AddCommandGroup<TCmdEnum>(Action<TCmdEnum> callback,
            EnableMethodDelegate<TCmdEnum> enable = null)
            where TCmdEnum : IComparable, IFormattable, IConvertible
            => AddCommandGroup(new EnumCommandGroupSpec<TCmdEnum>(App, callback, enable, GetNextAvailableGroupId(), m_CommandGroups.Keys));


        /// <inheritdoc/>
        /// <exception cref="GroupIdAlreadyExistsException"/>
        /// <exception cref="InvalidMenuToolbarOptionsException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="CallbackNotSpecifiedException"/>
        public CommandGroup AddContextMenu<TCmdEnum>(Action<TCmdEnum> callback,
            swSelectType_e contextMenuSelectType = swSelectType_e.swSelEVERYTHING,
            EnableMethodDelegate<TCmdEnum> enable = null)
            where TCmdEnum : IComparable, IFormattable, IConvertible
            => AddContextMenu(
                new EnumCommandGroupSpec<TCmdEnum>(App, callback, enable, GetNextAvailableGroupId(), m_CommandGroups.Keys),
                contextMenuSelectType);


        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public CommandGroup AddCommandGroup(ICommandGroupSpec cmdBar)
            => AddCommandGroupOrContextMenu(cmdBar, false, 0);


        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public CommandGroup AddContextMenu(ICommandGroupSpec cmdBar,
            swSelectType_e contextMenuSelectType = swSelectType_e.swSelEVERYTHING)
            => AddCommandGroupOrContextMenu(cmdBar, true, contextMenuSelectType);


        /// <inheritdoc/>
        public IDocumentsHandler<TDocHandler> CreateDocumentsHandler<TDocHandler>()
            where TDocHandler : IDocumentHandler, new() {
            var docsHandler = new DocumentsHandler<TDocHandler>(App, Logger);

            m_DocsHandlers.Add(docsHandler);

            return docsHandler;
        }

        /// <inheritdoc/>
        public IDocumentsHandler<DocumentHandler> CreateDocumentsHandler() {
            var docsHandler = new DocumentsHandler<DocumentHandler>(App, Logger);

            m_DocsHandlers.Add(docsHandler);

            return docsHandler;
        }

        /// <inheritdoc/>
        public ITaskpaneView CreateTaskPane<TControl>(out TControl ctrl)
            where TControl : UserControl, new()
            => CreateTaskPane<TControl, EmptyTaskPaneCommands_e>(null, out ctrl);


        /// <inheritdoc/>
        public ITaskpaneView CreateTaskPane<TControl, TCmdEnum>(Action<TCmdEnum> cmdHandler, out TControl ctrl)
            where TControl : UserControl, new()
            where TCmdEnum : IComparable, IFormattable, IConvertible {
            CommandGroupIcon taskPaneIcon = null;
            string tooltip = "";

            void GetTaskPaneDisplayData(Type t, bool isControl) {
                if(taskPaneIcon == null)
                    taskPaneIcon = DisplayInfoExtractor.ExtractCommandDisplayIcon<TaskPaneIconAttribute,
                        CommandGroupIcon>(t, i => new TaskPaneMasterIcon(i), a => a.Icon, isControl);

                if(string.IsNullOrEmpty(tooltip)) {
                    if(t.TryGetAttribute<DisplayNameAttribute>(out var dn))
                        tooltip = dn.DisplayName;
                    else if(t.TryGetAttribute<DescriptionAttribute>(out var da))
                        tooltip = da.Description;
                }
            }

            if(typeof(TCmdEnum) != typeof(EmptyTaskPaneCommands_e))
                GetTaskPaneDisplayData(typeof(TCmdEnum), false);

            GetTaskPaneDisplayData(typeof(TControl), true);

            Logger.Log($"Creating task pane for {typeof(TControl).FullName} type");

            ITaskpaneView taskPaneView;
            ITaskPaneHandler taskPaneHandler;

            using(var iconConv = new IconsConverter()) {
                taskPaneView = App.SupportsHighResIcons(SldWorksExtension.HighResIconsScope_e.TaskPane)
                    ? App.CreateTaskpaneView3(iconConv.ConvertIcon(taskPaneIcon, true), tooltip)
                    : App.CreateTaskpaneView2(iconConv.ConvertIcon(taskPaneIcon, false)[0], tooltip);

                taskPaneHandler = new TaskPaneHandler<TCmdEnum>(App, taskPaneView, cmdHandler, iconConv, Logger);
            }

            if(typeof(TControl).IsComVisible()) {
                var progId = typeof(TControl).GetProgId();
                ctrl = taskPaneView.AddControl(progId, "") as TControl
                    ?? throw new NullReferenceException($"Failed to create COM control from {progId}. Ensure COM registration.");
            } else {
                ctrl = new TControl();
                ctrl.CreateControl();
                var handle = ctrl.Handle;

                if(!taskPaneView.DisplayWindowFromHandle(handle.ToInt32()))
                    throw new NullReferenceException($"Failed to host .NET control (handle {handle}) in task pane");
            }

            taskPaneHandler.Disposed += OnTaskPaneHandlerDisposed;
            m_TaskPanes.Add(taskPaneHandler);

            return taskPaneView;
        }

        private void OnTaskPaneHandlerDisposed(ITaskPaneHandler handler) {
            handler.Disposed -= OnTaskPaneHandlerDisposed;
            m_TaskPanes.Remove(handler);
        }

        private int GetNextAvailableGroupId()
            => m_CommandGroups.Any() ? m_CommandGroups.Keys.Max(g => g.Id) + 1 : 0;

        private CommandGroup AddCommandGroupOrContextMenu(ICommandGroupSpec cmdBar,
            bool isContextMenu, swSelectType_e contextMenuSelectType) {
            Logger.Log($"Creating command group: {cmdBar.Id}");

            if(m_CommandGroups.Keys.FirstOrDefault(g => g.Id == cmdBar.Id) != null) {
                throw new GroupIdAlreadyExistsException(cmdBar);
            }

            var title = GetMenuPath(cmdBar);

            var cmdGroup = CreateCommandGroup(cmdBar.Id, title, cmdBar.Tooltip,
                cmdBar.Commands.Select(c => c.UserId).ToArray(), isContextMenu,
                contextMenuSelectType);

            m_CommandGroups.Add(cmdBar, cmdGroup);

            using(var iconsConv = new IconsConverter()) {
                CreateIcons(cmdGroup, cmdBar, iconsConv);

                var createdCmds = CreateCommandItems(cmdGroup, cmdBar.Id, cmdBar.Commands);

                var tabGroup = GetRootCommandGroup(cmdBar);

                try {
                    CreateCommandTabBox(tabGroup, createdCmds);
                } catch(Exception ex) {
                    Logger.Log(ex);
                    //not critical error - continue operation
                }
            }

            return cmdGroup;
        }

        private CommandGroup GetRootCommandGroup(ICommandGroupSpec cmdBar) {
            var root = cmdBar;

            while(root.Parent != null)
                root = root.Parent;

            return m_CommandGroups[root];
        }

        private string GetMenuPath(ICommandGroupSpec cmdBar) {
            var title = new StringBuilder();

            var parent = cmdBar.Parent;

            while(parent != null) {
                title.Insert(0, parent.Title + SUB_GROUP_SEPARATOR);
                parent = parent.Parent;
            }

            title.Append(cmdBar.Title);

            return title.ToString();
        }

        private CommandGroup CreateCommandGroup(int groupId, string title, string toolTip,
            int[] knownCmdIDs, bool isContextMenu, swSelectType_e contextMenuSelectType) {
            int cmdGroupErr = 0;

            var isChanged = true;

            if(CmdMgr.GetGroupDataFromRegistry(groupId, out object registryIDs)) {
                isChanged = !CompareIDs(registryIDs as int[], knownCmdIDs);
            }

            Logger.Log($"Command ids changed: {isChanged}");

            CommandGroup cmdGroup;

            if(isContextMenu) {
                cmdGroup = CmdMgr.AddContextMenu(groupId, title);
                cmdGroup.SelectType = (int)contextMenuSelectType;
            } else {
                cmdGroup = CmdMgr.CreateCommandGroup2(groupId, title, toolTip,
                    toolTip, -1, isChanged, ref cmdGroupErr);

                Logger.Log($"Command group creation result: {(swCreateCommandGroupErrors)cmdGroupErr}");

                Debug.Assert(cmdGroupErr == (int)swCreateCommandGroupErrors.swCreateCommandGroup_Success);
            }

            return cmdGroup;
        }

        private void CreateIcons(CommandGroup cmdGroup, ICommandGroupSpec cmdBar, IIconsConverter iconsConv) {
            var mainIcon = cmdBar.Icon;

            CommandGroupIcon[] iconList = null;

            if(cmdBar.Commands != null) {
                iconList = cmdBar.Commands.Select(c => c.Icon).ToArray();
            }

            //NOTE: if commands are not used, main icon will fail if toolbar commands image list is not specified, so it is required to specify it explicitly

            if(App.SupportsHighResIcons(SldWorksExtension.HighResIconsScope_e.CommandManager)) {
                var iconsList = iconsConv.ConvertIcon(mainIcon, true);
                cmdGroup.MainIconList = iconsList;

                if(iconList?.Any() == true) {
                    cmdGroup.IconList = iconsConv.ConvertIconsGroup(iconList, true);
                } else {
                    cmdGroup.IconList = iconsList;
                }
            } else {
                var mainIconPath = iconsConv.ConvertIcon(mainIcon, false);

                var smallIcon = mainIconPath[0];
                var largeIcon = mainIconPath[1];

                cmdGroup.SmallMainIcon = smallIcon;
                cmdGroup.LargeMainIcon = largeIcon;

                if(iconList?.Any() == true) {
                    var iconListPath = iconsConv.ConvertIconsGroup(iconList, true);
                    var smallIconList = iconListPath[0];
                    var largeIconList = iconListPath[1];

                    cmdGroup.SmallIconList = smallIconList;
                    cmdGroup.LargeIconList = largeIconList;
                } else {
                    cmdGroup.SmallIconList = smallIcon;
                    cmdGroup.LargeIconList = largeIcon;
                }
            }
        }

        private Dictionary<ICommandSpec, int> CreateCommandItems(CommandGroup cmdGroup, int groupId, ICommandSpec[] cmds) {
            var callbackMethodName = nameof(OnCommandClick);
            var enableMethodName = nameof(OnCommandEnable);

            swCommandItemType_e GetMenuToolbarOpts(ICommandSpec cmd) {
                swCommandItemType_e opts = 0;
                if(cmd.HasMenu) opts |= swCommandItemType_e.swMenuItem;
                if(cmd.HasToolbar) opts |= swCommandItemType_e.swToolbarItem;
                if(opts == 0) throw new InvalidMenuToolbarOptionsException(cmd);
                return opts;
            }

            var createdCmds = new Dictionary<ICommandSpec, int>();

            foreach(var (cmd, i) in cmds.Select((c, idx) => (c, idx))) {
                var menuToolbarOpts = GetMenuToolbarOpts(cmd);
                var cmdName = $"{groupId}.{cmd.UserId}";

                m_Commands.Add(cmdName, cmd);

                var callbackFunc = $"{callbackMethodName}({cmdName})";
                var enableFunc = $"{enableMethodName}({cmdName})";

                if(cmd.HasSpacer)
                    cmdGroup.AddSpacer2(-1, (int)menuToolbarOpts);

                var cmdIndex = cmdGroup.AddCommandItem2(
                    cmd.Title, -1, cmd.Tooltip,
                    cmd.Title, i, callbackFunc, enableFunc, cmd.UserId,
                    (int)menuToolbarOpts);

                createdCmds[cmd] = cmdIndex;

                Logger.Log($"Created command {cmdIndex} for {cmd}");
            }

            cmdGroup.HasToolbar = cmdGroup.HasMenu = true;
            cmdGroup.Activate();

            return createdCmds.ToDictionary(p => p.Key, p => cmdGroup.CommandID[p.Value]);
        }

        private void CreateCommandTabBox(CommandGroup cmdGroup, Dictionary<ICommandSpec, int> commands) {
            Logger.Log("Creating command tab box");

            // 构造所有 TabCommandInfo
            var tabCommands = commands
                .Where(c => c.Key.HasTabBox)
                .SelectMany(c => {
                    var (cmd, cmdId) = (c.Key, c.Value);

                    IEnumerable<swDocumentTypes_e> GetDocTypes() {
                        if(cmd.SupportedWorkspace.HasFlag(swWorkspaceTypes_e.Part))
                            yield return swDocumentTypes_e.swDocPART;
                        if(cmd.SupportedWorkspace.HasFlag(swWorkspaceTypes_e.Assembly))
                            yield return swDocumentTypes_e.swDocASSEMBLY;
                        if(cmd.SupportedWorkspace.HasFlag(swWorkspaceTypes_e.Drawing))
                            yield return swDocumentTypes_e.swDocDRAWING;
                    }

                    return GetDocTypes().Select(t => new TabCommandInfo(t, cmdId, cmd.TabBoxStyle));
                }).ToList();

            foreach(var cmdGrp in tabCommands.GroupBy(c => c.DocType)) {
                var docType = cmdGrp.Key;
                var cmdTab = CmdMgr.GetCommandTab((int)docType, cmdGroup.Name)
                           ?? CmdMgr.AddCommandTab((int)docType, cmdGroup.Name)
                           ?? throw new NullReferenceException("Failed to create command tab box");

                var cmdIds = cmdGrp.Select(c => c.CmdId).ToArray();
                var txtTypes = cmdGrp.Select(c => (int)c.TextType).ToArray();

                var cmdBox = TryFindCommandTabBox(cmdTab, cmdIds);

                if(cmdBox == null) {
                    cmdBox = cmdTab.AddCommandTabBox();
                } else if(IsCommandTabBoxChanged(cmdBox, cmdIds, txtTypes)) {
                    ClearCommandTabBox(cmdBox);
                } else {
                    continue;
                }

                if(!cmdBox.AddCommands(cmdIds, txtTypes))
                    throw new InvalidOperationException("Failed to add commands to commands tab box");
            }
        }

        private CommandTabBox TryFindCommandTabBox(ICommandTab cmdTab, int[] cmdIds) {

            var cmdBoxes = cmdTab.CommandTabBoxes().ConvertSw<CommandTabBox>();
            var cmdBoxGroup = cmdBoxes.GroupBy(b => {
                b.GetCommands(out var existingCmds, out var existingTextStyles);
                return (existingCmds is int[] v) ? v.Intersect(cmdIds).Count() : 0;
            }).OrderByDescending(g => g.Key).FirstOrDefault();

            if(cmdBoxGroup?.Key > 0) return cmdBoxGroup.FirstOrDefault();

            return null;
        }

        private bool IsCommandTabBoxChanged(ICommandTabBox cmdBox, int[] cmdIds, int[] txtTypes) {
            cmdBox.GetCommands(out var existingCmds, out object existingTextStyles);

            if(existingCmds != null && existingTextStyles != null) {
                return !(existingCmds as int[]).SequenceEqual(cmdIds)
                    || !(existingTextStyles as int[]).SequenceEqual(txtTypes);
            }

            return true;
        }

        private void ClearCommandTabBox(ICommandTabBox cmdBox) {
            cmdBox.GetCommands(out var existingCmds, out _);
            if(existingCmds != null)
                cmdBox.RemoveCommands(existingCmds as int[]);
        }

        private bool CompareIDs(IEnumerable<int> storedIDs, IEnumerable<int> addinIDs)
            => new HashSet<int>(storedIDs).SetEquals(new HashSet<int>(addinIDs));
    }
}
