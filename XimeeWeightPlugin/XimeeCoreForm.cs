using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = System.Drawing.Font;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using PdfFont = iTextSharp.text.Font;
using PdfImage = iTextSharp.text.Image;
using PdfRectangle = iTextSharp.text.Rectangle;

namespace XimeeWeightPlugin
{


public class XimeeCoreForm : Form
{
	public enum AppMode
	{
		Core,
		Weight,
		GemMap
	}

	private const int BASE_WIDTH = 550;

	private const float BASE_FONT_SIZE = 9f;

	private const int BASE_ROW_HEIGHT = 26;

	private const int BASE_HEADER_HEIGHT = 32;

	private const int BASE_GEM_ROW_HEIGHT = 40;

	private AppMode currentMode;

	private bool _gemExpanded;

	private bool isVN = true;

	private Button btnLang;

	private Button btnLang2;

	private RhinoDoc doc;

	private double volumeCm3;

	private DataGridView gridMetal;

	private DataGridView gridGem;

	private Button btnGemMap;

	private Panel panelGemBtns;

	private Button btnHideGemMap;

	private Button btnRestoreGemMap;

	private Button btnPrintGemMap;

	private FlowLayoutPanel panelColor;

	private Dictionary<Guid, Color> originalColors = new Dictionary<Guid, Color>();

	private Dictionary<Guid, ObjectColorSource> originalSources = new Dictionary<Guid, ObjectColorSource>();

	private List<Tuple<string, double, double, double>> masterMetalData = new List<Tuple<string, double, double, double>>();

	private string selectedMetal = "";

	private string selectedColor = "";

	private List<GeometryBase> metalGeoms;

	private Dictionary<Tuple<string, string>, List<RhinoObject>> gemGroups = new Dictionary<Tuple<string, string>, List<RhinoObject>>();

	private Button btnAnnotate;

	private Button btnExportPDF;

	private string pdfExportPath = "";

	private TextBox txtModelCode;

	private Label lblModelCode;
	private Button btnNi;

	private Panel panelModelCode;

	private int lastScaledWidth = -1;

	private static int savedWidth;

	private bool gemPreviewRun;

	private static string SettingsPath
	{
		get
		{
			string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XimeeCore");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return Path.Combine(text, "settings.txt");
		}
	}

	private static int LoadSavedWidth()
	{
		try
		{
			int result;
			if (File.Exists(SettingsPath) && int.TryParse(File.ReadAllText(SettingsPath).Trim(), out result) && result >= 300)
			{
				return result;
			}
		}
		catch
		{
		}
		return 0;
	}

	private static void SaveWidth(int w)
	{
		try
		{
			File.WriteAllText(SettingsPath, w.ToString());
		}
		catch
		{
		}
	}

	public XimeeCoreForm(RhinoDoc doc, double volumeCm3, List<GeometryBase> metalGeoms, AppMode mode = AppMode.Core)
	{
		currentMode = mode;
		this.doc = doc;
		this.volumeCm3 = volumeCm3;
		this.metalGeoms = metalGeoms;
		if (savedWidth == 0)
		{
			savedWidth = LoadSavedWidth();
		}
		InitializeComponent();
		if (currentMode != AppMode.GemMap)
		{
			PopulateMetalData();
		}
		AdjustFormSize(gemExpanded: false);
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		if (currentMode == AppMode.GemMap && !gemPreviewRun)
		{
			gemPreviewRun = true;
			PerformGemMapPreview();
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (((Control)this).IsHandleCreated)
		{
			LayoutControls(((Form)this).ClientSize.Width, setFormSize: true);
		}
	}

	protected override void OnResizeEnd(EventArgs e)
	{
		base.OnResizeEnd(e);
		savedWidth = ((Form)this).ClientSize.Width;
		SaveWidth(savedWidth);
	}

	private void InitializeComponent()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Expected O, but got Unknown
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Expected O, but got Unknown
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Expected O, but got Unknown
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Expected O, but got Unknown
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Expected O, but got Unknown
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Expected O, but got Unknown
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Expected O, but got Unknown
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Expected O, but got Unknown
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ad: Expected O, but got Unknown
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Expected O, but got Unknown
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0589: Expected O, but got Unknown
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f2: Expected O, but got Unknown
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_066d: Expected O, but got Unknown
		//IL_067a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0681: Expected O, but got Unknown
		//IL_06cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d7: Expected O, but got Unknown
		//IL_0726: Unknown result type (might be due to invalid IL or missing references)
		//IL_072d: Expected O, but got Unknown
		//IL_0784: Unknown result type (might be due to invalid IL or missing references)
		//IL_078b: Expected O, but got Unknown
		//IL_07f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0802: Expected O, but got Unknown
		//IL_0846: Unknown result type (might be due to invalid IL or missing references)
		//IL_0850: Expected O, but got Unknown
		//IL_08a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08aa: Expected O, but got Unknown
		//IL_090a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0914: Expected O, but got Unknown
		//IL_0925: Unknown result type (might be due to invalid IL or missing references)
		//IL_092f: Expected O, but got Unknown
		//IL_0940: Unknown result type (might be due to invalid IL or missing references)
		//IL_094a: Expected O, but got Unknown
		//IL_094b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0955: Expected O, but got Unknown
		//IL_0963: Unknown result type (might be due to invalid IL or missing references)
		//IL_096d: Expected O, but got Unknown
		//IL_09b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bd: Expected O, but got Unknown
		//IL_09da: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e4: Expected O, but got Unknown
		//IL_09f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ff: Expected O, but got Unknown
		//IL_0a0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a15: Expected O, but got Unknown
		//IL_0a21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2b: Expected O, but got Unknown
		//IL_0a37: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a41: Expected O, but got Unknown
		//IL_0a4d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a57: Expected O, but got Unknown
		//IL_0a63: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6d: Expected O, but got Unknown
		//IL_0a79: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a83: Expected O, but got Unknown
		//IL_0a8f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a99: Expected O, but got Unknown
		if (currentMode == AppMode.GemMap)
		{
			((Control)this).Text = "KPT";
		}
		else if (currentMode == AppMode.Weight)
		{
			((Control)this).Text = "Ximee Weight";
		}
		else
		{
			((Control)this).Text = "Ximee Core";
		}
		((Form)this).StartPosition = (FormStartPosition)1;
		((Form)this).FormBorderStyle = (FormBorderStyle)6;
		((Form)this).MaximizeBox = false;
		((Form)this).MinimizeBox = false;
		((Form)this).ShowIcon = false;
		((Control)this).MinimumSize = new Size(250, 200);
		((Control)this).Font = new Font("Segoe UI", 9f, (FontStyle)0);
		gridMetal = new DataGridView();
		gridMetal.AllowUserToAddRows = false;
		gridMetal.ReadOnly = true;
		gridMetal.RowHeadersVisible = false;
		gridMetal.AutoSizeColumnsMode = (DataGridViewAutoSizeColumnsMode)16;
		gridMetal.SelectionMode = (DataGridViewSelectionMode)1;
		gridMetal.BackgroundColor = SystemColors.Window;
		gridMetal.ScrollBars = (ScrollBars)0;
		gridMetal.DefaultCellStyle.SelectionBackColor = SystemColors.Window;
		gridMetal.DefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
		gridMetal.EnableHeadersVisualStyles = false;
		gridMetal.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, (FontStyle)1);
		gridMetal.ColumnHeadersHeightSizeMode = (DataGridViewColumnHeadersHeightSizeMode)1;
		gridMetal.ColumnHeadersHeight = 32;
		gridMetal.Columns.Add("Metal", isVN ? "Kim Loại" : "Metal");
		gridMetal.Columns.Add("Chi", isVN ? "Chỉ" : "Tael");
		gridMetal.Columns.Add("Gram", "Gram");
		gridMetal.Columns.Add("Ounce", "Ounce");
		gridMetal.Columns[0].FillWeight = 35f;
		gridMetal.Columns[1].FillWeight = 20f;
		gridMetal.Columns[2].FillWeight = 22f;
		gridMetal.Columns[3].FillWeight = 23f;
		gridGem = new DataGridView();
		gridGem.BackgroundColor = SystemColors.Window;
		gridGem.AllowUserToAddRows = false;
		gridGem.ReadOnly = true;
		gridGem.RowHeadersVisible = false;
		gridGem.AutoSizeColumnsMode = (DataGridViewAutoSizeColumnsMode)16;
		gridGem.SelectionMode = (DataGridViewSelectionMode)1;
		gridGem.ScrollBars = (ScrollBars)2;
		gridGem.DefaultCellStyle.SelectionBackColor = SystemColors.Window;
		gridGem.DefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
		((Control)gridGem).Font = new Font("Segoe UI", 10f, (FontStyle)0);
		gridGem.EnableHeadersVisualStyles = false;
		gridGem.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, (FontStyle)1);
		gridGem.ColumnHeadersHeightSizeMode = (DataGridViewColumnHeadersHeightSizeMode)1;
		gridGem.ColumnHeadersHeight = 32;
		gridGem.Columns.Add("Shape", isVN ? "Kiểu đá" : "Gem Shape");
		gridGem.Columns.Add("Size", isVN ? "Kích cỡ (mm)" : "Size (mm)");
		gridGem.Columns.Add("Qty", isVN ? "S.Lượng" : "Qty");
		gridGem.Columns[0].FillWeight = 40f;
		gridGem.Columns[1].FillWeight = 35f;
		gridGem.Columns[2].FillWeight = 25f;
		gridGem.ClearSelection();
		gridGem.CellPainting += new DataGridViewCellPaintingEventHandler(GridGem_CellPainting);
		gridGem.CellClick += new DataGridViewCellEventHandler(GridGem_CellClick);
		gridGem.CellDoubleClick += new DataGridViewCellEventHandler(GridGem_CellDoubleClick);
		panelColor = new FlowLayoutPanel();
		((Control)panelColor).Height = 30;
		((Control)panelColor).BackColor = Color.WhiteSmoke;
		panelColor.FlowDirection = (FlowDirection)0;
		string[] array = new string[5] { "Tất cả", "Vàng", "Trắng", "Hồng", "Platinum" };
		string[] array2 = new string[5] { "All", "Yellow", "White", "Rose", "Platinum" };
		string[] array3 = new string[5] { "all", "yellow", "white", "rose", "platinum" };
		string[] array4 = (isVN ? array : array2);
		for (int i = 0; i < array4.Length; i++)
		{
			RadioButton val = new RadioButton();
			((Control)val).Text = array4[i];
			((Control)val).Tag = array3[i];
			((Control)val).AutoSize = true;
			if (i == 0)
			{
				val.Checked = true;
			}
			val.CheckedChanged += RbColor_CheckedChanged;
			((Control)panelColor).Controls.Add((Control)val);
		}
		gridMetal.CellClick += new DataGridViewCellEventHandler(GridMetal_CellClick);
		btnGemMap = new Button();
		((Control)btnGemMap).Text = (isVN ? "Bản Đồ Đá" : "Gem Map");
		((Control)btnGemMap).Font = new Font("Segoe UI", 9f, (FontStyle)1);
		((Control)btnGemMap).Cursor = Cursors.Hand;
		((Control)btnGemMap).BackColor = Color.FromArgb(50, 150, 250);
		((Control)btnGemMap).ForeColor = Color.White;
		((ButtonBase)btnGemMap).FlatStyle = (FlatStyle)0;
		((ButtonBase)btnGemMap).FlatAppearance.BorderSize = 0;
		((Control)btnGemMap).Click += BtnGemMap_Click;
		panelGemBtns = new Panel();
		((Control)panelGemBtns).Height = 35;
		Button val2 = new Button();
		((Control)val2).Text = (isVN ? "Chụp Ảnh" : "Snapshot");
		((Control)val2).Width = 120;
		((Control)val2).Dock = (DockStyle)3;
		btnPrintGemMap = val2;
		((Control)btnPrintGemMap).Click += BtnPrintGemMap_Click;
		btnLang = new Button();
		((Control)btnLang).Text = (isVN ? "VN" : "EN");
		((Control)btnLang).Width = 45;
		((Control)btnLang).Dock = (DockStyle)3;
		((Control)btnLang).Click += BtnLang_Click;
		Button val3 = new Button();
		((Control)val3).Text = (isVN ? "Trở về" : "Back");
		((Control)val3).Width = 65;
		((Control)val3).Dock = (DockStyle)3;
		btnRestoreGemMap = val3;
		((Control)btnRestoreGemMap).Click += BtnRestoreGemMap_Click;
		((Control)btnRestoreGemMap).Visible = true;
		Button val4 = new Button();
		((Control)val4).Text = (isVN ? "Ẩn" : "Hide");
		((Control)val4).Width = 60;
		((Control)val4).Dock = (DockStyle)4;
		btnHideGemMap = val4;
		((Control)btnHideGemMap).Visible = false;
		if (currentMode == AppMode.GemMap)
		{
			((Control)btnHideGemMap).Visible = false;
		}
		((Control)btnHideGemMap).Click += delegate
		{
			AdjustFormSize(gemExpanded: false);
		};
		btnLang2 = new Button();
		((Control)btnLang2).Text = (isVN ? "VN" : "EN");
		((Control)btnLang2).Width = 45;
		((Control)btnLang2).Click += BtnLang_Click;
		btnAnnotate = new Button();
		((Control)btnAnnotate).Text = (isVN ? "Ghi Chú" : "Annotate");
		((Control)btnAnnotate).Width = 80;
		((Control)btnAnnotate).Dock = (DockStyle)3;
		((Control)btnAnnotate).Click += BtnAnnotate_Click;
		btnExportPDF = new Button();
		((Control)btnExportPDF).Text = (isVN ? "Xuất PDF" : "PDF Export");
		((Control)btnExportPDF).Width = 60;
		((Control)btnExportPDF).Dock = (DockStyle)3;
		((Control)btnExportPDF).Click += BtnExportPDF_Click;
		((Control)panelGemBtns).Controls.Add((Control)btnPrintGemMap);
		((Control)panelGemBtns).Controls.Add((Control)btnAnnotate);
		((Control)panelGemBtns).Controls.Add((Control)btnRestoreGemMap);
		panelModelCode = new Panel();
		((Control)panelModelCode).Height = 25;
		lblModelCode = new Label();
		((Control)lblModelCode).Text = (isVN ? "Mã" : "Code");
		((Control)lblModelCode).Width = 35;
		((Control)lblModelCode).Dock = (DockStyle)3;
		lblModelCode.TextAlign = (ContentAlignment)16;

