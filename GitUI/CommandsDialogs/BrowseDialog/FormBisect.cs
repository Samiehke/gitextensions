﻿using GitCommands.Git;
using GitCommands.Git.Commands;
using GitUI.CommandDialogs;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed partial class FormBisect : GitModuleForm
    {
        // TODO: Improve me
        private readonly TranslationString _bisectStart =
            new("Mark selected revisions as start bisect range?");

        private readonly IRevisionGridInfo _revisionGridInfo;

        public FormBisect(RevisionGridControl revisionGrid)
            : base(revisionGrid.UICommands)
        {
            _revisionGridInfo = revisionGrid;
            InitializeComponent();
            InitializeComplete();
            UpdateButtonsState();
        }

        private void UpdateButtonsState()
        {
            bool inTheMiddleOfBisect = Module.InTheMiddleOfBisect();
            Start.Enabled = !inTheMiddleOfBisect;
            Good.Enabled = inTheMiddleOfBisect;
            Bad.Enabled = inTheMiddleOfBisect;
            Stop.Enabled = inTheMiddleOfBisect;
            btnSkip.Enabled = inTheMiddleOfBisect;
        }

        private void Start_Click(object sender, EventArgs e)
        {
            FormProcess.ShowDialog(this, UICommands, arguments: GitCommandHelpers.StartBisectCmd(), Module.WorkingDir, input: null, useDialogSettings: true);

            UpdateButtonsState();

            var revisions = _revisionGridInfo.GetSelectedRevisions();
            if (revisions.Count > 1)
            {
                if (MessageBox.Show(this, _bisectStart.Text, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    BisectRange(revisions[0].ObjectId, revisions[^1].ObjectId);
                    Close();
                }
            }

            return;

            void BisectRange(ObjectId startRevision, ObjectId endRevision)
            {
                var command = GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Good, startRevision);
                bool success = FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
                if (!success)
                {
                    return;
                }

                command = GitCommandHelpers.ContinueBisectCmd(GitBisectOption.Bad, endRevision);
                FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
            }
        }

        private void Good_Click(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Good);
        }

        private void Bad_Click(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Bad);
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            FormProcess.ShowDialog(this, UICommands, arguments: GitCommandHelpers.StopBisectCmd(), Module.WorkingDir, input: null, useDialogSettings: false);
            Close();
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Skip);
        }

        private void ContinueBisect(GitBisectOption bisectOption)
        {
            FormProcess.ShowDialog(this, UICommands, arguments: GitCommandHelpers.ContinueBisectCmd(bisectOption), Module.WorkingDir, input: null, useDialogSettings: false);
            Close();
        }
    }
}
