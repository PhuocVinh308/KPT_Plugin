using System;
using System.Collections.Generic;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace KPTPlugin
{
	public static class SupportUtils
	{
		public static List<Guid> GenerateAutoSupports(RhinoDoc doc, IEnumerable<GeometryBase> geometries, double tipRadius, double stemRadius, double baseRadius, double gridSpacing, double baseZ)
		{
			List<Guid> createdIds = new List<Guid>();
			int layerIndex = doc.Layers.Find("KPT_Support", true);
			if (layerIndex < 0)
			{
				layerIndex = doc.Layers.Add("KPT_Support", System.Drawing.Color.DarkMagenta);
			}

			foreach (GeometryBase geom in geometries)
			{
				if (geom == null) continue;
				BoundingBox bbox = geom.GetBoundingBox(true);
				if (!bbox.IsValid) continue;

				Mesh mesh = geom as Mesh;
				if (mesh == null && geom is Brep)
				{
					Mesh[] meshes = Mesh.CreateFromBrep((Brep)geom, MeshingParameters.Default);
					if (meshes != null && meshes.Length > 0)
					{
						mesh = new Mesh();
						foreach (Mesh m in meshes) mesh.Append(m);
					}
				}

				if (mesh == null) continue;
				mesh.Normals.ComputeNormals();

				List<Point3d> supportPoints = new List<Point3d>();
				for (int i = 0; i < mesh.Vertices.Count; i++)
				{
					Vector3f normal = mesh.Normals[i];
					if (normal.Z < -0.4f)
					{
						Point3d pt = new Point3d(mesh.Vertices[i]);
						bool tooClose = false;
						foreach (Point3d existing in supportPoints)
						{
							if (pt.DistanceTo(existing) < gridSpacing)
							{
								tooClose = true;
								break;
							}
						}
						if (!tooClose && pt.Z > baseZ + 0.5)
						{
							supportPoints.Add(pt);
						}
					}
				}

				foreach (Point3d pt in supportPoints)
				{
					List<GeometryBase> pillarGeoms = CreateSupportPillar(pt, baseZ, tipRadius, stemRadius, baseRadius);
					foreach (GeometryBase pillar in pillarGeoms)
					{
						Guid id = Guid.Empty;
						Brep bPillar = pillar as Brep;
						Mesh mPillar = pillar as Mesh;
						if (bPillar != null) id = doc.Objects.AddBrep(bPillar);
						else if (mPillar != null) id = doc.Objects.AddMesh(mPillar);

						if (id != Guid.Empty)
						{
							RhinoObject obj = doc.Objects.Find(id);
							if (obj != null)
							{
								obj.Attributes.LayerIndex = layerIndex;
								obj.Attributes.ObjectColor = System.Drawing.Color.DarkMagenta;
								obj.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
								obj.CommitChanges();
								createdIds.Add(id);
							}
						}
					}
				}
			}

			doc.Views.Redraw();
			return createdIds;
		}

		public static List<GeometryBase> CreateSupportPillar(Point3d topPoint, double baseZ, double tipRadius, double stemRadius, double baseRadius)
		{
			List<GeometryBase> geoms = new List<GeometryBase>();
			if (topPoint.Z <= baseZ + 0.5) return geoms;

			double tipHeight = 1.0;
			Point3d tipBottom = new Point3d(topPoint.X, topPoint.Y, topPoint.Z - tipHeight);
			if (tipBottom.Z < baseZ) tipBottom.Z = baseZ;

			Plane planeTop = new Plane(topPoint, Vector3d.ZAxis);
			Plane planeTipBottom = new Plane(tipBottom, Vector3d.ZAxis);
			Circle circleTop = new Circle(planeTop, tipRadius);
			Circle circleTipBottom = new Circle(planeTipBottom, stemRadius);
			Brep[] tipLoft = Brep.CreateFromLoft(new Curve[] { circleTop.ToNurbsCurve(), circleTipBottom.ToNurbsCurve() }, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);
			if (tipLoft != null && tipLoft.Length > 0)
			{
				Brep cappedTip = tipLoft[0].CapPlanarHoles(0.01);
				if (cappedTip != null) geoms.Add(cappedTip);
				else geoms.Add(tipLoft[0]);
			}

			Point3d stemBottom = new Point3d(topPoint.X, topPoint.Y, baseZ + 0.3);
			if (stemBottom.Z < tipBottom.Z)
			{
				Cylinder stemCyl = new Cylinder(new Circle(new Plane(stemBottom, Vector3d.ZAxis), stemRadius), tipBottom.Z - stemBottom.Z);
				Brep stemBrep = stemCyl.ToBrep(true, true);
				if (stemBrep != null) geoms.Add(stemBrep);
			}

			Cylinder baseCyl = new Cylinder(new Circle(new Plane(new Point3d(topPoint.X, topPoint.Y, baseZ), Vector3d.ZAxis), baseRadius), 0.3);
			Brep baseBrep = baseCyl.ToBrep(true, true);
			if (baseBrep != null) geoms.Add(baseBrep);

			return geoms;
		}

		public static void ClearSupports(RhinoDoc doc)
		{
			int layerIndex = doc.Layers.Find("KPT_Support", true);
			if (layerIndex >= 0)
			{
				RhinoObject[] objs = doc.Objects.FindByLayer(doc.Layers[layerIndex]);
				if (objs != null)
				{
					foreach (RhinoObject obj in objs)
					{
						if (obj != null) doc.Objects.Delete(obj, true);
					}
				}
			}
			doc.Views.Redraw();
		}
	}
}
