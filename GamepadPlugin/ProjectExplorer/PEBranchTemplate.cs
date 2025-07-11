﻿using GamepadPlugin;
using Lumos.GUI;
using Lumos.GUI.Connection;
using Lumos.GUI.Facade.GUISession;
using Lumos.GUI.GuiActions;
using Lumos.GUI.Windows.ProjectExplorer;
using LumosLIB.Tools.I18n;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LumosGUIPluginTemplates.ProjectExplorer
{
    class PEBranchTemplate : AbstractExplorerBranch
    {
        public override int OrderIndex => 70;

        public PEBranchTemplate()
            : base("Gamepads", T._("Gamepads"))
        {
            GamepadManager.Instance.GetAllControllers().ToList();
        }

        public override ENodeSorting AllowedNodeSorting => ENodeSorting.BY_DISPLAYNAME;

        public override string ImageKey => KnownIcons.SHUFFLE;

        public override IEnumerable<TextBlock> Description
        {
            get
            {
                yield return new TextBlock()
                {
                    Text = T._("The \"{0}\" branch contains connected Controllers (Gamepads)", this.DisplayName)
                };
            }
        }

        public override async Task connectionClosing()
        {
            await base.connectionClosing();
        }

        public override async Task connectionEstablished()
        {
            await base.connectionEstablished();
        }

        public override async Task loadProjectFromKernel()
        {
            if (ConnectionManager.getInstance().Connected)
            {
                ISessionFacade s = ConnectionManager.getInstance().GuiSession;
                RefreshControllersList();
            }
        }

        protected override async Task<IEnumerable<string>> GetDetailInfoInternal(IProjectExplorerNode node)
        {
            if (node is PENodeTemplate n)
            {
                return new string[]
                {
                    n.ID,
                    T._(n.Color)
                };
            }
            return await base.GetDetailInfoInternal(node);
        }

        protected override IEnumerable<string> DetailHeadlinesInternal
        {
            get
            {
                return new string[] { T._("ID"), T._("Color") };
            }
        }

        public override bool getEditLableOnAdd(IProjectExplorerNode node)
        {
            return true;
        }

        public override ReadOnlyCollection<ActionItemMetadata> MenuItems
        {
            get
            {
                List<ActionItemMetadata> tmp = new List<ActionItemMetadata>()
                {
                    new ActionItemMetadata("ActCreatePETemplate", this.ID, T._("Refresh List"), KnownIcons.POSITION + "_add", EActionItemDisplayType.TOOL_STRIP | EActionItemDisplayType.CONTEXT_MENU, 1, "0_InputAssignmentBranch", null, RefreshControllersListM)
                };
                return tmp.AsReadOnly();
            }
        }

        private void RefreshControllersListM(object sender, ActionItemMetadata meta, EMouseButtons buttons, bool down)
        {
            RefreshControllersList();
        }

        private void RefreshControllersList()
        {
            base.ClearSubNodes();
            foreach (var controller in GamepadManager.Instance.GetAllControllers())
            {
                base.AddSubNode(new PENodeTemplate(controller) { DisplayName = T._("Gamepad {0}", controller.ControllerIndex) });
            }
            // base.AddSubNode(new PENodeTemplate());
        }

        private List<ActionItemMetadata> menuItems = new List<ActionItemMetadata>();

        public override bool CanDrop(params IProjectExplorerNode[] nodes)
        {
            return base.CanDrop<PENodeTemplate>(nodes);
        }
    }
}