		btnNi = new Button();
		((Control)btnNi).Text = "Ni";
		((Control)btnNi).Width = 40;
		((Control)btnNi).Dock = (DockStyle)4;
		((Control)btnNi).Click += BtnNi_Click;

		txtModelCode = new TextBox();
		((Control)txtModelCode).Dock = (DockStyle)5;

		((Control)panelModelCode).Controls.Add((Control)txtModelCode);
		((Control)panelModelCode).Controls.Add((Control)btnNi);
		((Control)panelModelCode).Controls.Add((Control)lblModelCode);
		((Control)this).Controls.Add((Control)panelModelCode);
		((Control)this).Controls.Add((Control)gridMetal);
		((Control)this).Controls.Add((Control)panelColor);
		((Control)this).Controls.Add((Control)btnLang2);
		((Control)this).Controls.Add((Control)btnGemMap);
		((Control)this).Controls.Add((Control)gridGem);
		((Control)this).Controls.Add((Control)panelGemBtns);
	}

	private void PopulateMetalData()
	{
		Dictionary<string, double> obj = new Dictionary<string, double>
		{
			{ "Vàng 24K / 24K Gold", 19.32 },
			{ "Vàng 18K / 18K Gold", 15.6 },
			{ "Vàng 14K / 14K Gold", 13.0 },
			{ "Vàng 10K / 10K Gold", 11.5 },
			{ "Bạc 925 / Silver 925", 10.36 },
			{ "Platinum Pt999", 21.45 },
			{ "Platinum Pt950", 20.64 }
		};
		gridMetal.Rows.Clear();
		gridMetal.RowTemplate.Height = 26;
		masterMetalData.Clear();
		foreach (KeyValuePair<string, double> item3 in obj)
		{
			double num = volumeCm3 * item3.Value;
			double item = num / 3.75;
			double item2 = num / 31.1034768;
			masterMetalData.Add(new Tuple<string, double, double, double>(item3.Key, item, num, item2));
		}
		RefreshMetalGrid();
	}

	private void AdjustFormSize(bool gemExpanded)
	{
		_gemExpanded = gemExpanded;
		int formWidth = 280;
		if (savedWidth >= 200 && savedWidth < 450)
		{
			formWidth = savedWidth;
		}
		LayoutControls(formWidth, setFormSize: true);
	}

	private float ScaleFactor(int formWidth)
	{
		float val = (float)formWidth / 280f;
		return Math.Max(1f, Math.Min(val, 1.5f));
	}

	private void ApplyScaledFonts(int formWidth)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Expected O, but got Unknown
		int num = formWidth / 20 * 20;
		if (num == lastScaledWidth)
		{
			return;
		}
		lastScaledWidth = num;
		float num2 = ScaleFactor(formWidth);
		float num3 = 11f * num2;
		float num4 = 13f * num2;
		Font font = new Font("Segoe UI", num3, (FontStyle)0);
		Font font2 = new Font("Segoe UI", num3, (FontStyle)1);
		Font font3 = new Font("Segoe UI", num4, (FontStyle)0);
		gridMetal.DefaultCellStyle.Font = font;
		gridMetal.ColumnHeadersDefaultCellStyle.Font = font2;
		int columnHeadersHeight = Math.Max(20, (int)(32f * num2));
		int height = Math.Max(18, (int)(26f * num2));
		gridMetal.ColumnHeadersHeight = columnHeadersHeight;
		gridMetal.RowTemplate.Height = height;
		for (int i = 0; i < gridMetal.Rows.Count; i++)
		{
			gridMetal.Rows[i].Height = height;
		}
		gridGem.DefaultCellStyle.Font = font3;
		gridGem.ColumnHeadersDefaultCellStyle.Font = font2;
		gridGem.ColumnHeadersHeight = columnHeadersHeight;
		int height2 = Math.Max(24, (int)(40f * num2));
		gridGem.RowTemplate.Height = height2;
		for (int j = 0; j < gridGem.Rows.Count; j++)
		{
			gridGem.Rows[j].Height = height2;
		}
		((Control)btnGemMap).Font = font2;
		((Control)btnPrintGemMap).Font = font;
		((Control)btnAnnotate).Font = font;
		((Control)btnExportPDF).Font = font;
		((Control)lblModelCode).Font = font;
		((Control)txtModelCode).Font = font;
		((Control)btnNi).Font = font;
		((Control)btnRestoreGemMap).Font = font;
		((Control)btnHideGemMap).Font = font;
		((Control)btnLang).Font = font;
		((Control)btnLang2).Font = font;
		for (int k = 0; k < ((ArrangedElementCollection)((Control)panelColor).Controls).Count; k++)
		{
			Control val = ((Control)panelColor).Controls[k];
			RadioButton val2 = (RadioButton)((val is RadioButton) ? val : null);
			if (val2 != null)
			{
				((Control)val2).Font = font;
			}
		}
	}

	private void LayoutControls(int formWidth, bool setFormSize)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Expected O, but got Unknown
		bool gemExpanded = _gemExpanded;
		float num = ScaleFactor(formWidth);
		ApplyScaledFonts(formWidth);
		int num2 = 0;
		if (currentMode != AppMode.GemMap)
		{
			num2 = gridMetal.ColumnHeadersHeight;
			foreach (DataGridViewRow item in (IEnumerable)gridMetal.Rows)
			{
				DataGridViewRow val = item;
				num2 += val.Height;
			}
		}
		int num3 = ((currentMode != AppMode.GemMap) ? ((int)(30f * num)) : 0);
		int num4 = (int)(50f * num);
		((Control)gridMetal).Visible = currentMode != AppMode.GemMap;
		((Control)panelColor).Visible = currentMode != AppMode.GemMap;
		int num5 = 0;
		if (currentMode != AppMode.GemMap)
		{
			((Control)gridMetal).SetBounds(0, num5, formWidth, num2);
			num5 += num2;
			((Control)panelColor).SetBounds(0, num5, formWidth - num4, num3);
			((Control)panelColor).Height = num3;
			((Control)btnLang2).SetBounds(formWidth - num4, num5, num4, num3);
			((Control)btnLang2).Visible = true;
			num5 += num3;
		}
		else
		{
			((Control)btnLang2).Visible = false;
		}
		((Control)btnGemMap).Visible = false;
		((Control)panelGemBtns).Visible = false;
		((Control)panelModelCode).Visible = false;
		((Control)gridGem).Visible = false;
		int num6 = (int)(35f * num);
		int num7 = (int)(45f * num);
		if (currentMode != AppMode.Weight)
		{
			if (gemExpanded)
			{
				((Control)panelGemBtns).Visible = true;
				((Control)panelModelCode).Visible = true;
				((Control)gridGem).Visible = true;
				((Control)panelGemBtns).SetBounds(0, num5, formWidth, num6);
				((Control)panelGemBtns).Height = num6;
				((Control)btnPrintGemMap).Width = (int)(75f * num);
				((Control)btnAnnotate).Width = (int)(65f * num);
				((Control)btnRestoreGemMap).Width = (int)(65f * num);
				((Control)btnExportPDF).Width = (int)(60f * num);
				((Control)btnLang).Width = (int)(35f * num);
				((Control)btnHideGemMap).Width = (int)(60f * num);
				num5 += num6;
				int num8 = (int)(30f * num);
				((Control)panelModelCode).SetBounds(0, num5, formWidth, num8);
				((Control)panelModelCode).Height = num8;
				((Control)lblModelCode).Width = (int)(35f * num);
				((Control)btnNi).Width = (int)(40f * num);
				num5 += num8;
				int num9;
				if (setFormSize)
				{
					num9 = gridGem.ColumnHeadersHeight;
					foreach (DataGridViewRow item2 in (IEnumerable)gridGem.Rows)
					{
						DataGridViewRow val2 = item2;
						num9 += val2.Height;
					}
					num9 += 2;
					Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
					int num10 = num5 + num9;
					int num11 = (int)((double)workingArea.Height * 0.9);
					if (num10 > num11)
					{
						num9 = num11 - num5;
					}
				}
				else
				{
					num9 = gridGem.ColumnHeadersHeight;
					for (int i = 0; i < gridGem.Rows.Count; i++)
					{
						num9 += gridGem.Rows[i].Height;
					}
					num9 += 2;
					int num12 = (int)((double)Screen.PrimaryScreen.WorkingArea.Height * 0.9);
					if (num5 + num9 > num12)
					{
						num9 = num12 - num5;
					}
					if (num9 < 50)
					{
						num9 = 50;
					}
				}
				((Control)gridGem).SetBounds(0, num5, formWidth, num9);
				num5 += num9;
			}
			else
			{
				((Control)btnGemMap).Visible = true;
				((Control)btnGemMap).SetBounds(0, num5, formWidth, num7);
				num5 += num7;
			}
		}
		if (setFormSize)
		{
			((Form)this).ClientSize = new Size(formWidth, num5);
		}
		if (((Control)btnGemMap).Visible)
		{
			((Control)btnGemMap).Enabled = true;
		}
	}

	private void RefreshMetalGrid()
	{
		gridMetal.Rows.Clear();
		foreach (Tuple<string, double, double, double> masterMetalDatum in masterMetalData)
		{
			if (string.IsNullOrEmpty(selectedMetal) || !(masterMetalDatum.Item1 != selectedMetal))
			{
				string text = LocalizeMetal(masterMetalDatum.Item1);
				if (!string.IsNullOrEmpty(selectedColor) && !string.IsNullOrEmpty(selectedMetal))
				{
					text = text + " " + selectedColor;
				}
				gridMetal.Rows.Add(new object[4]
				{
					text,
					masterMetalDatum.Item2.ToString("F2"),
					masterMetalDatum.Item3.ToString("F2"),
					masterMetalDatum.Item4.ToString("F2")
				});
			}
		}
		AdjustFormSize(((Control)gridGem).Visible);
	}

	private string LocalizeMetal(string dualName)
	{
		string[] array = dualName.Split(new char[1] { '/' });
		if (array.Length == 2)
		{
			if (!isVN)
			{
				return array[1].Trim();
			}
			return array[0].Trim();
		}
		return dualName;
	}

	private void GridMetal_CellClick(object sender, DataGridViewCellEventArgs e)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		if (e.RowIndex < 0)
		{
			return;
		}
		string text = gridMetal.Rows[e.RowIndex].Cells[0].Value.ToString();
		if (!string.IsNullOrEmpty(selectedMetal))
		{
			selectedMetal = "";
			selectedColor = "";
			foreach (Control item in (ArrangedElementCollection)((Control)panelColor).Controls)
			{
				Control val = item;
				RadioButton val2 = (RadioButton)((val is RadioButton) ? val : null);
				if (val2 != null && ((Control)val2).Tag as string == "all")
				{
					val2.Checked = true;
					break;
				}
			}
		}
		else
		{
			foreach (Tuple<string, double, double, double> masterMetalDatum in masterMetalData)
			{
				string text2 = LocalizeMetal(masterMetalDatum.Item1);
				if (text.StartsWith(text2.Split(new char[1] { ' ' })[0]))
				{
					selectedMetal = masterMetalDatum.Item1;
					break;
				}
			}
		}
		RefreshMetalGrid();
	}

	private void RbColor_CheckedChanged(object sender, EventArgs e)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		RadioButton val = (RadioButton)((sender is RadioButton) ? sender : null);
		if (val == null || !val.Checked)
		{
			return;
		}
		if (((((Control)val).Tag as string) ?? "") == "all")
		{
			selectedColor = "";
			selectedMetal = "";
		}
		else
		{
			selectedColor = ((Control)val).Text;
			if (string.IsNullOrEmpty(selectedMetal) && gridMetal.Rows.Count > 0)
			{
				string text = gridMetal.Rows[0].Cells[0].Value.ToString();
				foreach (Tuple<string, double, double, double> masterMetalDatum in masterMetalData)
				{
					string text2 = LocalizeMetal(masterMetalDatum.Item1);
					if (text.StartsWith(text2.Split(new char[1] { ' ' })[0]))
					{
						selectedMetal = masterMetalDatum.Item1;
						break;
					}
				}
			}
		}
		RefreshMetalGrid();
	}

	private void BtnLang_Click(object sender, EventArgs e)
	{
		isVN = !isVN;
		UpdateLanguage();
	}

	private void UpdateLanguage()
	{
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Expected O, but got Unknown
		string text = (isVN ? "VN" : "EN");
		((Control)btnLang).Text = text;
		((Control)btnLang2).Text = text;
		gridMetal.Columns[0].HeaderText = (isVN ? "Kim Loại" : "Metal");
		gridMetal.Columns[1].HeaderText = (isVN ? "Chỉ" : "Tael");
		gridMetal.Columns[2].HeaderText = "Gram";
		gridMetal.Columns[3].HeaderText = "Ounce";
		gridGem.Columns[0].HeaderText = (isVN ? "Kiểu đá" : "Gem Shape");
		gridGem.Columns[1].HeaderText = (isVN ? "Kích cỡ (mm)" : "Size (mm)");
		gridGem.Columns[2].HeaderText = (isVN ? "S.Lượng" : "Qty");
		((Control)btnGemMap).Text = (isVN ? "Bản Đồ Đá" : "Gem Map");
		((Control)btnPrintGemMap).Text = (isVN ? "Chụp Ảnh" : "Snapshot");
		((Control)btnAnnotate).Text = (isVN ? "Ghi Chú" : "Annotate");
		((Control)btnExportPDF).Text = (isVN ? "Xuất PDF" : "Export PDF");
		((Control)lblModelCode).Text = (isVN ? "Mã" : "Code");
		((Control)btnRestoreGemMap).Text = (isVN ? "Trở về" : "Back");
		((Control)btnHideGemMap).Text = (isVN ? "Ẩn" : "Hide");
		string[] array = new string[5] { "Tất cả", "Vàng", "Trắng", "Hồng", "Platinum" };
		string[] array2 = new string[5] { "All", "Yellow", "White", "Rose", "Platinum" };
		string[] array3 = (isVN ? array : array2);
		for (int i = 0; i < ((ArrangedElementCollection)((Control)panelColor).Controls).Count && i < array3.Length; i++)
		{
			Control val = ((Control)panelColor).Controls[i];
			RadioButton val2 = (RadioButton)((val is RadioButton) ? val : null);
			if (val2 != null)
			{
				((Control)val2).Text = array3[i];
			}
		}
		lastScaledWidth = -1;
		for (int j = 0; j < gridGem.Rows.Count; j++)
		{
			if (((DataGridViewBand)gridGem.Rows[j]).Tag as string == "SUM")
			{
				gridGem.Rows[j].Cells[0].Value = (isVN ? "TỔNG" : "TOTAL");
				break;
			}
		}
		if (currentMode != AppMode.GemMap)
		{
			RefreshMetalGrid();
		}
		else
		{
			LayoutControls(((Form)this).ClientSize.Width, setFormSize: false);
		}
	}

	private void BtnPrintGemMap_Click(object sender, EventArgs e)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			SendKeys.SendWait("^%s");
		}
		catch (Exception ex)
		{
			MessageBox.Show("Could not invoke shortcut: " + ex.Message);
		}
	}

	private void GridGem_CellClick(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0 || e.RowIndex >= gridGem.Rows.Count || ((DataGridViewBand)gridGem.Rows[e.RowIndex]).Tag as string == "SUM")
		{
			return;
		}
		doc.Objects.UnselectAll(true);
		string toolTipText = gridGem.Rows[e.RowIndex].Cells[0].ToolTipText;
		string item = gridGem.Rows[e.RowIndex].Cells[1].Value.ToString();
		Tuple<string, string> key = new Tuple<string, string>(toolTipText, item);
		List<RhinoObject> value;
		if (gemGroups != null && gemGroups.TryGetValue(key, out value))
		{
			foreach (RhinoObject item2 in value)
			{
				if (item2 != null)
				{
					item2.Select(true);
				}
			}
		}
		doc.Views.Redraw();
	}

	private void GridGem_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0 || e.RowIndex >= gridGem.Rows.Count || ((DataGridViewBand)gridGem.Rows[e.RowIndex]).Tag as string == "SUM")
		{
			return;
		}
		doc.Objects.UnselectAll(true);
		string toolTipText = gridGem.Rows[e.RowIndex].Cells[0].ToolTipText;
		string item = gridGem.Rows[e.RowIndex].Cells[1].Value.ToString();
		Tuple<string, string> key = new Tuple<string, string>(toolTipText, item);
		List<RhinoObject> value;
		if (gemGroups != null && gemGroups.TryGetValue(key, out value))
		{
			foreach (RhinoObject item2 in value)
			{
				if (item2 != null)
				{
					item2.Select(true);
				}
			}
			RhinoApp.RunScript("_Zoom _Selected", false);
		}
		doc.Views.Redraw();
	}

	private void BtnAnnotate_Click(object sender, EventArgs e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0550: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_0497: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).Hide();
		try
		{
			GetPoint val = new GetPoint();
			((GetBaseClass)val).SetCommandPrompt(isVN ? "Chọn điểm đặt chữ ghi chú" : "Select insertion point for annotations");
			if ((int)val.Get() != 8)
			{
				return;
			}
			Point3d val2 = ((GetBaseClass)val).Point();
			Plane val3 = doc.Views.ActiveView.ActiveViewport.ConstructionPlane();
			double num = 2.0;
			double num2 = 1.0;
			int num3 = 0;
			int num4 = doc.Layers.Find("KPT_GhiChu", true);
			if (num4 >= 0)
			{
				RhinoObject[] array = doc.Objects.FindByLayer(doc.Layers[num4]);
				if (array != null)
				{
					RhinoObject[] array2 = array;
					foreach (RhinoObject val4 in array2)
					{
						if (val4 != null)
						{
							doc.Objects.Delete(val4, true);
						}
					}
				}
			}
			int num5 = doc.Layers.Find("KPT_GhiChu", true);
			if (num5 < 0)
			{
				num5 = doc.Layers.Add("KPT_GhiChu", Color.Black);
			}
			int result = 0;
			for (int j = 0; j < gridGem.Rows.Count; j++)
			{
				if (((DataGridViewBand)gridGem.Rows[j]).Tag as string == "SUM")
				{
					int.TryParse(gridGem.Rows[j].Cells[2].Value.ToString(), out result);
					continue;
				}
				string toolTipText = gridGem.Rows[j].Cells[0].ToolTipText;
				string text = gridGem.Rows[j].Cells[1].Value.ToString();
				string text2 = gridGem.Rows[j].Cells[2].Value.ToString();
				Color objectColor = ((gridGem.Rows[j].Cells[0].Tag != null) ? ((Color)gridGem.Rows[j].Cells[0].Tag) : Color.Black);
				string text3 = text + "  " + text2 + "v";
				Point3d val5 = (val3.Origin = val2 - val3.YAxis * (double)num3 * (num + num2));
				Point3d iconCenter = val5 - val3.XAxis * 3.0 + val3.YAxis * (num / 2.0);
				Guid guid = DrawGemShapeIcon(toolTipText, iconCenter, val3);
				if (guid != Guid.Empty)
				{
					RhinoObject val7 = doc.Objects.Find(guid);
					if (val7 != null)
					{
						val7.Attributes.LayerIndex = num5;
						val7.Attributes.ObjectColor = objectColor;
						val7.Attributes.ColorSource = (ObjectColorSource)1;
						val7.CommitChanges();
					}
				}
				Guid guid2 = doc.Objects.AddText(text3, val3, num, "Arial", true, false);
				RhinoObject val8 = doc.Objects.Find(guid2);
				if (val8 != null)
				{
					val8.Attributes.LayerIndex = num5;
					val8.Attributes.ObjectColor = objectColor;
					val8.Attributes.ColorSource = (ObjectColorSource)1;
					val8.CommitChanges();
				}
				num3++;
			}
			string text4 = (isVN ? "TỔNG  " + result + "v" : "TOTAL  " + result + " pcs");
			val3.Origin = val2 - val3.YAxis * (double)num3 * (num + num2);
			Guid guid3 = doc.Objects.AddText(text4, val3, num, "Arial", true, false);
			RhinoObject val9 = doc.Objects.Find(guid3);
			if (val9 != null)
			{
				val9.Attributes.LayerIndex = num5;
				val9.Attributes.ObjectColor = Color.Red;
				val9.Attributes.ColorSource = (ObjectColorSource)1;
				val9.CommitChanges();
			}
			string text5 = ((Control)txtModelCode).Text.Trim();
			if (!string.IsNullOrEmpty(text5))
			{
				num3++;
				val3.Origin = val2 - val3.YAxis * (double)num3 * (num + num2);
				Guid guid4 = doc.Objects.AddText(text5, val3, num, "Arial", true, false);
				RhinoObject val10 = doc.Objects.Find(guid4);
				if (val10 != null)
				{
					val10.Attributes.LayerIndex = num5;
					val10.Attributes.ObjectColor = Color.DarkOrange;
					val10.Attributes.ColorSource = (ObjectColorSource)1;
					val10.CommitChanges();
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(isVN ? ("Lỗi khi tạo ghi chú: " + ex.Message) : ("Error generating annotation: " + ex.Message));
		}
		finally
		{
			((Control)this).Show();
			doc.Views.Redraw();
		}
	}

	private Guid DrawGemShapeIcon(string shape, Point3d iconCenter, Plane plane)
	{
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_0507: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_0518: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0553: Unknown result type (might be due to invalid IL or missing references)
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0563: Unknown result type (might be due to invalid IL or missing references)
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		double num = 0.8;
		switch (shape)
		{
		case "Pearl":
		case "Round":
		{
			Plane val3 = plane;
			val3.Origin = iconCenter;
			Circle val4 = new Circle(val3, num);
			return doc.Objects.AddCircle(val4);
		}
		case "Princess":
		{
			Point3d[] array3 = new Point3d[5]
			{
				iconCenter - plane.XAxis * num - plane.YAxis * num,
				iconCenter + plane.XAxis * num - plane.YAxis * num,
				iconCenter + plane.XAxis * num + plane.YAxis * num,
				iconCenter - plane.XAxis * num + plane.YAxis * num,
				iconCenter - plane.XAxis * num - plane.YAxis * num
			};
			return doc.Objects.AddPolyline((IEnumerable<Point3d>)array3);
		}
		case "Baguette":
		case "Tapered Baguette":
		{
			Point3d[] array2 = new Point3d[5]
			{
				iconCenter - plane.XAxis * num * 1.2 - plane.YAxis * num * 0.6,
				iconCenter + plane.XAxis * num * 1.2 - plane.YAxis * num * 0.6,
				iconCenter + plane.XAxis * num * 1.2 + plane.YAxis * num * 0.6,
				iconCenter - plane.XAxis * num * 1.2 + plane.YAxis * num * 0.6,
				iconCenter - plane.XAxis * num * 1.2 - plane.YAxis * num * 0.6
			};
			return doc.Objects.AddPolyline((IEnumerable<Point3d>)array2);
		}
		case "Oval":
		case "Cushion":
		case "Emerald":
		case "Radiant":
		{
			Plane val = plane;
			val.Origin = iconCenter;
			Ellipse val2 = new Ellipse(val, num * 1.2, num * 0.7);
			return doc.Objects.AddCurve(val2.ToNurbsCurve());
		}
		default:
		{
			Point3d[] array = new Point3d[5]
			{
				iconCenter - plane.YAxis * num * 1.1,
				iconCenter + plane.XAxis * num,
				iconCenter + plane.YAxis * num * 1.1,
				iconCenter - plane.XAxis * num,
				iconCenter - plane.YAxis * num * 1.1
			};
			return doc.Objects.AddPolyline((IEnumerable<Point3d>)array);
		}
		}
	}

	private void BtnRestoreGemMap_Click(object sender, EventArgs e)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Guid, Color> originalColor in originalColors)
		{
			RhinoObject val = doc.Objects.Find(originalColor.Key);
			if (val != null)
			{
				val.Attributes.ObjectColor = originalColor.Value;
				if (originalSources.ContainsKey(originalColor.Key))
				{
					val.Attributes.ColorSource = originalSources[originalColor.Key];
				}
				val.CommitChanges();
			}
		}
		string[] array = new string[1] { "KPT_GhiChu" };
		foreach (string text in array)
		{
			int num = doc.Layers.Find(text, true);
			if (num < 0)
			{
				continue;
			}
			RhinoObject[] array2 = doc.Objects.FindByLayer(doc.Layers[num]);
			if (array2 != null)
			{
				RhinoObject[] array3 = array2;
				foreach (RhinoObject val2 in array3)
				{
					doc.Objects.Delete(val2, true);
				}
			}
		}
		doc.Views.Redraw();
	}

	private void BtnExportPDF_Click(object sender, EventArgs e)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Invalid comparison between Unknown and I4
		SaveFileDialog val = new SaveFileDialog();
		((FileDialog)val).Filter = "PDF Files (*.pdf)|*.pdf";
		string text = ((Control)txtModelCode).Text.Trim();
		((FileDialog)val).FileName = (isVN ? ("Bao_Cao_Da_" + (string.IsNullOrEmpty(text) ? "KPT" : text) + ".pdf") : ("Gem_Report_" + (string.IsNullOrEmpty(text) ? "KPT" : text) + ".pdf"));
		((FileDialog)val).Title = (isVN ? "Lưu báo cáo PDF 3D" : "Save 3D PDF Report");
		if ((int)((CommonDialog)val).ShowDialog() == 1)
		{
			pdfExportPath = ((FileDialog)val).FileName;
			((Control)this).Hide();
			RhinoApp.Idle += RhinoApp_Idle_ExportPDF;
		}
	}

	private void RhinoApp_Idle_ExportPDF(object sender, EventArgs e)
	{
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected O, but got Unknown
		//IL_0913: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Expected O, but got Unknown
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Expected O, but got Unknown
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Expected O, but got Unknown
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Expected O, but got Unknown
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Expected O, but got Unknown
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Expected O, but got Unknown
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Expected O, but got Unknown
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Expected O, but got Unknown
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Expected O, but got Unknown
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Expected O, but got Unknown
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Expected O, but got Unknown
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Expected O, but got Unknown
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Expected O, but got Unknown
		//IL_056b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Expected O, but got Unknown
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0577: Expected O, but got Unknown
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Expected O, but got Unknown
		//IL_05a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b2: Expected O, but got Unknown
		//IL_05ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b4: Expected O, but got Unknown
		//IL_05e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Expected O, but got Unknown
		//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f1: Expected O, but got Unknown
		//IL_0700: Unknown result type (might be due to invalid IL or missing references)
		//IL_070a: Expected O, but got Unknown
		//IL_0705: Unknown result type (might be due to invalid IL or missing references)
		//IL_070c: Expected O, but got Unknown
		//IL_0722: Unknown result type (might be due to invalid IL or missing references)
		//IL_072c: Expected O, but got Unknown
		//IL_0727: Unknown result type (might be due to invalid IL or missing references)
		//IL_072e: Expected O, but got Unknown
		//IL_07c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cf: Expected O, but got Unknown
		//IL_07ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d1: Expected O, but got Unknown
		//IL_075d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0767: Expected O, but got Unknown
		//IL_0762: Unknown result type (might be due to invalid IL or missing references)
		//IL_0769: Expected O, but got Unknown
		//IL_080d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0817: Expected O, but got Unknown
		//IL_0812: Unknown result type (might be due to invalid IL or missing references)
		//IL_0819: Expected O, but got Unknown
		//IL_087b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0882: Expected O, but got Unknown
		//IL_08e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e6: Invalid comparison between Unknown and I4
		RhinoApp.Idle -= RhinoApp_Idle_ExportPDF;
		if (string.IsNullOrEmpty(pdfExportPath))
		{
			((Control)this).Show();
			return;
		}
		string text = pdfExportPath;
		pdfExportPath = "";
		string text2 = Path.Combine(Path.GetTempPath(), "KPT_TempModel.u3d");
		string text3 = Path.Combine(Path.GetTempPath(), "KPT_TempPoster.png");
		bool flag = false;
		bool flag2 = false;
		try
		{
			doc.Objects.UnselectAll(true);
			foreach (RhinoObject @object in doc.Objects)
			{
				if (@object != null && !@object.IsDeleted && @object.Attributes.Visible)
				{
					@object.Select(true);
				}
			}
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			if (File.Exists(text3))
			{
				File.Delete(text3);
			}
			flag = RhinoApp.RunScript("_-Export \"" + text2 + "\" _Enter", false);
			try
			{
				Bitmap val = doc.Views.ActiveView.CaptureToBitmap(new Size(800, 600));
				try
				{
					if (val != null)
					{
						((Image)val).Save(text3, ImageFormat.Png);
						flag2 = true;
					}
				}
				finally
				{
					if (val != null) ((IDisposable)val).Dispose();
				}
			}
			catch (Exception ex)
			{
				RhinoApp.WriteLine("Capture error: " + ex.Message);
			}
		}
		catch (Exception ex2)
		{
			RhinoApp.WriteLine("Export prep error: " + ex2.Message);
		}
		finally
		{
			doc.Objects.UnselectAll(true);
			doc.Views.Redraw();
		}
		try
		{
			string text4 = ((Control)txtModelCode).Text.Trim();
			Document val2 = new Document(PageSize.A4, 36f, 36f, 36f, 36f);
			using (FileStream fileStream = new FileStream(text, FileMode.Create))
			{
				PdfWriter instance = PdfWriter.GetInstance(val2, (Stream)fileStream);
				val2.Open();
				PdfFont font = FontFactory.GetFont("Arial", 16f, 1, BaseColor.BLACK);
				Paragraph val3 = new Paragraph(isVN ? "TIỆM VÀNG KIM PHONG THẢO" : "KIM PHONG THAO JEWELRY", font);
				val3.Alignment = 1;
				val3.SpacingAfter = 5f;
				val2.Add((IElement)(object)val3);
				PdfFont font2 = FontFactory.GetFont("Arial", 9f, 0, BaseColor.DARK_GRAY);
				Paragraph val4 = new Paragraph("Website: kimphongthao.com   |   Email: contact@kimphongthao.com", font2);
				val4.Alignment = 1;
				val4.SpacingAfter = 15f;
				val2.Add((IElement)(object)val4);
				PdfFont font3 = FontFactory.GetFont("Arial", 11f, 1, BaseColor.ORANGE);
				Paragraph val5 = new Paragraph(isVN ? ("MÃ MẪU: " + (string.IsNullOrEmpty(text4) ? "KPT" : text4)) : ("MODEL CODE: " + (string.IsNullOrEmpty(text4) ? "KPT" : text4)), font3);
				val5.Alignment = 0;
				val5.SpacingAfter = 10f;
				val2.Add((IElement)(object)val5);
				if (flag2 && File.Exists(text3))
				{
					PdfImage instance2 = PdfImage.GetInstance(text3);
					instance2.ScaleToFit(400f, 300f);
					instance2.Alignment = 1;
					if (flag && File.Exists(text2))
					{
						PdfStream val6 = new PdfStream(File.ReadAllBytes(text2));
						((PdfDictionary)val6).Put(PdfName.TYPE, (PdfObject)new PdfName("3D"));
						((PdfDictionary)val6).Put(PdfName.SUBTYPE, (PdfObject)new PdfName("U3D"));
						PdfIndirectReference indirectReference = instance.AddToBody((PdfObject)(object)val6).IndirectReference;
						PdfRectangle val7 = new PdfRectangle(120f, 400f, 480f, 670f);
						PdfAnnotation val8 = new PdfAnnotation(instance, val7);
						((PdfDictionary)val8).Put(PdfName.SUBTYPE, (PdfObject)new PdfName("3D"));
						((PdfDictionary)val8).Put(new PdfName("3DD"), (PdfObject)(object)indirectReference);
						PdfAppearance val9 = instance.DirectContent.CreateAppearance(360f, 270f);
						((PdfContentByte)val9).AddImage(instance2, 360f, 0f, 0f, 270f, 0f, 0f);
						val8.SetAppearance(PdfName.N, (PdfTemplate)(object)val9);
						instance.AddAnnotation(val8);
						PdfFont font4 = FontFactory.GetFont("Arial", 8f, 2, BaseColor.LIGHT_GRAY);
						Paragraph val10 = new Paragraph(isVN ? "(Nhấp chuột vào hình trên để xoay lật 3D tương tác / Click to rotate 3D)" : "(Click on the image above to rotate in 3D)", font4);
						val10.Alignment = 1;
						val10.SpacingAfter = 15f;
						val2.Add((IElement)(object)val10);
					}
					else
					{
						instance2.SpacingAfter = 15f;
						val2.Add((IElement)(object)instance2);
					}
				}
				Paragraph val11 = new Paragraph("----------------------------------------------------------------------------------------------------------------", font2);
				val11.SpacingAfter = 10f;
				val2.Add((IElement)(object)val11);
				Paragraph val12 = new Paragraph(isVN ? "BẢNG THỐNG KÊ CHI TIẾT ĐÁ:" : "GEMSTONE REPORT DETAILS:", FontFactory.GetFont("Arial", 10f, 1, BaseColor.BLACK));
				val12.SpacingAfter = 5f;
				val2.Add((IElement)(object)val12);
				PdfPTable val13 = new PdfPTable(3);
				val13.WidthPercentage = 100f;
				val13.SetWidths(new float[3] { 40f, 35f, 25f });
				PdfFont font5 = FontFactory.GetFont("Arial", 10f, 1, BaseColor.WHITE);
				BaseColor backgroundColor = new BaseColor(40, 40, 40);
				PdfPCell val14 = new PdfPCell(new Phrase(isVN ? "Kiểu đá" : "Shape", font5));
				val14.BackgroundColor = backgroundColor;
				val14.HorizontalAlignment = 1;
				val13.AddCell(val14);
				PdfPCell val15 = new PdfPCell(new Phrase(isVN ? "Kích cỡ (mm)" : "Size (mm)", font5));
				val15.BackgroundColor = backgroundColor;
				val15.HorizontalAlignment = 1;
				val13.AddCell(val15);
				PdfPCell val16 = new PdfPCell(new Phrase(isVN ? "S.Lượng" : "Qty", font5));
				val16.BackgroundColor = backgroundColor;
				val16.HorizontalAlignment = 1;
				val13.AddCell(val16);
				PdfFont font6 = FontFactory.GetFont("Arial", 10f, 0, BaseColor.BLACK);
				int result = 0;
				for (int i = 0; i < gridGem.Rows.Count; i++)
				{
					if (((DataGridViewBand)gridGem.Rows[i]).Tag as string == "SUM")
					{
						int.TryParse(gridGem.Rows[i].Cells[2].Value.ToString(), out result);
						continue;
					}
					string toolTipText = gridGem.Rows[i].Cells[0].ToolTipText;
					string text5 = gridGem.Rows[i].Cells[1].Value.ToString();
					string text6 = gridGem.Rows[i].Cells[2].Value.ToString();
					PdfPCell val17 = new PdfPCell(new Phrase(toolTipText, font6));
					val17.HorizontalAlignment = 1;
					val13.AddCell(val17);
					PdfPCell val18 = new PdfPCell(new Phrase(text5, font6));
					val18.HorizontalAlignment = 1;
					val13.AddCell(val18);
					PdfPCell val19 = new PdfPCell(new Phrase(text6 + (isVN ? " v" : ""), font6));
					val19.HorizontalAlignment = 1;
					val13.AddCell(val19);
				}
				PdfFont font7 = FontFactory.GetFont("Arial", 10f, 1, BaseColor.RED);
				PdfPCell val20 = new PdfPCell(new Phrase(isVN ? "TỔNG SỐ LƯỢNG ĐÁ" : "TOTAL GEMS QTY", font7));
				val20.Colspan = 2;
				val20.HorizontalAlignment = 2;
				val13.AddCell(val20);
				PdfPCell val21 = new PdfPCell(new Phrase(result + (isVN ? " viên" : " pcs"), font7));
				val21.HorizontalAlignment = 1;
				val13.AddCell(val21);
				val2.Add((IElement)(object)val13);
				Paragraph val22 = new Paragraph(isVN ? ("Ngày xuất bản vẽ: " + DateTime.Now.ToString("dd/MM/yyyy")) : ("Report generated: " + DateTime.Now.ToString("dd/MM/yyyy")), font2);
				val22.SpacingBefore = 20f;
				val22.Alignment = 0;
				val2.Add((IElement)(object)val22);
				val2.Close();
			}
			if ((int)MessageBox.Show(isVN ? "Xuất báo cáo PDF thành công! Bạn có muốn mở tệp ngay không?" : "PDF Report exported successfully! Do you want to open it now?", isVN ? "Thành công" : "Success", (MessageBoxButtons)4, (MessageBoxIcon)64) == 6)
			{
				Process.Start(text);
			}
		}
		catch (Exception ex3)
		{
			MessageBox.Show((isVN ? "Lỗi khi xuất PDF: " : "Error exporting PDF: ") + ex3.Message);
		}
		finally
		{
			((Control)this).Show();
			try
			{
				if (File.Exists(text2))
				{
					File.Delete(text2);
				}
				if (File.Exists(text3))
				{
					File.Delete(text3);
				}
			}
			catch
			{
			}
		}
	}

	private void BtnGemMap_Click(object sender, EventArgs e)
	{
		PerformGemMapPreview();
	}

	public void PerformGemMapPreview()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_0c13: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c18: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cb2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cb7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ccd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ccf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_054a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0565: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0971: Unknown result type (might be due to invalid IL or missing references)
		//IL_0976: Unknown result type (might be due to invalid IL or missing references)
		//IL_097b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0980: Unknown result type (might be due to invalid IL or missing references)
		//IL_098b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0996: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dc: Expected O, but got Unknown
		//IL_0638: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Expected O, but got Unknown
		//IL_05fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_065a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0676: Unknown result type (might be due to invalid IL or missing references)
		((Control)btnGemMap).Enabled = false;
		ObjectEnumeratorSettings val = new ObjectEnumeratorSettings();
		val.NormalObjects = true;
		val.LockedObjects = true;
		val.HiddenObjects = false;
		val.ObjectTypeFilter = (ObjectType)4096;
		IEnumerable<RhinoObject> objectList = doc.Objects.GetObjectList(val);
		Dictionary<Tuple<string, string>, List<RhinoObject>> dictionary = new Dictionary<Tuple<string, string>, List<RhinoObject>>();
		RTree val2 = new RTree();
		Dictionary<Guid, BoundingBox> dictionary2 = new Dictionary<Guid, BoundingBox>();
		Dictionary<Guid, Tuple<double, double>> dictionary3 = new Dictionary<Guid, Tuple<double, double>>();
		Point3d val3 = default(Point3d);
		Point3d val4 = default(Point3d);
		Point3d val5 = default(Point3d);
		foreach (RhinoObject item3 in objectList)
		{
			if (item3 == null)
			{
				continue;
			}
			InstanceObject val6 = (InstanceObject)((item3 is InstanceObject) ? item3 : null);
			if (val6 == null)
			{
				continue;
			}
			string text = val6.InstanceDefinition.Name.ToLower();
			if (text.Contains("cutter") || text.Contains("prong") || text.Contains("chấu") || text.Contains("ngàm") || text.Contains("wire") || text.Contains("cater") || text.Contains("drill"))
			{
				continue;
			}
			bool num = text.Contains("gem") || text.Contains("stone") || text.Contains("diamond") || text.Contains("đá") || text.Contains("ruby") || text.Contains("sapphire") || text.Contains("emerald") || text.Contains("cz") || text.Contains("pearl");
			string text2 = "";
			if (text.Contains("oval"))
			{
				text2 = "Oval";
			}
			else if (text.Contains("pearl"))
			{
				text2 = "Pearl";
			}
			else if (text.Contains("pear"))
			{
				text2 = "Pear";
			}
			else if (text.Contains("emerald"))
			{
				text2 = "Emerald";
			}
			else if (text.Contains("cushion"))
			{
				text2 = "Cushion";
			}
			else if (text.Contains("princess"))
			{
				text2 = "Princess";
			}
			else if (text.Contains("marquise"))
			{
				text2 = "Marquise";
			}
			else if (text.Contains("heart"))
			{
				text2 = "Heart";
			}
			else if (text.Contains("trillion") || text.Contains("trili"))
			{
				text2 = "Trillion";
			}
			else if (text.Contains("radiant"))
			{
				text2 = "Radiant";
			}
			else if (text.Contains("asscher"))
			{
				text2 = "Asscher";
			}
			else if (text.Contains("tapered") || text.Contains("taper"))
			{
				text2 = "Tapered Baguette";
			}
			else if (text.Contains("baguette"))
			{
				text2 = "Baguette";
			}
			else if (text.Contains("round"))
			{
				text2 = "Round";
			}
			if (!num && string.IsNullOrEmpty(text2))
			{
				continue;
			}
			if (string.IsNullOrEmpty(text2))
			{
				text2 = "Round";
			}
			string text3 = "";
			double num2 = 1.0;
			double num3 = 1.0;
			Guid id = val6.InstanceDefinition.Id;
			BoundingBox value;
			if (!dictionary2.TryGetValue(id, out value))
			{
				value = BoundingBox.Empty;
				RhinoObject[] objects = val6.InstanceDefinition.GetObjects();
				for (int i = 0; i < objects.Length; i++)
				{
					BoundingBox boundingBox = objects[i].Geometry.GetBoundingBox(Transform.Identity);
					if (value.IsValid)
					{
						value.Union(boundingBox);
					}
					else
					{
						value = boundingBox;
					}
				}
				dictionary2[id] = value;
			}
			Point3d val7 = value.Max;
			double x = val7.X;
			val7 = value.Min;
			double num4 = x - val7.X;
			val7 = value.Max;
			double y = val7.Y;
			val7 = value.Min;
			double num5 = y - val7.Y;
			val7 = value.Max;
			// val7.Z;
			val7 = value.Min;
			// val7.Z;
			val7 = value.Min;
			double x2 = val7.X;
			val7 = value.Min;
			double y2 = val7.Y;
			val7 = value.Min;
			val3 = new Point3d(x2, y2, val7.Z);
			val7 = value.Max;
			double x3 = val7.X;
			val7 = value.Min;
			double y3 = val7.Y;
			val7 = value.Min;
			val4 = new Point3d(x3, y3, val7.Z);
			val7 = value.Min;
			double x4 = val7.X;
			val7 = value.Max;
			double y4 = val7.Y;
			val7 = value.Min;
			val5 = new Point3d(x4, y4, val7.Z);
			val3.Transform(val6.InstanceXform);
			val4.Transform(val6.InstanceXform);
			val5.Transform(val6.InstanceXform);
			if (num4 > 0.0)
			{
				num2 = val3.DistanceTo(val4) / num4;
			}
			if (num5 > 0.0)
			{
				num3 = val3.DistanceTo(val5) / num5;
			}
			if (text2 == "Tapered Baguette")
			{
				Tuple<double, double> value2;
			if (!dictionary3.TryGetValue(id, out value2))
				{
					double item = num4;
					double item2 = num4;
					List<Point3d> list = new List<Point3d>();
					RhinoObject[] objects = val6.InstanceDefinition.GetObjects();
					foreach (RhinoObject val8 in objects)
					{
						GeometryBase geometry = val8.Geometry;
						Brep val9 = (Brep)((geometry is Brep) ? geometry : null);
						if (val9 != null)
						{
							foreach (BrepVertex vertex in val9.Vertices)
							{
								list.Add(vertex.Location);
							}
							continue;
						}
						GeometryBase geometry2 = val8.Geometry;
						Mesh val10 = (Mesh)((geometry2 is Mesh) ? geometry2 : null);
						if (val10 == null)
						{
							continue;
						}
						foreach (Point3f vertex2 in val10.Vertices)
						{
							Point3f current3 = vertex2;
							list.Add(new Point3d(current3.X, current3.Y, current3.Z));
						}
					}
					if (list.Count > 0)
					{
						double maxY = list.Max((Point3d p) => p.Y);
						double minY = list.Min((Point3d p) => p.Y);
						List<Point3d> list2 = list.Where((Point3d p) => p.Y >= maxY - 0.01).ToList();
						List<Point3d> list3 = list.Where((Point3d p) => p.Y <= minY + 0.01).ToList();
						if (list2.Count > 0)
						{
							item = list2.Max((Point3d p) => p.X) - list2.Min((Point3d p) => p.X);
						}
						if (list3.Count > 0)
						{
							item2 = list3.Max((Point3d p) => p.X) - list3.Min((Point3d p) => p.X);
						}
					}
					Tuple<double, double> tuple = (dictionary3[id] = new Tuple<double, double>(item, item2));
					value2 = tuple;
				}
				double val11 = Math.Round(Math.Min(value2.Item1, value2.Item2) * num2, 2);
				double val12 = Math.Round(Math.Max(value2.Item1, value2.Item2) * num2, 2);
				double val13 = Math.Round(num5 * num3, 2);
				text3 = FormatGemSize(val11) + "x" + FormatGemSize(val12) + "x" + FormatGemSize(val13);
			}
			if (string.IsNullOrEmpty(text3))
			{
				double val14 = Math.Round(num4 * num2, 2);
				double val15 = Math.Round(num5 * num3, 2);
				double num6 = Math.Min(val14, val15);
				double num7 = Math.Max(val14, val15);
				switch (text2)
				{
				default:
					text3 = ((!(Math.Abs(num7 - num6) < 0.05)) ? (FormatGemSize(num6) + "x" + FormatGemSize(num7)) : FormatGemSize(num7));
					break;
				case "Round":
				case "Princess":
				case "Asscher":
				case "Pearl":
					text3 = FormatGemSize(num7);
					break;
				}
			}
			Tuple<string, string> key = new Tuple<string, string>(text2, text3);
			if (!dictionary.ContainsKey(key))
			{
				dictionary[key] = new List<RhinoObject>();
			}
			Point3d val16 = val6.InstanceXform * Point3d.Origin;
			bool isDuplicate = false;
			val2.Search(new Sphere(val16, 0.05), (EventHandler<RTreeEventArgs>)delegate
			{
				isDuplicate = true;
			});
			if (!isDuplicate)
			{
				dictionary[key].Add(item3);
				val2.Insert(val16, 0);
			}
		}
		if (dictionary.Count == 0)
		{
			string obj = (isVN ? "Không tìm thấy Block Đá nào." : "No gem blocks found.");
			string text4 = (isVN ? "Thông báo" : "Notice");
			MessageBox.Show(obj, text4, (MessageBoxButtons)0, (MessageBoxIcon)64);
			((Control)btnGemMap).Enabled = true;
			return;
		}
		Color[] array = new Color[30]
		{
			Color.Red,
			Color.Cyan,
			Color.Yellow,
			Color.Lime,
			Color.Fuchsia,
			Color.Orange,
			Color.DeepSkyBlue,
			Color.SpringGreen,
			Color.Pink,
			Color.Goldenrod,
			Color.DarkTurquoise,
			Color.MediumOrchid,
			Color.Crimson,
			Color.GreenYellow,
			Color.DodgerBlue,
			Color.Chartreuse,
			Color.BlueViolet,
			Color.Tomato,
			Color.LightSeaGreen,
			Color.MediumPurple,
			Color.OrangeRed,
			Color.Aquamarine,
			Color.HotPink,
			Color.Gold,
			Color.Violet,
			Color.YellowGreen,
			Color.CornflowerBlue,
			Color.Salmon,
			Color.PaleGreen,
			Color.MediumVioletRed
		};
		int num8 = 0;
		List<Tuple<string, string, int, Color>> list4 = new List<Tuple<string, string, int, Color>>();
		foreach (Tuple<string, string> item4 in dictionary.Keys.OrderByDescending((Tuple<string, string> tuple3) => tuple3.Item2))
		{
			Color color = array[num8 % array.Length];
			BoundingBox val17 = BoundingBox.Empty;
			foreach (RhinoObject item5 in dictionary[item4])
			{
				if (!originalColors.ContainsKey(item5.Id))
				{
					originalColors[item5.Id] = item5.Attributes.ObjectColor;
					originalSources[item5.Id] = item5.Attributes.ColorSource;
				}
				item5.Attributes.ObjectColor = color;
				item5.Attributes.ColorSource = (ObjectColorSource)1;
				item5.CommitChanges();
				BoundingBox boundingBox2 = item5.Geometry.GetBoundingBox(true);
				if (val17.IsValid)
				{
					val17.Union(boundingBox2);
				}
				else
				{
					val17 = boundingBox2;
				}
			}
			list4.Add(new Tuple<string, string, int, Color>(item4.Item1, item4.Item2, dictionary[item4].Count, color));
			num8++;
		}
		gemGroups = dictionary;
		doc.Views.Redraw();
		PopulateGemData(list4);
		AdjustFormSize(gemExpanded: true);
	}

	private void PopulateGemData(List<Tuple<string, string, int, Color>> data)
	{
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected O, but got Unknown
		gridGem.Rows.Clear();
		gridGem.RowTemplate.Height = 40;
		int num = 0;
		foreach (Tuple<string, string, int, Color> datum in data)
		{
			int num2 = gridGem.Rows.Add(new object[3]
			{
				"",
				datum.Item2,
				datum.Item3.ToString()
			});
			gridGem.Rows[num2].Cells[0].Tag = datum.Item4;
			gridGem.Rows[num2].Cells[0].ToolTipText = datum.Item1;
			num += datum.Item3;
		}
		string text = (isVN ? "TỔNG" : "TOTAL");
		int num3 = gridGem.Rows.Add(new object[3]
		{
			text,
			"",
			num.ToString()
		});
		((DataGridViewBand)gridGem.Rows[num3]).Tag = "SUM";
		((DataGridViewBand)gridGem.Rows[num3]).DefaultCellStyle.Font = new Font("Segoe UI", 13f, (FontStyle)1);
		((DataGridViewBand)gridGem.Rows[num3]).DefaultCellStyle.ForeColor = Color.Red;
		((DataGridViewBand)gridGem.Rows[num3]).DefaultCellStyle.Alignment = (DataGridViewContentAlignment)32;
	}

	private static string FormatGemSize(double val)
	{
		double num = Math.Round(val, 2);
		if (Math.Abs(num - Math.Round(num, 1)) < 0.005)
		{
			return num.ToString("F1");
		}
		return num.ToString("F2");
	}

	private void GridGem_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_0ecf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef3: Expected O, but got Unknown
		//IL_0701: Unknown result type (might be due to invalid IL or missing references)
		//IL_072b: Expected O, but got Unknown
		//IL_0d93: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d9a: Expected O, but got Unknown
		//IL_059f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ab: Expected O, but got Unknown
		//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Expected O, but got Unknown
		//IL_0ae5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aec: Expected O, but got Unknown
		//IL_081a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0826: Expected O, but got Unknown
		//IL_0841: Unknown result type (might be due to invalid IL or missing references)
		//IL_0848: Expected O, but got Unknown
		//IL_0ebf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ecb: Expected O, but got Unknown
		//IL_0c4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c52: Expected O, but got Unknown
		//IL_0e26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e32: Expected O, but got Unknown
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Expected O, but got Unknown
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Expected O, but got Unknown
		//IL_0b65: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b71: Expected O, but got Unknown
		//IL_0a60: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6c: Expected O, but got Unknown
		//IL_0a87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8e: Expected O, but got Unknown
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Expected O, but got Unknown
		//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fa: Expected O, but got Unknown
		//IL_0cd2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cde: Expected O, but got Unknown
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Expected O, but got Unknown
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Expected O, but got Unknown
		//IL_0d56: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d5d: Expected O, but got Unknown
		//IL_0b84: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b8b: Expected O, but got Unknown
		//IL_0d74: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d80: Expected O, but got Unknown
		//IL_0c2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c38: Expected O, but got Unknown
		if (e.RowIndex < 0 || e.ColumnIndex != 0)
		{
			return;
		}
		((HandledEventArgs)(object)e).Handled = true;
		e.PaintBackground(e.CellBounds, true);
		string text = e.Value as string;
		if (((DataGridViewBand)gridGem.Rows[e.RowIndex]).Tag as string == "SUM")
		{
			e.PaintContent(e.CellBounds);
			return;
		}
		string text2 = gridGem.Rows[e.RowIndex].Cells[0].ToolTipText;
		if (string.IsNullOrEmpty(text2))
		{
			text2 = text;
		}
		Color obj = ((gridGem.Rows[e.RowIndex].Cells[0].Tag != null) ? ((Color)gridGem.Rows[e.RowIndex].Cells[0].Tag) : Color.Black);
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = (SmoothingMode)4;
		SolidBrush val = new SolidBrush(obj);
		try
		{
			Rectangle cellBounds = e.CellBounds;
			cellBounds.Inflate(-8, -8);
			float num = (float)cellBounds.X + (float)cellBounds.Width / 2f;
			float num2 = (float)cellBounds.Y + (float)cellBounds.Height / 2f;
			float num3 = (float)Math.Min(cellBounds.Width, cellBounds.Height) / 2f - 2f;
			switch (text2)
			{
			case "Round":
				graphics.FillEllipse((Brush)val, num - num3, num2 - num3, num3 * 2f, num3 * 2f);
				break;
			case "Pearl":
			{
				graphics.FillEllipse((Brush)val, num - num3, num2 - num3, num3 * 2f, num3 * 2f);
				SolidBrush val10 = new SolidBrush(Color.FromArgb(90, 255, 255, 255));
				try
				{
					graphics.FillEllipse((Brush)val10, num - num3 * 0.45f, num2 - num3 * 0.65f, num3 * 0.7f, num3 * 0.5f);
					break;
				}
				finally
				{
					if (val10 != null) ((IDisposable)val10).Dispose();
				}
			}
			case "Princess":
			{
				PointF[] array9 = new PointF[4]
				{
					new PointF(num, num2 - num3 * 1.1f),
					new PointF(num + num3 * 1.1f, num2),
					new PointF(num, num2 + num3 * 1.1f),
					new PointF(num - num3 * 1.1f, num2)
				};
				graphics.FillPolygon((Brush)val, array9);
				break;
			}
			case "Asscher":
			{
				float num9 = num3 * 0.38f;
				PointF[] array7 = new PointF[8]
				{
					new PointF(num - num3 + num9, num2 - num3),
					new PointF(num + num3 - num9, num2 - num3),
					new PointF(num + num3, num2 - num3 + num9),
					new PointF(num + num3, num2 + num3 - num9),
					new PointF(num + num3 - num9, num2 + num3),
					new PointF(num - num3 + num9, num2 + num3),
					new PointF(num - num3, num2 + num3 - num9),
					new PointF(num - num3, num2 - num3 + num9)
				};
				graphics.FillPolygon((Brush)val, array7);
				Pen val9 = new Pen(Color.FromArgb(60, 255, 255, 255), 1f);
				try
				{
					float num10 = num3 * 0.55f;
					float num11 = num10 * 0.38f;
					PointF[] array8 = new PointF[8]
					{
						new PointF(num - num10 + num11, num2 - num10),
						new PointF(num + num10 - num11, num2 - num10),
						new PointF(num + num10, num2 - num10 + num11),
						new PointF(num + num10, num2 + num10 - num11),
						new PointF(num + num10 - num11, num2 + num10),
						new PointF(num - num10 + num11, num2 + num10),
						new PointF(num - num10, num2 + num10 - num11),
						new PointF(num - num10, num2 - num10 + num11)
					};
					graphics.DrawPolygon(val9, array8);
					break;
				}
				finally
				{
					if (val9 != null) ((IDisposable)val9).Dispose();
				}
			}
			case "Baguette":
				graphics.FillRectangle((Brush)val, num - num3 * 0.45f, num2 - num3, num3 * 0.9f, num3 * 2f);
				break;
			case "Oval":
				graphics.FillEllipse((Brush)val, num - num3, num2 - num3 * 0.7f, num3 * 2f, num3 * 1.4f);
				break;
			case "Emerald":
			{
				float num5 = num3;
				float num6 = num3 * 0.72f;
				float num7 = num3 * 0.22f;
				PointF[] array5 = new PointF[8]
				{
					new PointF(num - num5 + num7, num2 - num6),
					new PointF(num + num5 - num7, num2 - num6),
					new PointF(num + num5, num2 - num6 + num7),
					new PointF(num + num5, num2 + num6 - num7),
					new PointF(num + num5 - num7, num2 + num6),
					new PointF(num - num5 + num7, num2 + num6),
					new PointF(num - num5, num2 + num6 - num7),
					new PointF(num - num5, num2 - num6 + num7)
				};
				graphics.FillPolygon((Brush)val, array5);
				Pen val8 = new Pen(Color.FromArgb(50, 255, 255, 255), 1f);
				try
				{
					float num8 = 0.6f;
					PointF[] array6 = new PointF[8]
					{
						new PointF(num - num5 * num8 + num7 * num8, num2 - num6 * num8),
						new PointF(num + num5 * num8 - num7 * num8, num2 - num6 * num8),
						new PointF(num + num5 * num8, num2 - num6 * num8 + num7 * num8),
						new PointF(num + num5 * num8, num2 + num6 * num8 - num7 * num8),
						new PointF(num + num5 * num8 - num7 * num8, num2 + num6 * num8),
						new PointF(num - num5 * num8 + num7 * num8, num2 + num6 * num8),
						new PointF(num - num5 * num8, num2 + num6 * num8 - num7 * num8),
						new PointF(num - num5 * num8, num2 - num6 * num8 + num7 * num8)
					};
					graphics.DrawPolygon(val8, array6);
					break;
				}
				finally
				{
					if (val8 != null) ((IDisposable)val8).Dispose();
				}
			}
			case "Radiant":
			{
				float num4 = num3 * 0.18f;
				PointF[] array4 = new PointF[8]
				{
					new PointF(num - num3 + num4, num2 - num3),
					new PointF(num + num3 - num4, num2 - num3),
					new PointF(num + num3, num2 - num3 + num4),
					new PointF(num + num3, num2 + num3 - num4),
					new PointF(num + num3 - num4, num2 + num3),
					new PointF(num - num3 + num4, num2 + num3),
					new PointF(num - num3, num2 + num3 - num4),
					new PointF(num - num3, num2 - num3 + num4)
				};
				graphics.FillPolygon((Brush)val, array4);
				Pen val7 = new Pen(Color.FromArgb(50, 255, 255, 255), 1f);
				try
				{
					graphics.DrawLine(val7, num, num2 - num3 * 0.9f, num, num2 + num3 * 0.9f);
					graphics.DrawLine(val7, num - num3 * 0.9f, num2, num + num3 * 0.9f, num2);
					break;
				}
				finally
				{
					if (val7 != null) ((IDisposable)val7).Dispose();
				}
			}
			case "Cushion":
			{
				GraphicsPath val6 = new GraphicsPath();
				try
				{
					val6.AddArc(num - num3, num2 - num3, num3, num3, 180f, 90f);
					val6.AddArc(num, num2 - num3, num3, num3, 270f, 90f);
					val6.AddArc(num, num2, num3, num3, 0f, 90f);
					val6.AddArc(num - num3, num2, num3, num3, 90f, 90f);
					val6.CloseFigure();
					graphics.FillPath((Brush)val, val6);
					break;
				}
				finally
				{
					if (val6 != null) ((IDisposable)val6).Dispose();
				}
			}
			case "Marquise":
			{
				GraphicsPath val5 = new GraphicsPath();
				try
				{
					val5.AddCurve(new PointF[3]
					{
						new PointF(num - num3, num2),
						new PointF(num, num2 - num3 / 1.5f),
						new PointF(num + num3, num2)
					});
					val5.AddCurve(new PointF[3]
					{
						new PointF(num + num3, num2),
						new PointF(num, num2 + num3 / 1.5f),
						new PointF(num - num3, num2)
					});
					val5.CloseFigure();
					graphics.FillPath((Brush)val, val5);
					break;
				}
				finally
				{
					if (val5 != null) ((IDisposable)val5).Dispose();
				}
			}
			case "Pear":
			{
				GraphicsPath val4 = new GraphicsPath();
				try
				{
					val4.AddArc(num - num3, num2, num3 * 2f, num3, 0f, 180f);
					PointF[] array3 = new PointF[3]
					{
						new PointF(num - num3, num2),
						new PointF(num, num2 - num3 * 1.5f),
						new PointF(num + num3, num2)
					};
					val4.AddCurve(array3, 0.3f);
					val4.CloseFigure();
					graphics.FillPath((Brush)val, val4);
					break;
				}
				finally
				{
					if (val4 != null) ((IDisposable)val4).Dispose();
				}
			}
			case "Trillion":
			{
				PointF[] array2 = new PointF[3]
				{
					new PointF(num, num2 - num3 * 1.2f),
					new PointF(num + num3 * 1.1f, num2 + num3 / 1.5f),
					new PointF(num - num3 * 1.1f, num2 + num3 / 1.5f)
				};
				GraphicsPath val3 = new GraphicsPath();
				try
				{
					val3.AddCurve(array2, 0.1f);
					val3.CloseFigure();
					graphics.FillPath((Brush)val, val3);
					break;
				}
				finally
				{
					if (val3 != null) ((IDisposable)val3).Dispose();
				}
			}
			case "Heart":
			{
				GraphicsPath val2 = new GraphicsPath();
				try
				{
					val2.AddArc(num - num3, num2 - num3, num3, num3, 135f, 225f);
					val2.AddArc(num, num2 - num3, num3, num3, 180f, 225f);
					val2.AddLine(num + num3 * 0.8f, num2 - 0.2f * num3, num, num2 + num3);
					val2.AddLine(num, num2 + num3, num - num3 * 0.8f, num2 - 0.2f * num3);
					val2.CloseFigure();
					graphics.FillPath((Brush)val, val2);
					break;
				}
				finally
				{
					if (val2 != null) ((IDisposable)val2).Dispose();
				}
			}
			case "Tapered Baguette":
			{
				PointF[] array = new PointF[4]
				{
					new PointF(num - num3 / 2.5f, num2 - num3),
					new PointF(num + num3 / 2.5f, num2 - num3),
					new PointF(num + num3 / 1.5f, num2 + num3),
					new PointF(num - num3 / 1.5f, num2 + num3)
				};
				graphics.FillPolygon((Brush)val, array);
				break;
			}
			default:
				graphics.FillRectangle((Brush)val, num - num3 / 2f, num2 - num3 / 2f, num3, num3);
				break;
			}
		}
		finally
		{
			if (val != null) ((IDisposable)val).Dispose();
		}
	}

	private void BtnNi_Click(object sender, EventArgs e)
	{
		((Control)this).Hide();
		try
		{
			GetString getStr = new GetString();
			((GetBaseClass)getStr).SetCommandPrompt(isVN ? "Nhập số Ni (ví dụ: 12, 14.5):" : "Enter Ring Size Ni (e.g., 12, 14.5):");
			getStr.Get();

			if ((int)((GetBaseClass)getStr).CommandResult() == 0)
			{
				string inputVal = getStr.StringResult().Trim();
				if (!string.IsNullOrEmpty(inputVal))
				{
					string niFormatted = inputVal.StartsWith("Ni:", StringComparison.OrdinalIgnoreCase) ? inputVal : ("Ni: " + inputVal);
					string currentText = ((Control)txtModelCode).Text.Trim();

					if (string.IsNullOrEmpty(currentText))
					{
						((Control)txtModelCode).Text = niFormatted;
					}
					else if (System.Text.RegularExpressions.Regex.IsMatch(currentText, @"Ni:\s*\S+"))
					{
						((Control)txtModelCode).Text = System.Text.RegularExpressions.Regex.Replace(currentText, @"Ni:\s*\S+", niFormatted);
					}
					else
					{
						((Control)txtModelCode).Text = currentText + " " + niFormatted;
					}
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(isVN ? ("Lỗi khi nhập Ni: " + ex.Message) : ("Error entering Ring Size: " + ex.Message));
		}
		finally
		{
			((Control)this).Show();
			doc.Views.Redraw();
		}
	}
}

}