﻿using DevExpress.ReportServer.ServiceModel.ConnectionProviders;
using DevExpress.ReportServer.ServiceModel.DataContracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScheduledTasksAPIClientDemo {
    public partial class MainForm : Form {
        // Specifies the Report Server address.
        public const string ServerAddress = "http://localhost:83";

        // Specifies the connection provider for accessing the Report Server.
        // The Guest connection provider is used by default.
        // To be able to run the application, make sure that the Guest account
        // is activated in the Report Server administrative panel.
        readonly ConnectionProvider serverConnection = new WindowsUserConnectionProvider(ServerAddress);

        public MainForm() {
            InitializeComponent();
        }

        // The following method obtains the list of all scheduled jobs from the server 
        // and displays this list in a grid control.
        void MainForm_Load(object sender, EventArgs e) {
            splashScreenManager1.ShowWaitForm();

            serverConnection.DoWithScheduledJobAsync(x => x.GetScheduledJobsAsync(null))
                .ContinueWith(taskFunc => {
                    splashScreenManager1.CloseWaitForm();
                    if (taskFunc.IsFaulted) {
                        MessageBox.Show(taskFunc.Exception.Flatten().InnerException.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else {
                        FillScheduledJobListBox(taskFunc.Result);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void FillScheduledJobListBox(IEnumerable<ScheduledJobCatalogItemDto> scheduledJobs) {
            scheduledJobsGrid.DataSource = scheduledJobs;
            scheduledJobsView.BestFitColumns();
        }

        // The following method displays the Scheduler Task Viewer form
        // that enables you to inspect and manage a selected task.
        void showScheduledJobButton_Click(object sender, EventArgs e) {
            var selectedId = scheduledJobsView.GetFocusedRowCellValue("Id") as int?;
            if (selectedId.HasValue) {
                var form = new SchedulerJobViewerForm(selectedId.Value, serverConnection) { Owner = this };
                form.ShowDialog();
            }
        }

        // The following method displays the Scheduler Job Results form 
        // that lists all documents generated by a selected task 
        // and enables you to view a specific document.
        private void showScheduledJobResultsButton_Click(object sender, EventArgs e) {
            var selectedId = scheduledJobsView.GetFocusedRowCellValue("Id") as int?;
            var selectedName = scheduledJobsView.GetFocusedRowCellValue("Name") as string;
            if (selectedId.HasValue) {
                var form = new SchedulerJobResultsForm(selectedId.Value, selectedName, serverConnection) { Owner = this };
                form.ShowDialog();
            }
        }

        private void scheduledJobsView_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e) {
            showScheduledJobButton.Enabled = scheduledJobsView.SelectedRowsCount > 0;
            showScheduledJobResultsButton.Enabled = scheduledJobsView.SelectedRowsCount > 0;
        }

    }
}