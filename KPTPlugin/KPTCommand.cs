using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;

namespace KPTPlugin
{
	[Guid("8C2D4A1F-3B5E-49C7-A6F5-1D8C9A3B5E4F")]
	public class KPTCommand : Command
	{
		private static KPTCommand _instance;

		public static KPTCommand Instance { get { return _instance; } }

		public override string EnglishName { get { return "KPT"; } }

		public KPTCommand()
		{
			_instance = this;
		}

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			KPTCoreForm form = new KPTCoreForm(doc, 0.0, new List<GeometryBase>(), KPTCoreForm.AppMode.GemMap);
			IWin32Window owner = RhinoApp.MainWindow();
			if (owner != null)
			{
				((Form)form).Show(owner);
			}
			else
			{
				((Form)form).Show();
			}
			return Result.Success;
		}
	}
}