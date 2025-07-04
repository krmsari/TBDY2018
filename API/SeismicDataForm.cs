using API.Models.Seismic;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace API
{
    public partial class SeismicDataForm : Form
    {
        public SeismicParameters SeismicParameters { get; private set; }

        private TextBox txtSs, txtS1;
        private ComboBox cmbSiteClass;
        private NumericUpDown numR, numD, numI;

        public SeismicDataForm()
        {
            InitializeComponent();
            InitializeComponentUI();
            this.SeismicParameters = new SeismicParameters();
        }

        private void InitializeComponentUI()
        {
            this.Text = "TBDY-2018 Deprem Parametreleri";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(450, 350);
            this.Padding = new Padding(20);
            this.Font = new Font("Segoe UI", 9F);

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

            txtSs = AddRow(mainLayout, "Ss:");
            txtS1 = AddRow(mainLayout, "S1:");
            cmbSiteClass = AddRow<ComboBox>(mainLayout, "Zemin Sınıfı:");
            numR = AddRow<NumericUpDown>(mainLayout, "Taşıyıcı Sistem Davranış Katsayısı (R):");
            numD = AddRow<NumericUpDown>(mainLayout, "Dayanım Fazlalığı Katsayısı (D):");
            numI = AddRow<NumericUpDown>(mainLayout, "Bina Önem Katsayısı (I):");

            // ComboBox ve NumericUpDown ayarları
            cmbSiteClass.Items.AddRange(new object[] { "ZA", "ZB", "ZC", "ZD", "ZE" });
            cmbSiteClass.DropDownStyle = ComboBoxStyle.DropDownList;
            numR.DecimalPlaces = 1; numR.Minimum = 1;
            numD.DecimalPlaces = 1; numD.Minimum = 1;
            numI.DecimalPlaces = 2; numI.Minimum = 1;

            // Butonlar
            var btnOk = new Button { Text = "Tamam", DialogResult = DialogResult.OK, Width = 100, Height = 30 };
            var btnCancel = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, Width = 100, Height = 30 };
            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 50 };
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnOk);

            this.Controls.Add(mainLayout);
            this.Controls.Add(buttonPanel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            btnOk.Click += (s, e) => SaveData();
        }

        private T AddRow<T>(TableLayoutPanel pnl, string labelText) where T : Control, new()
        {
            pnl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var label = new Label { Text = labelText, Anchor = AnchorStyles.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            var control = new T() { Dock = DockStyle.Fill, Margin = new Padding(3, 6, 3, 6) };
            pnl.Controls.Add(label, 0, pnl.RowCount - 1);
            pnl.Controls.Add(control, 1, pnl.RowCount - 1);
            return control;
        }
        private TextBox AddRow(TableLayoutPanel pnl, string labelText) => AddRow<TextBox>(pnl, labelText);

        private void SaveData()
        {
            try
            {
                SeismicParameters.Ss = double.Parse(txtSs.Text, CultureInfo.InvariantCulture);
                SeismicParameters.S1 = double.Parse(txtS1.Text, CultureInfo.InvariantCulture);
                SeismicParameters.SiteClass = cmbSiteClass.SelectedItem.ToString();
                SeismicParameters.R = (double)numR.Value;
                SeismicParameters.D = (double)numD.Value;
                SeismicParameters.I = (double)numI.Value;
            }
            catch
            {
                MessageBox.Show("Lütfen tüm alanları doğru formatta doldurun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None; // Formun kapanmasını engelle
            }
        }
    }
}
