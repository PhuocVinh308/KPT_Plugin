using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace KPTPlugin
{
	[Guid("B7C89D01-2E3F-4567-89A0-1B2C3D4E5F6A")]
	public class KPTSupportCommand : Command
	{
		private static KPTSupportCommand _instance;

		public static KPTSupportCommand Instance
		{
			get { return _instance; }
		}

		public override string EnglishName
		{
			get { return "KPTSupport"; }
		}

		public KPTSupportCommand()
		{
			_instance = this;
		}

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			GetObject getObj = new GetObject();
			((GetBaseClass)getObj).SetCommandPrompt("Chọn các khối 3D để tạo chân Support (Enter để chọn tất cả đối tượng hiện)");
			getObj.GeometryFilter = (ObjectType)1073741872; // Surface, Polysurface, Mesh
			((GetBaseClass)getObj).AcceptNothing(true);
			getObj.GetMultiple(0, 0);

			List<GeometryBase> geoms = new List<GeometryBase>();
			if ((int)((GetBaseClass)getObj).CommandResult() == 0 && getObj.ObjectCount > 0)
			{
				for (int i = 0; i < getObj.ObjectCount; i++)
				{
					ObjRef objRef = getObj.Object(i);
					if (objRef != null && objRef.Geometry() != null)
					{
						geoms.Add(objRef.Geometry());
					}
				}
			}
			else
			{
				// Select all visible normal objects
				ObjectEnumeratorSettings settings = new ObjectEnumeratorSettings();
				settings.NormalObjects = true;
				settings.LockedObjects = false;
				settings.HiddenObjects = false;
				foreach (RhinoObject obj in doc.Objects.GetObjectList(settings))
				{
					if (obj != null && obj.Geometry != null)
					{
						geoms.Add(obj.Geometry);
					}
				}
			}

			if (geoms.Count == 0)
			{
				RhinoApp.WriteLine("Không tìm thấy khối 3D nào để tạo Support.");
				return Result.Nothing;
			}

			// Clear previous supports
			SupportUtils.ClearSupports(doc);

			// Generate auto supports (tip 0.35mm, stem 0.6mm, base 1.5mm, spacing 1.8mm)
			List<Guid> created = SupportUtils.GenerateAutoSupports(doc, geoms, 0.35, 0.6, 1.5, 1.8, 0.0);
			RhinoApp.WriteLine(string.Format("Đã tạo thành công {0} chân Support cho máy in 3D Resin (Layer: KPT_Support).", created.Count));

			return Result.Success;
		}
	}
}
