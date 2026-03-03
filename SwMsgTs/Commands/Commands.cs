using System.ComponentModel;
using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwMsgTs.Properties;

namespace CodeStack.SwMsgTs.Commands {

    [Title("SwMsgTs Commands")]
    [Description("Sample commands")]
    [Icon(typeof(Resources), nameof(Resources.command_group_icon))]
    [CommandGroupInfo(0)]
    enum Commands_e {
        [Title("Command One")]
        [Description("Sample Command 1")]
        [Icon(typeof(Resources), nameof(Resources.command1_icon))]
        Command1,

        [Title("Command Two")]
        [Description("Sample Command2")]
        [CommandIcon(typeof(Resources), nameof(Resources.command2_icon), nameof(Resources.command2_icon))]
        [CommandItemInfo(true, true, swWorkspaceTypes_e.All, true)]
        Command2,

        [CommandSpacer]
        [CommandItemInfo(true, true, swWorkspaceTypes_e.AllDocuments, true)]
        Command3,

        [CommandItemInfo(true, true, swWorkspaceTypes_e.AllDocuments, true)]
        Command4,

        [CommandSpacer]
        [Title("Feat Demo")]
        [Description("Parameters Macro Feature")]
        [CommandItemInfo(true, true, swWorkspaceTypes_e.Part, true)]
        ParamsMacroFeature,

        [Title("Cube Demo")]
        [Description("Parametric Cube Macro Feature")]
        [CommandItemInfo(true, true, swWorkspaceTypes_e.Part, true)]
        CubeMacroFeature,
    }

    [CommandGroupInfo(typeof(Commands_e))]
    [Title("Sub Menu Commands")]
    enum SubCommands_e {
        [CommandItemInfo(true, true, swWorkspaceTypes_e.AllDocuments, true)]
        SubCommand1,

        [CommandItemInfo(true, true, swWorkspaceTypes_e.AllDocuments, true)]
        SubCommand2
    }

    enum TaskPaneCommands_e {
        [Title("Task Pane Command 1")]
        [Icon(typeof(Resources), nameof(Resources.command1_icon))]
        Command1,

        Command2
    }
}
