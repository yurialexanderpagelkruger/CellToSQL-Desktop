using CellToSQL.Models;
using CellToSQL.Services;
using CellToSQL.UI;

namespace CellToSQL;

public class MainForm : Form
{
    private readonly ComboBox cmbProvider = new ComboBox();
    private readonly ModernTextBox txtServer = new ModernTextBox();
    private readonly ModernTextBox txtPort = new ModernTextBox();
    private readonly ModernTextBox txtDatabase = new ModernTextBox();
    private readonly ModernTextBox txtUser = new ModernTextBox();
    private readonly ModernTextBox txtPassword = new ModernTextBox();
    private readonly ModernTextBox txtSqliteFile = new ModernTextBox();
    private readonly ModernToggle chkIntegrated = new ModernToggle();

    private readonly ModernButton btnTest = new ModernButton();
    private readonly ModernButton btnAddFiles = new ModernButton();
    private readonly ModernButton btnRemoveFile = new ModernButton();
    private readonly ModernButton btnClear = new ModernButton();
    private readonly ModernButton btnImport = new ModernButton();
    private readonly ModernButton btnCancel = new ModernButton();
    private readonly ModernButton btnBrowseSqlite = new ModernButton();

    private readonly ListView lvFiles = new ListView();
    private readonly RichTextBox txtLog = new RichTextBox();
    private readonly ModernProgress progress = new ModernProgress();
    private readonly Label lblStatus = new Label();

    private readonly RoundPanel cardConn = new RoundPanel();
    private readonly RoundPanel cardFiles = new RoundPanel();
    private readonly RoundPanel cardResult = new RoundPanel();
    private readonly RoundPanel logBox = new RoundPanel();

    private CancellationTokenSource _cts;

    public MainForm()
    {
        LoadAppIcon();
        BuildUi();
        cmbProvider.SelectedIndex = 0;
    }

    private void LoadAppIcon()
    {
        var asm = typeof(MainForm).Assembly;
        using var stream = asm.GetManifestResourceStream("CellToSQL.Resources.app.ico");
        if (stream != null)
            Icon = new Icon(stream);
    }

    private void BuildUi()
    {
        Text = "CellToSQL";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1000, 824);
        MinimumSize = new Size(1020, 880);
        BackColor = Theme.Background;
        Font = Theme.Body;
        AllowDrop = true;
        DoubleBuffered = true;

