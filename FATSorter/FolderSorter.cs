using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Globalization;
using System.Security;

namespace UnpluggedSoft.FatSorter
{
	class FolderSorter : Component
	{
		/* Private variables */
		private String path;
		private bool recursive;
		private String baseName = @"\FAT.Sorter.Temp.Folder";
		private System.Diagnostics.Stopwatch timer;

		/* Constructors */
		public FolderSorter()
		{
		}

		public FolderSorter(String inPath, bool inRecursive)
		{
			this.Path = inPath;
			this.Recursive = inRecursive;
		}

		/* Events */
		public event EventHandler<StatusUpdateEventArgs> StatusUpdate;
		public event EventHandler<ProgressUpdateEventArgs> ProgressUpdate;

		protected virtual void OnStatusUpdate(StatusUpdateEventArgs e)
		{
			if (StatusUpdate != null)
			{
				StatusUpdate(this, e);
			}
		}

		protected virtual void OnProgressUpdate(ProgressUpdateEventArgs e)
		{
			if (ProgressUpdate != null)
			{
				ProgressUpdate(this, e);
			}
		}

		/* Properties */
		public String Path
		{
			set
			{
				if (Directory.Exists(value))
				{
					this.path = value;
				}
				else
				{
					throw new DirectoryNotFoundException(value + " was not found.");
				}
			}
		}

		public bool Recursive
		{
			set
			{
				this.recursive = value;
			}
		}
		
		/* Functions */
		public void start()
		{
			Thread thread = new Thread(new ThreadStart(this.startThread));
			thread.Name = "FolderSortThread";
			thread.Priority = ThreadPriority.BelowNormal;
			thread.Start();
		}

		public void startThread()
		{
			timer = new Stopwatch();
			timer.Start();
			if (this.path == null || !Directory.Exists(this.path))
			{
				this.logStatus("Unable to sort the selected folder.  It does not exist.");
				return;
			}

			this.resetProgress();

			this.logStatus("Beginning sort for " + this.path + "...");

			// Count all files first
			SearchOption searchOption = SearchOption.TopDirectoryOnly;
			if (this.recursive)
			{
				searchOption = SearchOption.AllDirectories;
			}
			String[] allFiles = Directory.GetFiles(this.path, "*", searchOption);
			this.updateProgressTotal(allFiles.Length);

			this.process(this.path, this.recursive);

			this.updateProgress(true);
			timer.Stop();
			
			this.logStatus("Sorting complete.  Total time " + timer.Elapsed.TotalSeconds.ToString("F2", CultureInfo.CurrentCulture) + " seconds");
		}

		private void process(String inPath, bool inRecursive)
		{
			DirectoryInfo directory = new DirectoryInfo(inPath);

			DirectoryInfo[] directories = directory.GetDirectories();
			FileInfo[] files = directory.GetFiles();

//			this.updateProgressTotal(files.Length);

			// Process subdirectories if needed
			if (inRecursive)
			{
				foreach (DirectoryInfo indir in directories)
				{
					this.process(indir.FullName, inRecursive);
				}
			}
			this.logStatus("Sorting " + inPath + "...");

			// make temp directory
			int counter = 1;
			String tempdirName = this.baseName;
			while (Directory.Exists(inPath + tempdirName))
			{
				tempdirName = this.baseName + counter.ToString(CultureInfo.InvariantCulture);
			}
			DirectoryInfo tempDirectory = Directory.CreateDirectory(directory + tempdirName);

			// Process directories first.
			foreach (DirectoryInfo indir in directories)
			{
				try
				{
					indir.MoveTo(tempDirectory.FullName + "\\" + indir.Name);
				}
				catch (IOException e)
				{
					this.logStatus("Error: Failed to move " + indir.Name);
					Debug.WriteLine(e.Message);
				}
			}
			// Then files
			foreach (FileInfo infile in files)
			{
				try
				{
					infile.MoveTo(tempDirectory.FullName + "\\" + infile.Name);
				}
				catch (IOException e)
				{
					this.logStatus("Error: Failed to move " + infile.Name);
					Debug.WriteLine(e.Message);
				}
			}

			// Now move them back
			directories = tempDirectory.GetDirectories();
			files = tempDirectory.GetFiles();

			// TODO: Need to sort them here
			List<DirectoryInfo> directoryList = directories.ToList<DirectoryInfo>();
			List<FileInfo> fileList = files.ToList<FileInfo>();

			if (directoryList.Count > 1)
			{
				directoryList.Sort(DirectoryCompare);
			}
			if (fileList.Count > 1)
			{
				fileList.Sort(FileCompare);
			}

			foreach (DirectoryInfo indir in directoryList)
			{
				try
				{
					indir.MoveTo(inPath + "\\" + indir.Name);
				}
				catch (IOException e)
				{
					this.logStatus("Error: Failed to move back " + indir.Name);
					Debug.WriteLine(e.Message);
				}
			}
			foreach (FileInfo infile in fileList)
			{
				try
				{
					infile.MoveTo(inPath + "\\" + infile.Name);
				}
				catch (IOException e)
				{
					this.logStatus("Error: Failed to move back " + infile.Name);
					Debug.WriteLine(e.Message);
				}

				this.updateProgress();
			}

			// Pull the temp directory
			try
			{
				tempDirectory.Delete();
			}
			catch (Exception e)
			{
				if (e.GetType() == typeof(IOException))
				{
					this.logStatus("Error: Temporary directory not empty: " + tempDirectory.FullName);
				}
				else if (e.GetType() == typeof(DirectoryNotFoundException))
				{
					this.logStatus("Error: Temporary directory not found: " + tempDirectory.FullName);
				}
				else if (e.GetType() == typeof(SecurityException))
				{
					this.logStatus("Error: Insufficent permission to delete temporary directory: " + tempDirectory.FullName);
				}
				else
				{
					throw;
				}
			}
		}

		private void updateProgress()
		{
			this.updateProgress(false);
		}

		private void updateProgress(bool finished)
		{
			ProgressUpdateEventArgs args = null;
			if (finished)
			{
				args = new ProgressUpdateEventArgs(true);
			}
			else
			{
				args = new ProgressUpdateEventArgs();
			}

			OnProgressUpdate(args);
		}

		private void updateProgressTotal(int addCount)
		{
			ProgressUpdateEventArgs args = new ProgressUpdateEventArgs(addCount);
			OnProgressUpdate(args);
		}

		private void logStatus(String message)
		{
			StatusUpdateEventArgs args = new StatusUpdateEventArgs(message);
			OnStatusUpdate(args);
		}

		private void resetProgress()
		{
			StatusUpdateEventArgs args = new StatusUpdateEventArgs();
			OnStatusUpdate(args);
		}

		/* Comparison functions */
		private static int DirectoryCompare(DirectoryInfo x, DirectoryInfo y)
		{
			return string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
		}

		private static int FileCompare(FileInfo x, FileInfo y)
		{
			return string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
		}
	}
}
