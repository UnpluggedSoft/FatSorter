using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Threading;

namespace FatSorter
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		public Window1()
		{
			InitializeComponent();
			this.loadSettings();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.saveSettings();
		}

		private void saveSettings()
		{
			Properties.Settings.Default.Recursive = this.checkBoxRecursive.IsChecked.GetValueOrDefault();
			Properties.Settings.Default.LastDirectory = this.textBoxDirectory.Text;
			Properties.Settings.Default.ShowDetails = this.expanderProgress.IsExpanded;
			Properties.Settings.Default.Save();
		}

		private void loadSettings()
		{
			Properties.Settings.Default.Upgrade();
			this.checkBoxRecursive.IsChecked = Properties.Settings.Default.Recursive;
			this.textBoxDirectory.Text = Properties.Settings.Default.LastDirectory;
			this.expanderProgress.IsExpanded = Properties.Settings.Default.ShowDetails;
		}

		private void browseForFolder()
		{
			String currentFolder = String.Empty;
			if (this.textBoxDirectory.Text.Length > 0)
			{
				currentFolder = this.textBoxDirectory.Text;
			}

			System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
			folderDialog.SelectedPath = currentFolder;
			folderDialog.Description = FolderDialogDescription;
			folderDialog.ShowNewFolderButton = false;
			System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				this.textBoxDirectory.Text = folderDialog.SelectedPath;
			}
		}

		private void sortFolder()
		{
			this.buttonStart.IsEnabled = false;
			String path = this.textBoxDirectory.Text;
			bool recursive = this.checkBoxRecursive.IsChecked.Value;

			try
			{
				FolderSorter sorter = new FolderSorter(path, recursive);
				sorter.StatusUpdate += new EventHandler<StatusUpdateEventArgs>(this.statusUpdateHandler);
				sorter.ProgressUpdate += new EventHandler<ProgressUpdateEventArgs>(this.progressUpdateHandler);

				sorter.start();
			}
			catch (DirectoryNotFoundException e)
			{
				MessageBox.Show(FolderDoesNotExist, FatSorterTitle, MessageBoxButton.OK, MessageBoxImage.Stop);
				Debug.WriteLine(e.Message);
				return;
			}
		}

		private void statusUpdateHandler(object sender, StatusUpdateEventArgs e)
		{
			Dispatcher.Invoke(DispatcherPriority.Normal, new RunStatusUpdate(doStatusUpdate), e);
		}

		private void progressUpdateHandler(object sender, ProgressUpdateEventArgs e)
		{
			Dispatcher.Invoke(DispatcherPriority.Normal, new RunProgressUpdate(doProgressUpdate), e);
		}

		private delegate void RunStatusUpdate(StatusUpdateEventArgs e);
		private delegate void RunProgressUpdate(ProgressUpdateEventArgs e);

		private void doStatusUpdate(StatusUpdateEventArgs e)
		{
			if (e.DoReset)
			{
				this.resetProgress();
			}
			else
			{
				this.logStatus(e.StatusText);
			}
		}

		private void doProgressUpdate(ProgressUpdateEventArgs e)
		{
			if (e.IncrementCounter)
			{
				this.updateProgress();

			}
			else if (e.IsFinished)
			{
				this.updateProgress(true);
			}
			else
			{
				this.updateProgressTotal(e.AddTotal);
			}
		}

		private void logStatus(String message)
		{
			// TODO: Figure out how to localize the status messages.
			// Probably will need to pass more information and use format string, rather
			// than try to set up more events.  But it can wait a bit.  Maybe 1.0.5
			this.textBlockProgress.Text += message + "\n";
			this.scrollViewerProgress.ScrollToBottom();
		}

		private void updateProgressTotal(int addCount)
		{
			this.progressSorting.Maximum += addCount;
		}

		private void updateProgress()
		{
			this.progressSorting.Value++;
		}

		private void updateProgress(bool finish)
		{
			if (finish)
			{
				//this.progressSorting.Value = this.progressSorting.Maximum;
				this.buttonStart.IsEnabled = true;
				MessageBox.Show(SortingCompleted, FatSorterTitle, MessageBoxButton.OK, MessageBoxImage.Information);
			}
			else
			{
				this.updateProgress();
			}
		}

		private void resetProgress()
		{
			this.textBlockProgress.Text = "";
			this.progressSorting.Maximum = 0;
			this.progressSorting.Value = 0;
		}

		#region UI Events
		private void buttonBrowse_Click(object sender, RoutedEventArgs e)
		{
			this.browseForFolder();
		}

		private void buttonStart_Click(object sender, RoutedEventArgs e)
		{
			this.sortFolder();
		}

		private void buttonClose_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void buttonAbout_Click(object sender, RoutedEventArgs e)
		{
			HolosTekUtility.AboutApp aboutDialog = new HolosTekUtility.AboutApp();
			aboutDialog.Show();
		}
		#endregion

		#region Localized Strings
		private static string FatSorterTitle
		{
			get
			{
				return (string)Application.Current.TryFindResource("FatSorterTitle");
			}
		}
		private static string FolderDialogDescription
		{
			get
			{
				return (string)Application.Current.TryFindResource("FolderDialogDescription");
			}
		}
		private static string FolderDoesNotExist
		{
			get
			{
				return (string)Application.Current.TryFindResource("FolderDoesNotExist");
			}
		}
		private static string SortingCompleted
		{
			get
			{
				return (string)Application.Current.TryFindResource("SortingCompleted");
			}
		}
		#endregion
	}
}