        // ---------- Header ----------
        var header = new Panel
        {
            BackColor = Theme.Surface,
            Location = new Point(0, 0),
            Size = new Size(1000, 76),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        header.Controls.Add(new Panel { BackColor = Theme.Border, Dock = DockStyle.Bottom, Height = 1 });

        var badge = new RoundPanel
        {
            Fill = Theme.Accent,
            DrawBorder = false,
            Radius = 10,
            Location = new Point(24, 18),
            Size = new Size(40, 40),
            BackColor = Theme.Surface
        };
        badge.Controls.Add(new Label
        {
            Text = "C",
            Font = Theme.Title,
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
        header.Controls.Add(badge);

        header.Controls.Add(new Label { Text = "CellToSQL", Font = Theme.Title, ForeColor = Theme.Text, AutoSize = true, Location = new Point(76, 14) });
        header.Controls.Add(new Label { Text = "Importador de archivos a base de datos", Font = Theme.Subtitle, ForeColor = Theme.Muted, AutoSize = true, Location = new Point(78, 44) });
        Controls.Add(header);

        // ---------- Card: Conexión ----------
        cardConn.Title = "Conexión a base de datos";
        cardConn.Location = new Point(24, 96);
        cardConn.Size = new Size(952, 240);
        cardConn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        Controls.Add(cardConn);

        cardConn.Controls.Add(FieldLabel("Proveedor", 20, 44));
        cmbProvider.Location = new Point(20, 62);
        cmbProvider.Size = new Size(200, 30);
        cmbProvider.Font = Theme.Body;
        cmbProvider.FlatStyle = FlatStyle.Flat;
        cmbProvider.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbProvider.BackColor = Theme.Surface;
        cmbProvider.ForeColor = Theme.Text;
        cmbProvider.Items.AddRange(new object[] { "SQL Server", "PostgreSQL", "MySQL", "SQLite" });
        cmbProvider.SelectedIndexChanged += (s, e) => ApplyProviderDefaults();
        cardConn.Controls.Add(cmbProvider);

        chkIntegrated.Text = "Autenticación Windows";
        chkIntegrated.Location = new Point(240, 66);
        chkIntegrated.Size = new Size(250, 24);
        chkIntegrated.CheckedChanged += (s, e) => ToggleCredentials();
        cardConn.Controls.Add(chkIntegrated);

        btnTest.Text = "Probar conexión";
        btnTest.Primary = true;
        btnTest.Location = new Point(752, 62);
        btnTest.Size = new Size(180, 34);
        btnTest.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnTest.Click += btnTest_Click;
        cardConn.Controls.Add(btnTest);

        cardConn.Controls.Add(FieldLabel("Servidor", 20, 108));
        txtServer.Location = new Point(20, 126);
        txtServer.Size = new Size(220, 34);
        cardConn.Controls.Add(txtServer);

        cardConn.Controls.Add(FieldLabel("Puerto", 252, 108));
        txtPort.Location = new Point(252, 126);
        txtPort.Size = new Size(90, 34);
        cardConn.Controls.Add(txtPort);

        cardConn.Controls.Add(FieldLabel("Base de datos", 354, 108));
        txtDatabase.Location = new Point(354, 126);
        txtDatabase.Size = new Size(240, 34);
        cardConn.Controls.Add(txtDatabase);

        cardConn.Controls.Add(FieldLabel("Usuario", 606, 108));
        txtUser.Location = new Point(606, 126);
        txtUser.Size = new Size(150, 34);
        cardConn.Controls.Add(txtUser);

        cardConn.Controls.Add(FieldLabel("Contraseña", 768, 108));
        txtPassword.Location = new Point(768, 126);
        txtPassword.Size = new Size(164, 34);
        txtPassword.PasswordChar = '*';
        cardConn.Controls.Add(txtPassword);

        cardConn.Controls.Add(FieldLabel("Archivo SQLite", 20, 172));
        txtSqliteFile.Location = new Point(20, 190);
        txtSqliteFile.Size = new Size(600, 34);
        cardConn.Controls.Add(txtSqliteFile);

        btnBrowseSqlite.Text = "Examinar...";
        btnBrowseSqlite.Primary = false;
        btnBrowseSqlite.Location = new Point(782, 190);
        btnBrowseSqlite.Size = new Size(150, 34);
        btnBrowseSqlite.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseSqlite.Click += btnBrowseSqlite_Click;
        cardConn.Controls.Add(btnBrowseSqlite);

        // ---------- Card: Archivos ----------
        cardFiles.Title = "Archivos de entrada";
        cardFiles.Location = new Point(24, 348);
        cardFiles.Size = new Size(952, 226);
        cardFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        Controls.Add(cardFiles);

        btnAddFiles.Text = "Agregar archivos";
        btnAddFiles.Primary = true;
        btnAddFiles.Location = new Point(20, 48);
        btnAddFiles.Size = new Size(160, 32);
        btnAddFiles.Click += btnAddFiles_Click;
        cardFiles.Controls.Add(btnAddFiles);

        btnRemoveFile.Text = "Quitar";
        btnRemoveFile.Primary = false;
        btnRemoveFile.Location = new Point(190, 48);
        btnRemoveFile.Size = new Size(110, 32);
        btnRemoveFile.Click += btnRemoveFile_Click;
        cardFiles.Controls.Add(btnRemoveFile);

        btnClear.Text = "Limpiar";
        btnClear.Primary = false;
        btnClear.Location = new Point(310, 48);
        btnClear.Size = new Size(110, 32);
        btnClear.Click += btnClear_Click;
        cardFiles.Controls.Add(btnClear);

        cardFiles.Controls.Add(new Label
        {
            Text = "Arrastrá archivos aquí o usá el botón Agregar archivos.",
            Font = Theme.Caption,
            ForeColor = Theme.Muted,
            AutoSize = true,
            Location = new Point(440, 58)
        });

        lvFiles.Location = new Point(20, 92);
        lvFiles.Size = new Size(912, 116);
        lvFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lvFiles.View = View.Details;
        lvFiles.FullRowSelect = true;
        lvFiles.BorderStyle = BorderStyle.None;
        lvFiles.BackColor = Theme.Surface;
        lvFiles.ForeColor = Theme.Text;
        lvFiles.Font = Theme.Body;
        lvFiles.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        lvFiles.AllowDrop = true;
        lvFiles.OwnerDraw = true;
        lvFiles.Columns.Add("Archivo", 320);
        lvFiles.Columns.Add("Ruta", 580);
        lvFiles.DrawColumnHeader += Lv_DrawColumnHeader;
        lvFiles.DrawItem += Lv_DrawItem;
        lvFiles.DrawSubItem += Lv_DrawSubItem;
        lvFiles.DragEnter += DragEnter_Handler;
        lvFiles.DragDrop += DragDrop_Handler;
        cardFiles.Controls.Add(lvFiles);

        // ---------- Card: Resultado ----------
        cardResult.Title = "Resultado";
        cardResult.Location = new Point(24, 586);
        cardResult.Size = new Size(952, 214);
        cardResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        Controls.Add(cardResult);

        btnImport.Text = "Importar a la base de datos";
        btnImport.Primary = true;
        btnImport.Location = new Point(20, 48);
        btnImport.Size = new Size(230, 36);
        btnImport.Click += btnImport_Click;
        cardResult.Controls.Add(btnImport);

        btnCancel.Text = "Cancelar";
        btnCancel.Primary = false;
        btnCancel.Location = new Point(262, 48);
        btnCancel.Size = new Size(110, 36);
        btnCancel.Enabled = false;
        btnCancel.Click += btnCancel_Click;
        cardResult.Controls.Add(btnCancel);

        progress.Location = new Point(390, 63);
        progress.Size = new Size(300, 8);
        cardResult.Controls.Add(progress);

        lblStatus.Location = new Point(706, 54);
        lblStatus.Size = new Size(226, 24);
        lblStatus.Font = Theme.Caption;
        lblStatus.ForeColor = Theme.Muted;
        lblStatus.Text = "";
        cardResult.Controls.Add(lblStatus);

        logBox.Fill = Theme.LogBg;
        logBox.DrawBorder = false;
        logBox.Radius = 10;
        logBox.BackColor = Theme.Surface;
        logBox.Location = new Point(20, 96);
        logBox.Size = new Size(912, 102);
        logBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        cardResult.Controls.Add(logBox);

        txtLog.Location = new Point(10, 10);
        txtLog.Size = new Size(892, 82);
        txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtLog.BorderStyle = BorderStyle.None;
        txtLog.BackColor = Theme.LogBg;
        txtLog.ForeColor = Theme.LogText;
        txtLog.Font = Theme.Log;
        txtLog.ReadOnly = true;
        txtLog.DetectUrls = false;
        logBox.Controls.Add(txtLog);

        DragEnter += DragEnter_Handler;
        DragDrop += DragDrop_Handler;
    }

    private static Label FieldLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            Font = Theme.Caption,
            ForeColor = Theme.Muted,
            AutoSize = true,
            Location = new Point(x, y)
        };
    }

