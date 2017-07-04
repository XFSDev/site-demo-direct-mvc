using System;
using System.Data;
using System.Web.UI.WebControls;
using Models;
using RadarSoft.RadarCube.Common;
using RadarSoft.RadarCube.Web;
using RadarSoft.RadarCube.Web.Mvc;
using OLAPDemoASP.Code;
using RadarSoft.RadarCube.Web.Analysis;
using System.IO;
using System.Web.UI;
using System.Drawing;

namespace SamplesFactory
{
    public class CellContentCustomizationSample : GettingStartedSample
    {
        public CellContentCustomizationSample(SamplesModel samplesModel) : base(samplesModel)
        {
        }

        public override MvcOlapAnalysis OlapAnalysis
        {
            get
            {
                if (_OlapAnalysis == null)
                {
                    _OlapAnalysis = new MvcOlapAnalysis("MvcOlapAnalysis1");

                    _OlapAnalysis.AnalysisType = AnalysisType.Grid;

                    _OlapAnalysis.InitOlap += delegate { InitOlapAnalysis(); };

                    OlapAnalysis.OnRenderCell += OlapAnalysisOnOnRenderCell;

                    var cube = new MvcRCube
                    {
                        DataSet = new Northwind(),
                        ID = OlapAnalysis.ID + "_Cube",
                    };
                    cube.OnCalculateField += Cube_OnCalculateField;
                    _OlapAnalysis.Cube = cube;
                }
                return _OlapAnalysis;
            }
        }

        internal void OlapAnalysisOnOnRenderCell(object sender, RenderCellEventArgs e)
        {
            if (OlapAnalysis.AnalysisType == AnalysisType.Chart)
                return;

            if (e.Cell.CellType == TCellType.ctMember)
            {
                var mc = e.Cell as IMemberCell;
                if ((mc.Member != null) && (mc.Member.Level.DisplayName == "Categories"))
                {
                    string s = e.Text;
                    if (s.Contains("/"))
                        s = s.Substring(0, s.IndexOf('/'));
                    e.Text = "<img src=\"/Content/images/Example/" + s + ".png\">" + e.Text;
                }
            }
            if (e.Cell.CellType == TCellType.ctLevel)
            {
                e.Text = WriteContextMenuButton(e.Text);
            }
            if (e.Cell.CellType == TCellType.ctData)
            {
                IDataCell dc = e.Cell as IDataCell;
                if (dc.Address.Measure != null && dc.Address.Measure.DisplayName == "Quantity")
                {
                    if ((!dc.IsTotalHorizontal) && (SetMaxValue()))
                    {
                        try
                        {
                            Double v = Convert.ToDouble(dc.Data);
                            double hue = (v - _minValue) / (_maxValue - _minValue) * 85;
                            Color c = FromHSB(hue, 255, 255);
                            e.Text = "<table width=\"100%\">" + 
                                "<tr>" +
                                "<td align=\"left\" valign=\"middle\">" +
                                "<img src=\"/OlapAnalysis/PaintTrend/?color=" +
                                ColorTranslator.ToHtml(c).Substring(1) + "\">" +
                                "</td>" +
                                "<td align=\"right\" valign=\"middle\">"
                                + e.Text + "</td>" +
                                "</tr>" +
                                "</table>";
                        }
                        catch
                        {; }
                    }
                }

                if ((dc.Address == null) || (dc.Address.MeasureMode == null)) return;
                if (dc.Address.MeasureMode.Mode != TMeasureShowModeType.smNormal) return;

                try
                {
                    if (dc.Address.Measure.DisplayName == "Sales")
                    {
                        Double d = Convert.ToDouble(30000);
                        Double v = Convert.ToDouble(dc.Data);
                        if (v < d) e.CellStyle.BackColor = Color.Coral;
                    }
                }
                catch
                {; }
            }
        }

        double _maxValue = Double.MinValue;
        double _minValue = Double.MaxValue;

        private bool SetMaxValue()
        {
            if (_maxValue != Double.MinValue) return true;
            if (_minValue == _maxValue) return false;
            for (int i = OlapAnalysis.CellSet.FixedRows; i < OlapAnalysis.CellSet.RowCount; i++)
            {
                IDataCell d = OlapAnalysis.CellSet[OlapAnalysis.CellSet.FixedColumns, i] as IDataCell;
                if ((d == null) || (d.Data == null) || (d.IsTotalHorizontal)) continue;
                try
                {
                    double v = Convert.ToDouble(d.Data);
                    if (v > _maxValue) _maxValue = v;
                    if (v < _minValue) _minValue = v;
                }
                catch
                {; }
            }
            return ((_maxValue != Double.MinValue) && (_minValue != _maxValue));
        }

        private string WriteContextMenuButton(string name)
        {
            var btn = new Button();
            btn.Text = name;
            btn.CssClass = "btn btn-default";
            btn.UseSubmitBehavior = false;
            btn.Attributes.Add("onclick", "RadarSoft.$('#" + OlapAnalysis.ClientID + "').data('grid').createPopup(event);");
            var sw = new StringWriter();
            var w = new Html32TextWriter(sw);
            btn.RenderControl(w);
            return sw.ToString();
        }

        private Color FromHSB(double H, double S, double B)
        {
            double r = B;
            double g = B;
            double b = B;
            if (S != 0)
            {
                double max = B;
                double dif = B * S / 255f;
                double min = B - dif;

                double h = H * 360f / 255f;

                if (h < 60f)
                {
                    r = max;
                    g = h * dif / 60f + min;
                    b = min;
                }
                else if (h < 120f)
                {
                    r = -(h - 120f) * dif / 60f + min;
                    g = max;
                    b = min;
                }
                else if (h < 180f)
                {
                    r = min;
                    g = max;
                    b = (h - 120f) * dif / 60f + min;
                }
                else if (h < 240f)
                {
                    r = min;
                    g = -(h - 240f) * dif / 60f + min;
                    b = max;
                }
                else if (h < 300f)
                {
                    r = (h - 240f) * dif / 60f + min;
                    g = min;
                    b = max;
                }
                else if (h <= 360f)
                {
                    r = max;
                    g = min;
                    b = -(h - 360f) * dif / 60 + min;
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }

            return Color.FromArgb
                (
                    255,
                    (int)Math.Round(Math.Min(Math.Max(r, 0), 255)),
                    (int)Math.Round(Math.Min(Math.Max(g, 0), 255)),
                    (int)Math.Round(Math.Min(Math.Max(b, 0), 255))
                    );
        }
    }
}