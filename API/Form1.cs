using API.Enums;
using API.Factories;
using API.Models;
using API.Models.Placements;
using API.Models.Seismic;
using API.services.validators;
using API.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace API
{
    public partial class Form1 : Form
    {
        // Gerekli servisler ve listeler.
        private readonly ISap2000ApiService _Sap2000ApiService;
        private readonly List<IMaterialProperties> _materialsToExport = new List<IMaterialProperties>();
        private readonly List<ISectionProperties> _sectionsToExport = new List<ISectionProperties>();
        private readonly List<ColumnPlacementInfo> _columnPlacements = new List<ColumnPlacementInfo>();
        private readonly List<BeamPlacementInfo> _beamPlacements = new List<BeamPlacementInfo>();
        private SeismicParameters _seismicParameters; // Sismik parametreleri tutmak için yeni bir alan

        // Otomatik isimler için sayaçlar.
        private int _nextColumnId = 101;
        private int _nextBeamId = 101;

        // Formdaki kutucuklar, butonlar vs.
        private DataGridView dgvAddedSections;
        private ComboBox cmbMaterials, cmbMaterialType;
        private readonly Dictionary<string, eCustomMatType> matTypeDisplayMap = new Dictionary<string, eCustomMatType>();
        private TextBox txtMaterialName, txtFck, txtFy, txtFu;
        private Label lblFck, lblFy, lblFu;
        private TextBox txtColName, txtColWidth, txtColHeight, txtColCover;
        private ComboBox cmbColConcrete, cmbColRebar;
        private TextBox txtBeamName, txtBeamWidth, txtBeamHeight, txtBeamCoverTop, txtBeamCoverBottom;
        private ComboBox cmbBeamConcrete, cmbBeamRebar;
        private TextBox txtSlabName, txtSlabThickness;
        private ComboBox cmbSlabMaterial;
        private NumericUpDown numTotalStories;
        private TextBox txtFirstStoryHeight, txtTypicalStoryHeight;
        private DataGridView dgvColumnPlacement;
        private NumericUpDown numNewColumnX, numNewColumnY;
        private ComboBox cmbNewColumnPlacementSection;
        private DataGridView dgvBeamPlacement;
        private ComboBox cmbBeamStartColumn;
        private ComboBox cmbBeamEndColumn;
        private ComboBox cmbBeamSection;
        private readonly Dictionary<Control, IInputValidator> _validators = new Dictionary<Control, IInputValidator>();

        public Form1(ISap2000ApiService Sap2000ApiService)
        {
            _Sap2000ApiService = Sap2000ApiService;
            InitializeComponentUI();
            InitializeComponent();
            this.MinimumSize = new Size(1200, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Load += OnFormLoad;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            LoadMaterialTypes();
            LoadDefaultMaterials();
            RegisterValidators();
        }

        // --- DEĞİŞTİRİLEN BÖLÜM ---
        // Bütün UI elemanlarını oluşturup forma yerleştirir.
        private void InitializeComponentUI()
        {
            this.SuspendLayout();

            // Ana layout paneli, 2 sütun ve 3 satırdan oluşur.
            var mainLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3, // Satır sayısı 3 olarak güncellendi.
                BackColor = SystemColors.ControlLightLight,
                Padding = new Padding(10),
                Margin = new Padding(0)
            };
            // Sütun genişlikleri: sol %40, sağ %60
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            // Satır yükseklikleri: ilk ikisi içeriğe göre, sonuncusu kalan alanı dolduracak şekilde.
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // İlgili GroupBox'lar oluşturuluyor.
            var gbMaterials = BuildMaterialGroupBox();
            var gbStories = BuildStoryDefinitionGroupBox();
            var gbSections = BuildSectionDefinitionGroupBox();
            var gbPlacement = BuildPlacementGroupBox();
            var bottomPanel = BuildBottomPanel();

            // GroupBox'lar TableLayoutPanel'in doğru hücrelerine yerleştiriliyor.
            // 1. Malzeme GroupBox'ı (sol üst)
            mainLayoutPanel.Controls.Add(gbMaterials, 0, 0);

            // 2. Kesit GroupBox'ı (sağ, 2 satır kaplayacak şekilde)
            mainLayoutPanel.Controls.Add(gbSections, 1, 0);
            mainLayoutPanel.SetRowSpan(gbSections, 2);

            // 3. Kat GroupBox'ı (sol, malzemenin altı)
            mainLayoutPanel.Controls.Add(gbStories, 0, 1);

            // 4. Yerleşim GroupBox'ı (en alt, 2 sütun kaplayacak şekilde)
            mainLayoutPanel.Controls.Add(gbPlacement, 0, 2);
            mainLayoutPanel.SetColumnSpan(gbPlacement, 2);

            // Ana paneli ve alt butonu forma ekle.
            this.Controls.Add(mainLayoutPanel);
            this.Controls.Add(bottomPanel);

            this.ResumeLayout(false);
        }
        // --- DEĞİŞİKLİK SONU ---

        private GroupBox BuildMaterialGroupBox()
        {
            var gb = new GroupBox { Text = "Malzeme Tanımlama", Dock = DockStyle.Fill, Padding = new Padding(10), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, BackColor = Color.White };
            var pnl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoSize = true, Font = new Font("Segoe UI", 9F), BackColor = Color.White };
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            txtMaterialName = AddRow(pnl, "Malzeme Adı:");
            var btnAddMaterial = CreateButton("Malzeme Ekle", Color.FromArgb(40, 167, 69), BtnAddMaterialToList_Click);
            pnl.Controls.Add(btnAddMaterial, 2, pnl.RowCount - 1);
            cmbMaterialType = AddRow(pnl, "Malzeme Tipi:", true);
            var btnClearMaterials = CreateButton("Listeyi Temizle", Color.FromArgb(220, 53, 69), (s, e) => { _materialsToExport.Clear(); UpdateUIDataSources(); });
            pnl.Controls.Add(btnClearMaterials, 2, pnl.RowCount - 1);
            cmbMaterialType.SelectedIndexChanged += (s, e) =>
            {
                if (cmbMaterialType.SelectedItem == null || !matTypeDisplayMap.ContainsKey(cmbMaterialType.SelectedItem.ToString())) return;
                var selectedType = matTypeDisplayMap[cmbMaterialType.SelectedItem.ToString()];
                var isRebar = selectedType == eCustomMatType.Rebar;
                lblFck.Visible = txtFck.Visible = !isRebar;
                lblFy.Visible = txtFy.Visible = isRebar;
                lblFu.Visible = txtFu.Visible = isRebar;
                txtMaterialName.Text = cmbMaterialType.Text == "Beton" ? "C25/30" : "B500C";
            };
            txtFck = AddRow(pnl, "fck (MPa):", out lblFck);
            txtFck.Text = "25";
            txtFy = AddRow(pnl, "fy (MPa):", out lblFy);
            txtFy.Text = "500";
            txtFu = AddRow(pnl, "fu (MPa):", out lblFu);
            txtFu.Text = "550";
            lblFy.Visible = txtFy.Visible = false;
            lblFu.Visible = txtFu.Visible = false;
            cmbMaterials = AddRow(pnl, "Tanımlı Malzemeler:", true);
            pnl.SetColumnSpan(cmbMaterials, 2);
            gb.Controls.Add(pnl);
            return gb;
        }

        private GroupBox BuildStoryDefinitionGroupBox()
        {
            var gb = new GroupBox { Text = "Kat Bilgileri Tanımlama", Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Padding = new Padding(10), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Margin = new Padding(3, 10, 3, 3), BackColor = Color.White };
            var pnl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoSize = true, Font = new Font("Segoe UI", 9F), BackColor = Color.White };
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnl.Controls.Add(new Label { Text = "Toplam Kat Sayısı", Anchor = AnchorStyles.Bottom | AnchorStyles.Left, AutoSize = true }, 0, 0);
            numTotalStories = new NumericUpDown { Dock = DockStyle.Top, Minimum = 1, Maximum = 100, Value = 5, Font = new Font("Segoe UI", 9F) };
            pnl.Controls.Add(numTotalStories, 0, 1);
            pnl.Controls.Add(new Label { Text = "İlk Kat Yüks. (m)", Anchor = AnchorStyles.Bottom | AnchorStyles.Left, AutoSize = true }, 1, 0);
            txtFirstStoryHeight = new TextBox { Dock = DockStyle.Top, Text = "3", Font = new Font("Segoe UI", 9F) };
            pnl.Controls.Add(txtFirstStoryHeight, 1, 1);
            pnl.Controls.Add(new Label { Text = "Normal Kat Yüks. (m)", Anchor = AnchorStyles.Bottom | AnchorStyles.Left, AutoSize = true }, 2, 0);
            txtTypicalStoryHeight = new TextBox { Dock = DockStyle.Top, Text = "2.8", Font = new Font("Segoe UI", 9F) };
            pnl.Controls.Add(txtTypicalStoryHeight, 2, 1);
            gb.Controls.Add(pnl);
            return gb;
        }

        private GroupBox BuildSectionDefinitionGroupBox()
        {
            var gb = new GroupBox
            {
                Text = "Kesit Tanımlama",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Height = 480,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.White
            };
            var sectionPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.White };
            sectionPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            sectionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            gb.Controls.Add(sectionPanel);
            var tcSections = new TabControl { Dock = DockStyle.Top, Height = 300, Font = new Font("Segoe UI", 9F) };
            tcSections.TabPages.Add(BuildFrameTabPage("Kolon", out txtColName, out cmbColConcrete, out cmbColRebar, out txtColWidth, out txtColHeight, out var colSpecificControls));
            tcSections.TabPages.Add(BuildFrameTabPage("Kiriş", out txtBeamName, out cmbBeamConcrete, out cmbBeamRebar, out txtBeamWidth, out txtBeamHeight, out var beamSpecificControls));
            tcSections.TabPages.Add(BuildSlabTabPage());
            txtColCover = colSpecificControls["Paspayı (mm):"] as TextBox;
            txtColCover.Text = "20";
            txtBeamCoverTop = beamSpecificControls["Üst Paspayı (mm):"] as TextBox;
            txtBeamCoverTop.Text = "20";
            txtBeamCoverBottom = beamSpecificControls["Alt Paspayı (mm):"] as TextBox;
            txtBeamCoverBottom.Text = "20";
            sectionPanel.Controls.Add(tcSections, 0, 0);
            dgvAddedSections = new DataGridView { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, Margin = new Padding(0, 10, 0, 0) };
            dgvAddedSections.Columns.Add("SectionName", "Kesit Adı");
            dgvAddedSections.Columns.Add("SectionType", "Tip");
            dgvAddedSections.Columns.Add("Material", "Beton Sınıfı");
            dgvAddedSections.Columns.Add("Rebar", "Donatı Çeliği");
            dgvAddedSections.Columns.Add("Dimensions", "Boyutlar");
            sectionPanel.Controls.Add(dgvAddedSections, 0, 1);
            return gb;
        }

        private GroupBox BuildPlacementGroupBox()
        {
            var gb = new GroupBox
            {
                Text = "Kesit Yerleşimi",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Height = 240,
                Padding = new Padding(10),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.White,
                Margin = new Padding(3, 10, 3, 3)
            };
            var tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F) };
            var tabKolon = new TabPage("Kolon") { BackColor = Color.White, Padding = new Padding(5) };
            tabKolon.Controls.Add(BuildColumnPlacementTab());
            var tabKiris = new TabPage("Kiriş") { BackColor = Color.White, Padding = new Padding(5) };
            tabKiris.Controls.Add(BuildBeamPlacementTab());
            tabControl.TabPages.Add(tabKolon);
            tabControl.TabPages.Add(tabKiris);
            gb.Controls.Add(tabControl);
            return gb;
        }

        private Panel BuildBottomPanel()
        {
            var bottomFlowPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 60, Padding = new Padding(0, 10, 10, 10), BackColor = Color.White };
            Button btnExportToSap2000 = CreateButton("Modeli Sap2000'e Aktar", Color.DarkGreen, BtnExportToSap2000_Click, 200, 40);
            Button btnSeismicInfo = CreateButton("Deprem Bilgilerini Gir", Color.DarkSlateBlue, BtnShowSeismicForm_Click, 200, 40); // Yeni buton
            bottomFlowPanel.Controls.Add(btnExportToSap2000);
            bottomFlowPanel.Controls.Add(btnSeismicInfo); // Butonu panele ekle
            return bottomFlowPanel;
        }

        private void BtnShowSeismicForm_Click(object sender, EventArgs e)
        {
            using (var form = new SeismicDataForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _seismicParameters = form.SeismicParameters;
                    MessageBox.Show("Deprem parametreleri başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnExportToSap2000_Click(object sender, EventArgs e)
        {
            try
            {
                var btn = sender as Button;
                btn.Text = "Aktarılıyor...";
                btn.Enabled = false;
                Application.DoEvents();

                // 1. Grid verilerini oluştur
                var gridData = CreateGridDataFromInputs();

                // 2. Servis metodunu yeni grid verileriyle çağır
                _Sap2000ApiService.CreateProjectInNewModel(
                    gridData,
                    _seismicParameters, // Yeni parametre
                    _materialsToExport,
                    _sectionsToExport,
                    _columnPlacements,
                    _beamPlacements,
                    true);

                MessageBox.Show("Model başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sap2000'e aktarımda hata oldu: \n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (sender is Button btn)
                {
                    btn.Text = "Modeli Sap2000'e Aktar";
                    btn.Enabled = true;
                }
            }
        }

        private GridSystemData CreateGridDataFromInputs()
        {
            var gridData = new GridSystemData();

            // X ve Y koordinatlarını kolon yerleşimlerinden al,
            // kopyaları önle (Distinct) ve sırala.
            gridData.XCoordinates = _columnPlacements
                .Select(c => c.X)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            gridData.YCoordinates = _columnPlacements
                .Select(c => c.Y)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            // Z koordinatlarını (kat yüksekliklerini) hesapla
            double firstStoryHeight = double.Parse(txtFirstStoryHeight.Text, CultureInfo.InvariantCulture) * 1000; // mm'ye çevir
            double typicalStoryHeight = double.Parse(txtTypicalStoryHeight.Text, CultureInfo.InvariantCulture) * 1000; // mm'ye çevir
            int totalStories = (int)numTotalStories.Value;

            gridData.ZCoordinates.Add(0); // Zemin kat
            double currentHeight = 0;
            for (int i = 0; i < totalStories; i++)
            {
                currentHeight += (i == 0) ? firstStoryHeight : typicalStoryHeight;
                gridData.ZCoordinates.Add(currentHeight);
            }

            return gridData;
        }

        private void RegisterValidators()
        {
            _validators[txtColWidth] = new NumericTextBoxValidator(300, "Kolon Genişliği");
            _validators[txtColHeight] = new NumericTextBoxValidator(300, "Kolon Yüksekliği");
            _validators[txtColCover] = new NumericTextBoxValidator(20, "Kolon Paspayı");
            _validators[txtBeamWidth] = new NumericTextBoxValidator(200, "Kiriş Genişliği");
            _validators[txtBeamHeight] = new NumericTextBoxValidator(300, "Kiriş Yüksekliği");
            _validators[txtBeamCoverTop] = new NumericTextBoxValidator(20, "Kiriş Üst Paspayı");
            _validators[txtBeamCoverBottom] = new NumericTextBoxValidator(20, "Kiriş Alt Paspayı");

            foreach (var pair in _validators)
            {
                if (pair.Key is TextBox textBox)
                {
                    textBox.Validating += OnValidatedTextBoxValidating;
                }
            }
        }

        private void OnValidatedTextBoxValidating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is Control control && _validators.TryGetValue(control, out var validator))
            {
                bool isValid = validator.Validate(control, out string error);
                validator.ApplyValidationStyle(control, isValid);
                control.Tag = isValid ? null : error;
            }
        }

        private TabPage BuildFrameTabPage(string type, out TextBox txtName, out ComboBox cmbConcrete, out ComboBox cmbRebar, out TextBox txtWidth, out TextBox txtHeight, out Dictionary<string, Control> specificControls)
        {
            var tabPage = new TabPage(type) { BackColor = Color.White, Padding = new Padding(10) };
            var pnl = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, AutoSize = true, BackColor = Color.White };
            txtName = AddRow(pnl, "Kesit Adı");
            txtName.Text = type == "Kolon" ? $"S30/30" : "K30/50";
            pnl.SetColumnSpan(txtName, 2);
            cmbConcrete = AddRow(pnl, "Beton Malzemesi:", true);
            pnl.SetColumnSpan(cmbConcrete, 2);
            cmbRebar = AddRow(pnl, "Donatı Malzemesi:", true);
            pnl.SetColumnSpan(cmbRebar, 2);
            txtWidth = AddRow(pnl, "Genişlik (b, mm):");
            txtWidth.Text = type == "Kolon" ? "300" : "200";
            txtHeight = AddRow(pnl, "Yükseklik (h, mm):");
            txtHeight.Text = type == "Kolon" ? "300" : "300";
            specificControls = new Dictionary<string, Control>();
            if (type == "Kolon") { var txtCover = AddRow(pnl, "Paspayı (mm):"); pnl.SetColumnSpan(txtCover, 2); specificControls.Add("Paspayı (mm):", txtCover); }
            else if (type == "Kiriş") { var txtCoverTop = AddRow(pnl, "Üst Paspayı (mm):"); var txtCoverBottom = AddRow(pnl, "Alt Paspayı (mm):"); specificControls.Add("Üst Paspayı (mm):", txtCoverTop); specificControls.Add("Alt Paspayı (mm):", txtCoverBottom); }
            var btnAdd = CreateButton($"{type} Ekle", Color.FromArgb(40, 167, 69), (s, e) => AddSection(type));
            var btnDelete = CreateButton("Seçili Sil", Color.FromArgb(220, 53, 69), BtnDeleteSection_Click);
            pnl.Controls.Add(btnAdd, 3, 0);
            pnl.Controls.Add(btnDelete, 3, 1);
            tabPage.Controls.Add(pnl);
            return tabPage;
        }

        private TabPage BuildSlabTabPage()
        {
            var tabPage = new TabPage("Döşeme") { BackColor = Color.White, Padding = new Padding(10) };
            var pnl = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, AutoSize = true, BackColor = Color.White };
            txtSlabName = AddRow(pnl, "Kesit Adı");
            txtSlabName.Text = "D12";
            pnl.SetColumnSpan(txtSlabName, 2);
            cmbSlabMaterial = AddRow(pnl, "Döşeme Malzemesi:", true);
            pnl.SetColumnSpan(cmbSlabMaterial, 2);
            txtSlabThickness = AddRow(pnl, "Kalınlık (mm):");
            txtSlabThickness.Text = "120";
            pnl.SetColumnSpan(txtSlabThickness, 2);
            var btnAdd = CreateButton("Döşeme Ekle", Color.FromArgb(40, 167, 69), (s, e) => AddSection("Döşeme"));
            var btnDelete = CreateButton("Seçili Sil", Color.FromArgb(220, 53, 69), BtnDeleteSection_Click);
            pnl.Controls.Add(btnAdd, 3, 0);
            pnl.Controls.Add(btnDelete, 3, 1);
            tabPage.Controls.Add(pnl);
            return tabPage;
        }

        private Control BuildColumnPlacementTab()
        {
            var mainPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2, BackColor = Color.White };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            dgvColumnPlacement = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false, BackgroundColor = Color.White };
            dgvColumnPlacement.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColumnName", DataPropertyName = "ColumnName", HeaderText = "Kolon Adı" });
            dgvColumnPlacement.Columns.Add(new DataGridViewTextBoxColumn { Name = "X", DataPropertyName = "X", HeaderText = "X (m)" });
            dgvColumnPlacement.Columns.Add(new DataGridViewTextBoxColumn { Name = "Y", DataPropertyName = "Y", HeaderText = "Y (m)" });
            dgvColumnPlacement.Columns.Add(new DataGridViewTextBoxColumn { Name = "SectionName", DataPropertyName = "SectionName", HeaderText = "Kesit Adı" });
            mainPanel.Controls.Add(dgvColumnPlacement, 0, 0);
            var rightPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5, // 1 Buton Paneli + 3 Girdi Kontrolü + 1 Boşluk Satırı = 5 satır
                ColumnCount = 2,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Satırların davranışlarını tanımlıyoruz
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Butonlar için
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Kolon Kesiti için
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // X koordinatı için
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Y koordinatı için
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Kalan tüm boşluğu dolduracak yay satırı

            // Butonları oluştur ve en üstteki satıra (index 0) ekle
            var btnAdd = CreateButton("Kolon Ekle", Color.FromArgb(40, 167, 69), BtnAddNewColumn_Click, 120);
            var btnDelete = CreateButton("Seçili Sil", Color.FromArgb(220, 53, 69), BtnDeleteSelectedColumn_Click, 120);
            var buttonFlowPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 0)
            };
            buttonFlowPanel.Controls.Add(btnAdd);
            buttonFlowPanel.Controls.Add(btnDelete);
            rightPanel.Controls.Add(buttonFlowPanel, 0, 0); // Satır 0
            rightPanel.SetColumnSpan(buttonFlowPanel, 2);

            // Girdi kontrollerini butonların altına ekliyoruz
            var lblSection = new Label { Text = "Kolon Kesiti:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
            cmbNewColumnPlacementSection = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            rightPanel.Controls.Add(lblSection, 0, 1); // Satır 1
            rightPanel.Controls.Add(cmbNewColumnPlacementSection, 1, 1);

            var lblX = new Label { Text = "X (m):", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
            numNewColumnX = new NumericUpDown { DecimalPlaces = 2, Minimum = -1000, Maximum = 1000, Dock = DockStyle.Fill };
            rightPanel.Controls.Add(lblX, 0, 2); // Satır 2
            rightPanel.Controls.Add(numNewColumnX, 1, 2);

            var lblY = new Label { Text = "Y (m):", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
            numNewColumnY = new NumericUpDown { DecimalPlaces = 2, Minimum = -1000, Maximum = 1000, Dock = DockStyle.Fill };
            rightPanel.Controls.Add(lblY, 0, 3); // Satır 3
            rightPanel.Controls.Add(numNewColumnY, 1, 3);
            mainPanel.Controls.Add(rightPanel, 1, 0);
            return mainPanel;
        }

        private Control BuildBeamPlacementTab()
        {
            var mainPanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 1, ColumnCount = 2, BackColor = Color.White };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            dgvBeamPlacement = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false, BackgroundColor = Color.White };
            dgvBeamPlacement.Columns.Add(new DataGridViewTextBoxColumn { Name = "BeamName", DataPropertyName = "BeamName", HeaderText = "Kiriş Adı" });
            dgvBeamPlacement.Columns.Add(new DataGridViewTextBoxColumn { Name = "StartColumnName", DataPropertyName = "StartColumnName", HeaderText = "Başlangıç Kolonu" });
            dgvBeamPlacement.Columns.Add(new DataGridViewTextBoxColumn { Name = "EndColumnName", DataPropertyName = "EndColumnName", HeaderText = "Bitiş Kolonu" });
            dgvBeamPlacement.Columns.Add(new DataGridViewTextBoxColumn { Name = "SectionName", DataPropertyName = "SectionName", HeaderText = "Kiriş Kesiti" });
            mainPanel.Controls.Add(dgvBeamPlacement, 0, 0);
            var rightPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 2,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Satırların davranışlarını aynı kalıyor
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // DEĞİŞİKLİK: Butonları en üste (satır 0) alıyoruz.
            var btnAdd = CreateButton("Kiriş Ekle", Color.FromArgb(23, 162, 184), BtnAddNewBeam_Click, 120);
            var btnDelete = CreateButton("Seçili Sil", Color.FromArgb(220, 53, 69), BtnDeleteSelectedBeam_Click, 120);
            var buttonFlowPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 0)
            };
            buttonFlowPanel.Controls.Add(btnAdd);
            buttonFlowPanel.Controls.Add(btnDelete);
            rightPanel.Controls.Add(buttonFlowPanel, 0, 0); // Butonlar satır 0'a eklendi.
            rightPanel.SetColumnSpan(buttonFlowPanel, 2);

            // ComboBox'ları butonların altına (satır 1, 2, 3) ekliyoruz.
            var lblStartCol = new Label { Text = "Başlangıç Kolonu:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
            cmbBeamStartColumn = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            rightPanel.Controls.Add(lblStartCol, 0, 1); // Satır 1
            rightPanel.Controls.Add(cmbBeamStartColumn, 1, 1);

            var lblEndCol = new Label { Text = "Bitiş Kolonu:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
            cmbBeamEndColumn = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            rightPanel.Controls.Add(lblEndCol, 0, 2); // Satır 2
            rightPanel.Controls.Add(cmbBeamEndColumn, 1, 2);

            var lblSection = new Label { Text = "Kiriş Kesiti:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
            cmbBeamSection = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            rightPanel.Controls.Add(lblSection, 0, 3); // Satır 3
            rightPanel.Controls.Add(cmbBeamSection, 1, 3);
            mainPanel.Controls.Add(rightPanel, 1, 0);
            return mainPanel;
        }

        private void BtnAddMaterialToList_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbMaterialType.SelectedItem == null || !matTypeDisplayMap.ContainsKey(cmbMaterialType.SelectedItem.ToString())) throw new Exception("Malzeme tipi seçiniz.");
                var matType = matTypeDisplayMap[cmbMaterialType.SelectedItem.ToString()];
                var parameters = new Dictionary<string, object> { { "MaterialName", txtMaterialName.Text } };
                IMaterialFactory factory = (matType == eCustomMatType.Rebar) ? (IMaterialFactory)new RebarMaterialFactory() : new ConcreteMaterialFactory();
                if (matType == eCustomMatType.Rebar) { parameters["Fy"] = Convert.ToDouble(txtFy.Text, CultureInfo.InvariantCulture); parameters["Fu"] = Convert.ToDouble(txtFu.Text, CultureInfo.InvariantCulture); }
                else { parameters["Fck"] = Convert.ToDouble(txtFck.Text, CultureInfo.InvariantCulture); }
                var material = factory.CreateMaterial(parameters);
                if (string.IsNullOrWhiteSpace(material.MaterialName) || _materialsToExport.Any(m => m.MaterialName.Equals(material.MaterialName, StringComparison.OrdinalIgnoreCase))) throw new Exception("Malzeme adı boş olamaz veya bu isimde malzeme zaten var.");
                _materialsToExport.Add(material);
                UpdateUIDataSources();
                txtMaterialName.Clear();
            }
            catch (Exception ex) { MessageBox.Show("Malzeme eklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void AddSection(string type)
        {
            try
            {
                ISectionProperties section = null;
                if (type == "Kolon") section = new ColumnSectionProperties { SectionName = txtColName.Text, MaterialName = cmbColConcrete.SelectedItem.ToString(), RebarMaterialName = cmbColRebar.SelectedItem.ToString(), Width = double.Parse(txtColWidth.Text), Depth = double.Parse(txtColHeight.Text), ConcreteCover = double.Parse(txtColCover.Text) };
                else if (type == "Kiriş") section = new BeamSectionProperties { SectionName = txtBeamName.Text, MaterialName = cmbBeamConcrete.SelectedItem.ToString(), RebarMaterialName = cmbBeamRebar.SelectedItem.ToString(), Width = double.Parse(txtBeamWidth.Text), Depth = double.Parse(txtBeamHeight.Text), CoverTop = double.Parse(txtBeamCoverTop.Text), CoverBottom = double.Parse(txtBeamCoverBottom.Text) };
                else if (type == "Döşeme") section = new SlabSectionProperties { SectionName = txtSlabName.Text, SlabMaterialName = cmbSlabMaterial.SelectedItem.ToString(), Thickness = double.Parse(txtSlabThickness.Text) };

                if (section != null && !string.IsNullOrWhiteSpace(section.SectionName) && !_sectionsToExport.Any(s => s.SectionName.Equals(section.SectionName, StringComparison.OrdinalIgnoreCase)))
                {
                    _sectionsToExport.Add(section);
                    UpdateUIDataSources();
                }
                else throw new Exception("Kesit adı boş olamaz veya bu isimde kesit zaten var.");
            }
            catch (Exception ex) { MessageBox.Show($"{type} bilgileri hatalı veya eksik.\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnDeleteSection_Click(object sender, EventArgs e)
        {
            if (dgvAddedSections.SelectedRows.Count == 0) return;
            var namesToDelete = dgvAddedSections.SelectedRows.Cast<DataGridViewRow>().Select(r => r.Cells["SectionName"].Value?.ToString()).Where(name => name != null).ToList();
            _sectionsToExport.RemoveAll(s => namesToDelete.Contains(s.SectionName));
            UpdateUIDataSources();
        }

        private void BtnAddNewColumn_Click(object sender, EventArgs e)
        {
            if (cmbNewColumnPlacementSection.SelectedItem == null) { MessageBox.Show("Lütfen bir kesit seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string newColumnName = $"S{_nextColumnId++}";
            var placement = new ColumnPlacementInfo
            {
                ColumnName = newColumnName,
                X = (double)numNewColumnX.Value * 1000,
                Y = (double)numNewColumnY.Value * 1000,
                SectionName = cmbNewColumnPlacementSection.SelectedItem.ToString()
            };
            _columnPlacements.Add(placement);
            UpdateUIDataSources();
        }

        private void BtnDeleteSelectedColumn_Click(object sender, EventArgs e)
        {
            if (dgvColumnPlacement.SelectedRows.Count == 0) return;
            var selectedName = dgvColumnPlacement.SelectedRows[0].Cells["ColumnName"].Value.ToString();
            if (_beamPlacements.Any(b => b.StartColumnName == selectedName || b.EndColumnName == selectedName)) { MessageBox.Show("Bu kolon bir kirişe bağlı olduğu için silinemez.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var itemToRemove = _columnPlacements.FirstOrDefault(c => c.ColumnName == selectedName);
            if (itemToRemove != null) { _columnPlacements.Remove(itemToRemove); UpdateUIDataSources(); }
        }

        private void BtnAddNewBeam_Click(object sender, EventArgs e)
        {
            if (cmbBeamStartColumn.SelectedItem == null || cmbBeamEndColumn.SelectedItem == null || cmbBeamSection.SelectedItem == null) { MessageBox.Show("Lütfen başlangıç, bitiş kolonu ve kesit seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var startCol = cmbBeamStartColumn.SelectedItem.ToString();
            var endCol = cmbBeamEndColumn.SelectedItem.ToString();
            if (startCol == endCol) { MessageBox.Show("Başlangıç ve bitiş kolonları aynı olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            string newBeamName = $"K{_nextBeamId++}";
            var placement = new BeamPlacementInfo { BeamName = newBeamName, StartColumnName = startCol, EndColumnName = endCol, SectionName = cmbBeamSection.SelectedItem.ToString() };
            _beamPlacements.Add(placement);
            UpdateUIDataSources();
        }

        private void BtnDeleteSelectedBeam_Click(object sender, EventArgs e)
        {
            if (dgvBeamPlacement.SelectedRows.Count > 0 && dgvBeamPlacement.SelectedRows[0].DataBoundItem is BeamPlacementInfo info)
            { _beamPlacements.Remove(info); UpdateUIDataSources(); }
        }

        private void UpdateUIDataSources() { UpdateMaterialLists(); UpdateSectionLists(); UpdatePlacementGridsAndInputs(); }
        private void UpdateMaterialLists()
        {
            var concreteMaterials = _materialsToExport.OfType<ConcreteMaterialProperties>().Select(m => m.MaterialName).ToArray();
            var rebarMaterials = _materialsToExport.OfType<RebarMaterialProperties>().Select(m => m.MaterialName).ToArray();
            UpdateCombo(cmbColConcrete, concreteMaterials); UpdateCombo(cmbBeamConcrete, concreteMaterials); UpdateCombo(cmbSlabMaterial, concreteMaterials);
            UpdateCombo(cmbColRebar, rebarMaterials); UpdateCombo(cmbBeamRebar, rebarMaterials);
            cmbMaterials.Items.Clear();
            var materialDescriptions = _materialsToExport.Select(m => $"{EnumHelper.GetDescription(m.MaterialType)} - {m.MaterialName}").ToArray();
            if (materialDescriptions.Any()) { cmbMaterials.Items.AddRange(materialDescriptions); cmbMaterials.SelectedIndex = 0; }
        }

        private void UpdateSectionLists()
        {
            UpdateAddedSectionsGrid();
            var columnSectionNames = _sectionsToExport.OfType<ColumnSectionProperties>().Select(cs => cs.SectionName).ToArray();
            UpdateCombo(cmbNewColumnPlacementSection, columnSectionNames);
            var beamSectionNames = _sectionsToExport.OfType<BeamSectionProperties>().Select(bs => bs.SectionName).ToArray();
            UpdateCombo(cmbBeamSection, beamSectionNames);
        }

        private void UpdateAddedSectionsGrid()
        {
            dgvAddedSections.Rows.Clear();
            foreach (var section in _sectionsToExport)
            {
                string type = "Bilinmeyen", dims = "-", mat = "-", rebar = "N/A";
                if (section is ColumnSectionProperties col) { type = "Kolon"; dims = $"{col.Depth}/{col.Width}"; mat = col.MaterialName; rebar = col.RebarMaterialName; }
                else if (section is BeamSectionProperties beam) { type = "Kiriş"; dims = $"{beam.Depth}/{beam.Width}"; mat = beam.MaterialName; rebar = beam.RebarMaterialName; }
                else if (section is SlabSectionProperties slab) { type = "Döşeme"; dims = $"Kalınlık: {slab.Thickness}"; mat = slab.SlabMaterialName; }
                dgvAddedSections.Rows.Add(section.SectionName, type, mat, rebar, dims);
            }
        }

        private void UpdatePlacementGridsAndInputs()
        {
            dgvColumnPlacement.DataSource = null; if (_columnPlacements.Any()) dgvColumnPlacement.DataSource = new BindingSource { DataSource = _columnPlacements.ToList() };
            dgvBeamPlacement.DataSource = null; if (_beamPlacements.Any()) dgvBeamPlacement.DataSource = new BindingSource { DataSource = _beamPlacements.ToList() };
            var columnNames = _columnPlacements.Select(c => c.ColumnName).ToArray();
            UpdateCombo(cmbBeamStartColumn, columnNames); UpdateCombo(cmbBeamEndColumn, columnNames);
        }

        private void LoadMaterialTypes()
        {
            cmbMaterialType.Items.Clear(); matTypeDisplayMap.Clear();
            foreach (eCustomMatType type in Enum.GetValues(typeof(eCustomMatType))) { string desc = EnumHelper.GetDescription(type); cmbMaterialType.Items.Add(desc); matTypeDisplayMap[desc] = type; }
            if (cmbMaterialType.Items.Count > 0) cmbMaterialType.SelectedIndex = 0;
        }

        private void LoadDefaultMaterials()
        {
            _materialsToExport.Clear(); _sectionsToExport.Clear();
            _materialsToExport.AddRange(new List<IMaterialProperties> { new ConcreteMaterialProperties { MaterialName = "C30/37", Fck = 30 }, new RebarMaterialProperties { MaterialName = "B420C", Fy = 420, Fu = 550 } });
            UpdateUIDataSources();
        }


        private T AddRow<T>(TableLayoutPanel pnl, string labelText, out Label label, bool isComboBox = false) where T : Control, new()
        {
            pnl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            label = new Label { Text = labelText, Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 8, 3, 3), Dock = DockStyle.Fill };
            var control = new T() { Margin = new Padding(3, 6, 3, 3), Dock = DockStyle.Fill };
            if (isComboBox && control is ComboBox cmb) cmb.DropDownStyle = ComboBoxStyle.DropDownList;
            int nextRow = pnl.RowCount++;
            pnl.Controls.Add(label, 0, nextRow); pnl.Controls.Add(control, 1, nextRow);
            return control;
        }

        private T AddRow<T>(TableLayoutPanel pnl, string labelText, bool isComboBox = false) where T : Control, new() => AddRow<T>(pnl, labelText, out _, isComboBox);
        private TextBox AddRow(TableLayoutPanel pnl, string labelText) => AddRow<TextBox>(pnl, labelText, out _);
        private TextBox AddRow(TableLayoutPanel pnl, string labelText, out Label label) => AddRow<TextBox>(pnl, labelText, out label);
        private ComboBox AddRow(TableLayoutPanel pnl, string labelText, bool isComboBox) => AddRow<ComboBox>(pnl, labelText, out _, isComboBox);

        private T AddControlToPanel<T>(TableLayoutPanel panel, string labelText, T control) where T : Control
        {
            var label = new Label { Text = labelText, Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };
            control.Margin = new Padding(3, 6, 3, 3); control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            int nextRow = panel.RowCount; panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(label, 0, nextRow); panel.Controls.Add(control, 1, nextRow);
            panel.RowCount++; return control;
        }

        private Button CreateButton(string text, Color color, EventHandler click, int width = 120, int height = 35)
        {
            var btn = new Button { Text = text, Width = width, Height = height, FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Margin = new Padding(5) };
            btn.FlatAppearance.BorderSize = 0; btn.Click += click; return btn;
        }

        private void UpdateCombo(ComboBox cmb, string[] items)
        {
            var selected = cmb.SelectedItem?.ToString();
            cmb.DataSource = null; cmb.Items.Clear();
            if (items != null && items.Any())
            {
                cmb.Items.AddRange(items);
                if (selected != null && cmb.Items.Contains(selected)) cmb.SelectedItem = selected;
                else if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
            }
        }
    }
}