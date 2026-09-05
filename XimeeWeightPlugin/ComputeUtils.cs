using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace XimeeWeightPlugin
{


public static class ComputeUtils
{
	public static Result SelectAndComputeVolume(RhinoDoc doc, out double totalVolumeCm3, out List<GeometryBase> metalGeoms)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		totalVolumeCm3 = 0.0;
		metalGeoms = new List<GeometryBase>();
		GetObject val = new GetObject();
		((GetBaseClass)val).SetCommandPrompt("Chọn các khối kim loại để tính khối lượng (Enter để bỏ qua) / Select metal objects (Enter to skip)");
		val.GeometryFilter = (ObjectType)1073741872;
		val.SubObjectSelect = false;
		val.GroupSelect = true;
		((GetBaseClass)val).AcceptNothing(true);
		val.GetMultiple(0, 0);
		if ((int)((GetBaseClass)val).CommandResult() != 0 && (int)((GetBaseClass)val).CommandResult() != 2)
		{
			return ((GetBaseClass)val).CommandResult();
		}
		List<Brep> list = new List<Brep>();
		List<Mesh> list2 = new List<Mesh>();
		if ((int)((GetBaseClass)val).CommandResult() == 0)
		{
			ObjRef[] array = val.Objects();
			foreach (ObjRef val2 in array)
			{
				Brep val3 = val2.Brep();
				if (val3 != null)
				{
					list.Add(val3);
				}
				else
				{
					Mesh val4 = val2.Mesh();
					if (val4 != null)
					{
						list2.Add(val4);
					}
				}
				if (val2.Geometry() != null)
				{
					metalGeoms.Add(val2.Geometry());
				}
			}
		}
		double x = RhinoMath.UnitScale(doc.ModelUnitSystem, (UnitSystem)3);
		if (list.Count > 0)
		{
			foreach (Brep item in list)
			{
				VolumeMassProperties val5 = VolumeMassProperties.Compute(item);
				if (val5 != null)
				{
					totalVolumeCm3 += val5.Volume * Math.Pow(x, 3.0);
				}
			}
		}
		if (list2.Count > 0)
		{
			foreach (Mesh item2 in list2)
			{
				VolumeMassProperties val6 = VolumeMassProperties.Compute(item2);
				if (val6 != null)
				{
					totalVolumeCm3 += val6.Volume * Math.Pow(x, 3.0);
				}
			}
		}
		return (Result)0;
	}
}

}