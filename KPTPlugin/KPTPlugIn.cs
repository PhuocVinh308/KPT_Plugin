using System.Runtime.InteropServices;
using Rhino.PlugIns;

namespace KPTPlugin
{


[Guid("A1C497B3-8F32-4740-9E11-2D27A1C20B6E")]
public class KPTPlugIn : PlugIn
{
	private static KPTPlugIn _instance;

	public static KPTPlugIn Instance { get { return _instance; } }

	public override PlugInLoadTime LoadTime { get { return (PlugInLoadTime)2; } }

	public KPTPlugIn()
	{
		_instance = this;
	}

	protected override LoadReturnCode OnLoad(ref string errorMessage)
	{
		return (LoadReturnCode)1;
	}
}

}