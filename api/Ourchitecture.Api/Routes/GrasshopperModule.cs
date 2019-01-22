﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using Nancy;
using Newtonsoft.Json;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace Ourchitecture.Api.Routes
{
    public class GrasshopperModule : NancyModule
    {
        public GrasshopperModule()
        {
            Post["/grasshopper"] = _ =>
            {
                var archive = new GH_Archive();
                string json = string.Empty;
                using (var reader = new StreamReader(Request.Body)) json = reader.ReadToEnd();

                GrasshopperInput input = Newtonsoft.Json.JsonConvert.DeserializeObject<GrasshopperInput>(json);

                byte[] byteArray = Convert.FromBase64String(input.Algo);
                string grasshopperXml = System.Text.Encoding.UTF8.GetString(byteArray);

                if (!archive.Deserialize_Xml(grasshopperXml))
                    throw new Exception();

                var definition = new GH_Document();
                if (!archive.ExtractObject(definition, "Definition"))
                    throw new Exception();

                foreach (var obj in definition.Objects)
                {
                    var param = obj as IGH_Param;

                    if (param == null) continue;

                    //this is an input!
                    if (param.Sources.Count == 0 && param.Recipients.Count != 0)
                    {
                        string nick = param.NickName;
                        if (input.Values.ContainsKey(nick))
                        {
                            var val = input.Values[nick];

                            IGH_Structure data = param.VolatileData;

                            GH_Number num = new GH_Number(Convert.ToDouble(val.ToString()));

                            param.AddVolatileData(new GH_Path(0), 0, num);
                        }
                    }
                }

                GrasshopperOutput outputs = new GrasshopperOutput();

                foreach (var obj in definition.Objects)
                {
                    var param = obj as IGH_Param;
                    if (param == null)
                        continue;

                    if (param.Sources.Count == 0 || param.Recipients.Count != 0)
                        continue;

                    try
                    {
                        param.CollectData();
                        param.ComputeData();
                    }
                    catch (Exception)
                    {
                        param.Phase = GH_SolutionPhase.Failed;
                        // TODO: throw something better
                        //throw;
                    }

                    var output = new List<Rhino.Geometry.GeometryBase>();
                    var volatileData = param.VolatileData;
                    for (int p = 0; p < volatileData.PathCount; p++)
                    {
                        foreach (var goo in volatileData.get_Branch(p))
                        {
                            if (goo == null) continue;
                            //case GH_Point point: output.Add(new Rhino.Geometry.Point(point.Value)); break;
                            //case GH_Curve curve: output.Add(curve.Value); break;
                            //case GH_Brep brep: output.Add(brep.Value); break;
                            //case GH_Mesh mesh: output.Add(mesh.Value); break;
                            if (goo.GetType() == typeof(GH_Number))
                            {

                                GrasshopperOutputItem item = new GrasshopperOutputItem();
                                item.Data = (goo as GH_Number).Value.ToString();
                                item.TypeHint = "number";
                                outputs.Items.Add(item);
                                //break;
                            }
                            else if (goo.GetType() == typeof(GH_Mesh))
                            {
                                var rhinoMesh = (goo as GH_Mesh).Value;
                                string jsonMesh = JsonConvert.SerializeObject(rhinoMesh);
                                GrasshopperOutputItem item = new GrasshopperOutputItem();
                                item.Data = jsonMesh;
                                item.TypeHint = "mesh";
                                outputs.Items.Add(item);
                                //break;
                            }
                            else if (goo.GetType() == typeof(GH_Circle))
                            {
                                var rhinoCircles = (goo as GH_Circle).Value;
                                string jsonCircle = JsonConvert.SerializeObject(rhinoCircles);
                                GrasshopperOutputItem item = new GrasshopperOutputItem();
                                item.Data = jsonCircle;
                                item.TypeHint = "circle";
                                outputs.Items.Add(item);
                                //break;
                            }
                            else if (goo.GetType() == typeof(GH_Brep))
                            {
                                var rhinoBrep = (goo as GH_Brep).Value;
                                string jsonBrep = JsonConvert.SerializeObject(rhinoBrep);
                                GrasshopperOutputItem item = new GrasshopperOutputItem();
                                item.Data = jsonBrep;
                                item.TypeHint = "brep";
                                outputs.Items.Add(item);
                            }
                            else if (goo.GetType() == typeof(GH_Line))
                            {
                                var rhinoLine = (goo as GH_Line).Value;
                                string jsonLine = JsonConvert.SerializeObject(rhinoLine);
                                GrasshopperOutputItem item = new GrasshopperOutputItem();
                                item.Data = jsonLine;
                                item.TypeHint = "line";
                                outputs.Items.Add(item);
                            }
                            else if (goo.GetType() == typeof(GH_Arc))
                            {
                                var rhinoArc = (goo as GH_Arc).Value;
                                string jsonArc = JsonConvert.SerializeObject(rhinoArc);
                                GrasshopperOutputItem item = new GrasshopperOutputItem();
                                item.Data = jsonArc;
                                item.TypeHint = "arc";
                                outputs.Items.Add(item);
                            }
                            else if (goo.GetType() == typeof(GH_Point))
                            {
                                var rhinoPoint = (goo as GH_Point).Value;
                                string jsonPoint = JsonConvert.SerializeObject(rhinoPoint);
                                GrasshopperOutputItem item = new GrasshopperOutputItem();
                                item.Data = jsonPoint;
                                item.TypeHint = "point";
                                outputs.Items.Add(item);
                            }
                            else if (goo.GetType() == typeof(GH_Curve))
                            {
                                var rhinoCurve = (goo as GH_Curve).Value;
                                string jsonCurve = JsonConvert.SerializeObject(rhinoCurve);
                                GrasshopperOutputItem item = new GrasshopperOutputItem();
                                item.Data = jsonCurve;
                                item.TypeHint = "curve";
                                outputs.Items.Add(item);
                            }
                            else if (goo.GetType() == typeof(GH_Surface))
                            {
                                var rhinoSurface = (goo as GH_Surface).Value;
                                string jsonSurface = JsonConvert.SerializeObject(rhinoSurface);
                                GrasshopperOutputItem item = new GrasshopperOutputItem();
                                item.Data = jsonSurface;
                                item.TypeHint = "surface";
                                outputs.Items.Add(item);
                            }
                            else if (goo.GetType() == typeof(GH_Boolean))
                            {
                                GrasshopperOutputItem item = new GrasshopperOutputItem();
                                item.Data = (goo as GH_Boolean).Value.ToString();
                                item.TypeHint = "bool";
                                outputs.Items.Add(item);
                            }

                        }
                    }
                }

                if (outputs.Items.Count < 1) throw new Exception();

                string returnJson = JsonConvert.SerializeObject(outputs);
                return returnJson;
            };
        }
    }
}