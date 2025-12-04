using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Minshell
{
	public class MainForm : Form
	{
		TextBox txtAddress = new();
		ListView lvFiles = new() { View = View.Details, FullRowSelect = true };
		TextBox txtCmd = new();
		Button btnRun = new() { Text = "Run" };
		RichTextBox txtOut = new() { ReadOnly = true };
		ShellContext ctx = new();

		public MainForm(string filesRoot, string tempPath)
		{
			Text = "Minshell GUI (Dark Mode)";
			ClientSize = new Size(1000, 700);
			Font = new Font("Consolas", 10);

			// Dark mode colors
			BackColor = Color.FromArgb(30, 30, 30);
			ForeColor = Color.White;
			txtAddress.BackColor = Color.FromArgb(45, 45, 48);
			txtAddress.ForeColor = Color.White;
			txtCmd.BackColor = Color.FromArgb(45, 45, 48);
			txtCmd.ForeColor = Color.White;
			txtOut.BackColor = Color.FromArgb(20, 20, 20);
			txtOut.ForeColor = Color.LightGray;
			lvFiles.BackColor = Color.FromArgb(25, 25, 25);
			lvFiles.ForeColor = Color.White;
			btnRun.BackColor = Color.FromArgb(63, 63, 70);
			btnRun.ForeColor = Color.White;

			ctx.RootPath = filesRoot;
			ctx.Cwd = filesRoot;
			ctx.TempPath = tempPath;

			// Layout
			txtAddress.Dock = DockStyle.Top;

			lvFiles.Dock = DockStyle.Fill;
			lvFiles.Columns.Add("Name", 400);
			lvFiles.Columns.Add("Size", 150);

			txtOut.Dock = DockStyle.Bottom;
			txtOut.Height = 200;

			var cmdPanel = new Panel { Dock = DockStyle.Bottom, Height = 30 };
			txtCmd.Dock = DockStyle.Fill;
			btnRun.Dock = DockStyle.Right;
			btnRun.Width = 100;
			cmdPanel.Controls.Add(txtCmd);
			cmdPanel.Controls.Add(btnRun);

			Controls.Add(lvFiles);
			Controls.Add(txtOut);
			Controls.Add(cmdPanel);
			Controls.Add(txtAddress);

			// Events
			btnRun.Click += (_, __) => RunCmd();
			txtCmd.KeyDown += (s, e) =>
			{
				if (e.KeyCode == Keys.Enter)
				{
					RunCmd();
					e.Handled = true;
					e.SuppressKeyPress = true; // prevents ding
				}
			};

			lvFiles.DoubleClick += LvFiles_DoubleClick;

			ctx.Print = AppendOut;
			ctx.RefreshUi = PopulateList;
			ctx.SetAddress = p => txtAddress.Text = p;
			ctx.NavigateTo = p => NavigateTo(p);

			CommandRegistry.Init();
			txtAddress.Text = ctx.Cwd;
			PopulateList();
		}

		private void RunCmd()
		{
			var line = txtCmd.Text.Trim();
			if (line.Length == 0) return;
			txtCmd.Clear();
			CommandRegistry.Execute(ctx, line);
		}

		private void AppendOut(string text)
		{
			if (text == "\f") { txtOut.Clear(); return; }
			txtOut.AppendText(text + Environment.NewLine);
		}

		private void PopulateList()
		{
			lvFiles.Items.Clear();
			foreach (var d in Directory.GetDirectories(ctx.Cwd))
			{
				var di = new DirectoryInfo(d);
				var item = new ListViewItem(di.Name);
				item.SubItems.Add("<DIR>");
				lvFiles.Items.Add(item);
			}
			foreach (var f in Directory.GetFiles(ctx.Cwd))
			{
				var fi = new FileInfo(f);
				var item = new ListViewItem(fi.Name);
				item.SubItems.Add(fi.Length.ToString());
				lvFiles.Items.Add(item);
			}
		}

		private void NavigateTo(string path)
		{
			if (!Directory.Exists(path)) { AppendOut("Directory not found."); return; }
			ctx.Cwd = path;
			txtAddress.Text = path;
			PopulateList();
		}

		private void LvFiles_DoubleClick(object? sender, EventArgs e)
		{
			if (lvFiles.SelectedItems.Count == 0) return;
			var name = lvFiles.SelectedItems[0].Text;
			var path = Path.Combine(ctx.Cwd, name);
			if (Directory.Exists(path))
			{
				NavigateTo(path);
			}
			else
			{
				try
				{
					Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
				}
				catch (Exception ex)
				{
					AppendOut("Open failed: " + ex.Message);
				}
			}
		}
	}
}
