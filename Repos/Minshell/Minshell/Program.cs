using System;
using System.IO;
using System.Windows.Forms;

namespace Minshell
{
	internal static class Program
	{
		[STAThread]
		static void Main()
		{
			var filesRoot = @"F:\Repos\Minshell\Minshell\Files";
			var tempPath = @"F:\Repos\Minshell\Minshell\Temps";
			Directory.CreateDirectory(filesRoot);
			Directory.CreateDirectory(tempPath);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm(filesRoot, tempPath));
		}
	}
}