    private void Lv_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
    {
        using (var b = new SolidBrush(Theme.HeaderBg))
            e.Graphics.FillRectangle(b, e.Bounds);

        var textRect = new Rectangle(e.Bounds.X + 12, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
        TextRenderer.DrawText(e.Graphics, e.Header.Text, Theme.CaptionBold, textRect, Theme.Muted,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        using var pen = new Pen(Theme.Border);
        e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
    }

    private void Lv_DrawItem(object sender, DrawListViewItemEventArgs e)
    {
        e.DrawDefault = false;
    }

    private void Lv_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
    {
        Color bg = e.Item.Selected ? Theme.SelectedRow : (e.ItemIndex % 2 == 0 ? Theme.Surface : Theme.Zebra);
        using (var b = new SolidBrush(bg))
            e.Graphics.FillRectangle(b, e.Bounds);

        var textRect = new Rectangle(e.Bounds.X + 12, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
        TextRenderer.DrawText(e.Graphics, e.SubItem.Text, Theme.Body, textRect, Theme.Text,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void ApplyProviderDefaults()
    {
        if (cmbProvider.SelectedIndex < 0) return;

        var provider = (DatabaseProvider)cmbProvider.SelectedIndex;
        bool sqlite = provider == DatabaseProvider.Sqlite;
        bool sqlServer = provider == DatabaseProvider.SqlServer;

        txtServer.Enabled = !sqlite;
        txtPort.Enabled = !sqlite;
        txtDatabase.Enabled = !sqlite;
        txtSqliteFile.Enabled = sqlite;
        btnBrowseSqlite.Enabled = sqlite;
        chkIntegrated.Enabled = sqlServer;

        txtPort.Text = provider switch
        {
            DatabaseProvider.SqlServer => "1433",
            DatabaseProvider.PostgreSql => "5432",
            DatabaseProvider.MySql => "3306",
            _ => txtPort.Text
        };

        ToggleCredentials();
    }

    private void ToggleCredentials()
    {
        bool sqlServer = cmbProvider.SelectedIndex == 0;
        bool sqlite = cmbProvider.SelectedIndex == 3;
        bool integrated = sqlServer && chkIntegrated.Checked;

        txtUser.Enabled = !sqlite && !integrated;
        txtPassword.Enabled = !sqlite && !integrated;
    }

    private void AddFiles(IEnumerable<string> paths)
    {
        foreach (string p in paths)
        {
            if (!FileExtractorService.IsSupported(p))
            {
                Log("Formato no soportado: " + p);
                continue;
            }

            if (lvFiles.Items.Cast<ListViewItem>().Any(i => (string)i.Tag == p)) continue;

            var item = new ListViewItem(Path.GetFileName(p));
            item.SubItems.Add(p);
            item.Tag = p;
            lvFiles.Items.Add(item);
        }
    }

    private void btnAddFiles_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Archivos soportados|*.xlsx;*.xlsm;*.csv;*.txt;*.tsv;*.log;*.pdf;*.docx|Todos los archivos|*.*"
        };
        if (ofd.ShowDialog() == DialogResult.OK)
            AddFiles(ofd.FileNames);
    }

    private void btnRemoveFile_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem i in lvFiles.SelectedItems)
            lvFiles.Items.Remove(i);
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        lvFiles.Items.Clear();
    }

    private void DragEnter_Handler(object sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
        else
            e.Effect = DragDropEffects.None;
    }

    private void DragDrop_Handler(object sender, DragEventArgs e)
    {
        if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        AddFiles(files);
    }

    private void btnBrowseSqlite_Click(object sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog { Filter = "SQLite|*.db;*.sqlite;*.db3|Todos|*.*" };
        if (sfd.ShowDialog() == DialogResult.OK)
            txtSqliteFile.Text = sfd.FileName;
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
        try
        {
            var settings = BuildSettings();
            Validate(settings);
            new DatabaseService(settings).TestConnection();
            MessageBox.Show("Conexión exitosa.", "CellToSQL", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error de conexión: " + ex.Message, "CellToSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnImport_Click(object sender, EventArgs e)
    {
        if (lvFiles.Items.Count == 0)
        {
            MessageBox.Show("Agregue al menos un archivo.", "CellToSQL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var settings = BuildSettings();
        try { Validate(settings); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "CellToSQL", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        var paths = lvFiles.Items.Cast<ListViewItem>().Select(i => (string)i.Tag).ToList();

        _cts = new CancellationTokenSource();
        SetBusy(true);
        txtLog.Clear();

        try
        {
            var service = new ImportService(settings);
            var summary = await Task.Run(() => service.Import(paths, Log, _cts.Token));
            Log("Importación completada: " + summary.TablesImported + " tabla(s), " + summary.RowsImported + " fila(s).");
            MessageBox.Show("Importación completada.\n" + summary.TablesImported + " tabla(s) y " + summary.RowsImported + " fila(s) importadas.", "CellToSQL", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            Log("Operación cancelada por el usuario.");
        }
        catch (Exception ex)
        {
            Log("Error: " + ex.Message);
            MessageBox.Show("Error: " + ex.Message, "CellToSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
            _cts = null;
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        _cts?.Cancel();
    }

    private void SetBusy(bool busy)
    {
        btnImport.Enabled = !busy;
        btnTest.Enabled = !busy;
        btnAddFiles.Enabled = !busy;
        btnRemoveFile.Enabled = !busy;
        btnClear.Enabled = !busy;
        btnCancel.Enabled = busy;

        if (busy) progress.Start(); else progress.Stop();
        lblStatus.Text = busy ? "Importando..." : "";
    }

    private void Log(string message)
    {
        if (txtLog.InvokeRequired)
        {
            txtLog.Invoke(new Action(() => Log(message)));
            return;
        }
        txtLog.AppendText(message + Environment.NewLine);
        txtLog.ScrollToCaret();
    }

    private ConnectionSettings BuildSettings()
    {
        return new ConnectionSettings
        {
            Provider = (DatabaseProvider)(cmbProvider.SelectedIndex < 0 ? 0 : cmbProvider.SelectedIndex),
            Server = txtServer.Text.Trim(),
            Port = int.TryParse(txtPort.Text.Trim(), out int p) ? p : 1433,
            Database = txtDatabase.Text.Trim(),
            User = txtUser.Text.Trim(),
            Password = txtPassword.Text,
            SqliteFile = txtSqliteFile.Text.Trim(),
            IntegratedSecurity = chkIntegrated.Checked
        };
    }

    private static void Validate(ConnectionSettings s)
    {
        if (s.Provider == DatabaseProvider.Sqlite)
        {
            if (string.IsNullOrWhiteSpace(s.SqliteFile))
                throw new Exception("Indique el archivo SQLite.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(s.Server))
                throw new Exception("Indique el servidor.");
            if (string.IsNullOrWhiteSpace(s.Database))
                throw new Exception("Indique la base de datos.");
            if (s.Provider == DatabaseProvider.SqlServer && !s.IntegratedSecurity && string.IsNullOrWhiteSpace(s.User))
                throw new Exception("Indique usuario o active autenticación Windows.");
        }
    }
}
