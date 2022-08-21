using System;
using System.Windows.Forms;

namespace Ribbons
{
	internal static class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			using (Game game = new Game())
			{
				game.Run();
			}
		}

		internal static void RemoveBorder(IntPtr hWnd)
		{
			Form form = (Form)Control.FromHandle(hWnd);
			form.FormBorderStyle = FormBorderStyle.None;
		}

		internal static void PositionWindow(IntPtr hWnd, int left, int top)
		{
			Form form = (Form)Control.FromHandle(hWnd);
			form.SetDesktopLocation(left, top);
		}
	}
}
