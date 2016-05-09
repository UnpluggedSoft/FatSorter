using System;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Globalization;

namespace UnpluggedSoft.FatSorter
{
	/// <summary>
	/// Interaction logic for AboutApp.xaml
	/// </summary>
	public partial class AboutApp : Window
	{
		private System.Reflection.Assembly theApp;

		public AboutApp()
		{
			InitializeComponent();

			theApp = System.Reflection.Assembly.GetCallingAssembly();

			Bitmap __bitmap = Properties.Resources.LogoWatermark;
			BitmapSource __source = Imaging.CreateBitmapSourceFromHBitmap(__bitmap.GetHbitmap(), IntPtr.Zero, new Int32Rect(0, 0, __bitmap.Width, __bitmap.Height), BitmapSizeOptions.FromWidthAndHeight(__bitmap.Width, __bitmap.Height));
			imageLogo.Source = __source;

			// TODO: Any way to set the icon here... easily?

			// Show App Info
			showInfo();
		}

		private void showInfo()
		{
			labelAssemblyName.Content = AssemblyProduct + " " + AssemblyVersion + "\n";
			labelAssemblyCompany.Content += AssemblyCompany + "\n";
			textBoxDescription.Text += AssemblyDescription + "\n";
			labelAssemblyCopyright.Content += AssemblyCopyright + "\n";
		}

		#region Assembly Attribute Accessors

		private string AssemblyVersion
		{
			get
			{
				return theApp.GetName().Version.ToString();
			}
		}

/*
 * private string AssemblyTitle
		{
			get
			{
				object[] attributes = theApp.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (attributes.Length > 0)
				{
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
					if (String.IsNullOrEmpty(titleAttribute.Title))
					{
						return titleAttribute.Title;
					}
				}
				return System.IO.Path.GetFileNameWithoutExtension(theApp.CodeBase);
			}
		}
*/
		private string AssemblyDescription
		{
			get
			{
				object[] attributes = theApp.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		private string AssemblyProduct
		{
			get
			{
				object[] attributes = theApp.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		private string AssemblyCopyright
		{
			get
			{
				object[] attributes = theApp.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		private string AssemblyCompany
		{
			get
			{
				object[] attributes = theApp.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}
		#endregion
	}
}
