// Delft3D 4 Community Launcher
// Unofficial, not affiliated with Deltares. "Delft3D" and "Deltares" are trademarks of Stichting Deltares.
// Copyright (C) 2026 DevAnthonyM. License: GPL-3.0-or-later.
//
// A small front end that runs the Delft3D-FLOW engine (d_hydro.exe + flow2d3d.dll) exactly like
// bin\run_dflow2d3d.bat does: PATH = bin;share;lib, working directory = the model folder,
// command = d_hydro.exe config_d_hydro.xml.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("Delft3D 4 Community Launcher")]
[assembly: System.Reflection.AssemblyProduct("Delft3D 4 Community Launcher (unofficial)")]
[assembly: System.Reflection.AssemblyCopyright("GPL-3.0. Not affiliated with Deltares.")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]

namespace Delft3DCommunity
{
    static class BuildInfo
    {
        public const string Release = "DIMRset_2026.02";
        public const string Commit = "5a4649830b1e5072caf019fb4850bbdefd9ad431";
        public const string BuildDate = "2026-10-27";
        public const string Site = "https://delft3d-community.vercel.app";
        public const string HelpUrl = Site + "/#help";
        public const string TroubleshootingUrl = Site + "/#faq";
        public const string DeltaresGuiUrl = "https://download.deltares.nl/en/delft3d-4-gui-open-source";
        public const string SourceUrl = "https://github.com/Deltares/Delft3D/tree/" + Release;
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    class MainForm : Form
    {
        readonly string installDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
        readonly string settingsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Delft3D4CommunityLauncher", "lastfolder.txt");

        TextBox folderBox;
        ComboBox mdfCombo;
        Label folderStatus;
        Button createConfigButton, runButton, stopButton, exampleButton, openResultsButton, saveLogButton;
        ProgressBar progress;
        Label progressLabel, resultLabel;
        LinkLabel resultHelpLink;
        RichTextBox logBox;

        Process engine;
        string runFolder;
        bool stopRequested;
        static readonly Regex ProgressRx = new Regex(@"Time to finish\s+(?<t>.+?),\s+(?<p>[\d.]+)% completed", RegexOptions.Compiled);

        public MainForm()
        {
            Text = "Delft3D 4 Community Launcher (unofficial)";
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F); // designed at 100%; WinForms scales up for 125%/150% displays
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10f);
            ClientSize = new Size(820, 600);
            MinimumSize = new Size(700, 520);
            StartPosition = FormStartPosition.CenterScreen;
            Load += (s, e) => FitToScreen();
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            BuildMenu();
            BuildLayout();
            ResumeLayout(true);
            LoadLastFolder();
            FormClosing += (s, e) => { if (IsRunning) { var r = MessageBox.Show(this, "A model is still running. Stop it and close?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning); if (r == DialogResult.No) e.Cancel = true; else KillEngine(); } };

            if (!File.Exists(Path.Combine(installDir, "bin", "d_hydro.exe")))
                SetResult(false, "Engine not found next to this program (bin\\d_hydro.exe). Please reinstall.");
        }

        // ---------- UI ----------

        void FitToScreen()
        {
            // Keep the whole window (incl. bottom buttons) on screen at high display scaling.
            var area = Screen.FromControl(this).WorkingArea;
            Width = Math.Min(Width, area.Width - 20);
            Height = Math.Min(Height, area.Height - 20);
            Left = area.Left + (area.Width - Width) / 2;
            Top = area.Top + (area.Height - Height) / 2;
        }

        void StyleRunButton()
        {
            runButton.BackColor = runButton.Enabled ? Color.FromArgb(0, 120, 212) : Color.FromArgb(205, 205, 205);
            runButton.ForeColor = runButton.Enabled ? Color.White : Color.FromArgb(120, 120, 120);
        }

        void BuildMenu()
        {
            var menu = new MenuStrip();
            var help = new ToolStripMenuItem("Help");
            help.DropDownItems.Add("Help and troubleshooting (website)", null, (s, e) => OpenUrl(BuildInfo.HelpUrl));
            help.DropDownItems.Add("Get Deltares' free model-building GUI", null, (s, e) => OpenUrl(BuildInfo.DeltaresGuiUrl));
            help.DropDownItems.Add(new ToolStripSeparator());
            help.DropDownItems.Add("Licenses", null, (s, e) => OpenPath(Path.Combine(installDir, "licenses")));
            help.DropDownItems.Add("Source code (Deltares)", null, (s, e) => OpenUrl(BuildInfo.SourceUrl));
            help.DropDownItems.Add("About", null, (s, e) => ShowAbout());
            menu.Items.Add(help);
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        void BuildLayout()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(14, 6, 14, 12) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // header
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // step 1
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // step 2
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // result
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // log
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // bottom buttons

            // Header
            var header = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false, Dock = DockStyle.Fill };
            header.Controls.Add(new Label { Text = "Delft3D 4 Community Launcher", Font = new Font("Segoe UI Semibold", 16f), AutoSize = true });
            header.Controls.Add(new Label { Text = "Unofficial community build of Delft3D 4 (" + BuildInfo.Release + "). Not affiliated with Deltares.", ForeColor = Color.DimGray, AutoSize = true });
            exampleButton = new Button { Text = "Try it: run the example model", AutoSize = true, Margin = new Padding(0, 8, 0, 4) };
            exampleButton.Click += (s, e) => RunExample();
            header.Controls.Add(exampleButton);
            root.Controls.Add(header);

            // Step 1
            var step1 = new GroupBox { Text = "Step 1  Choose your model folder", Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(10) };
            var s1 = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoSize = true };
            s1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            s1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            s1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            folderBox = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
            var browse = new Button { Text = "Browse...", AutoSize = true };
            browse.Click += (s, e) => BrowseFolder();
            s1.Controls.Add(new Label { Text = "Folder:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            s1.Controls.Add(folderBox, 1, 0);
            s1.Controls.Add(browse, 2, 0);
            mdfCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            mdfCombo.SelectedIndexChanged += (s, e) => UpdateConfigState();
            s1.Controls.Add(new Label { Text = "Model:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            s1.Controls.Add(mdfCombo, 1, 1);
            createConfigButton = new Button { Text = "Create it for me", AutoSize = true, Visible = false };
            createConfigButton.Click += (s, e) => CreateConfig();
            s1.Controls.Add(createConfigButton, 2, 1);
            folderStatus = new Label { AutoSize = true, ForeColor = Color.DimGray, Text = "Pick the folder that contains your Delft3D-FLOW model (.mdf file)." };
            s1.Controls.Add(folderStatus, 1, 2);
            step1.Controls.Add(s1);
            root.Controls.Add(step1);

            // Step 2
            var step2 = new GroupBox { Text = "Step 2  Run", Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(10) };
            var s2 = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoSize = true };
            s2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            s2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            s2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            runButton = new Button { Text = "Run model", Font = new Font("Segoe UI Semibold", 12f), AutoSize = true, Padding = new Padding(16, 4, 16, 4), Enabled = false, BackColor = Color.FromArgb(0, 120, 212), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            runButton.Click += (s, e) => StartRun(folderBox.Text, mdfCombo.SelectedItem as string);
            runButton.EnabledChanged += (s, e) => StyleRunButton();
            runButton.FlatAppearance.BorderSize = 0;
            StyleRunButton();
            stopButton = new Button { Text = "Stop", AutoSize = true, Enabled = false, Anchor = AnchorStyles.Left };
            stopButton.Click += (s, e) => { stopRequested = true; KillEngine(); };
            progress = new ProgressBar { Dock = DockStyle.Fill, Maximum = 1000, Height = 22, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            s2.Controls.Add(runButton, 0, 0);
            s2.Controls.Add(stopButton, 1, 0);
            s2.Controls.Add(progress, 2, 0);
            progressLabel = new Label { AutoSize = true, ForeColor = Color.DimGray, Text = "" };
            s2.Controls.Add(progressLabel, 2, 1);
            step2.Controls.Add(s2);
            root.Controls.Add(step2);

            // Result
            var resultPanel = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
            resultLabel = new Label { AutoSize = true, Font = new Font("Segoe UI Semibold", 11f), Text = "" };
            resultHelpLink = new LinkLabel { AutoSize = true, Text = "What does this mean?", Visible = false, Margin = new Padding(8, 3, 0, 0) };
            resultHelpLink.LinkClicked += (s, e) => OpenUrl(BuildInfo.TroubleshootingUrl);
            resultPanel.Controls.Add(resultLabel);
            resultPanel.Controls.Add(resultHelpLink);
            root.Controls.Add(resultPanel);

            // Log
            logBox = new RichTextBox { Dock = DockStyle.Fill, MinimumSize = new Size(0, 120), ReadOnly = true, Font = new Font("Consolas", 9f), BackColor = Color.FromArgb(24, 24, 24), ForeColor = Color.Gainsboro, WordWrap = false };
            root.Controls.Add(logBox);

            // Bottom buttons
            var bottom = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            openResultsButton = new Button { Text = "Open results folder", AutoSize = true, Enabled = false };
            openResultsButton.Click += (s, e) => OpenPath(runFolder);
            saveLogButton = new Button { Text = "Save log...", AutoSize = true, Enabled = false };
            saveLogButton.Click += (s, e) => SaveLog();
            bottom.Controls.Add(openResultsButton);
            bottom.Controls.Add(saveLogButton);
            root.Controls.Add(bottom);

            Controls.Add(root);
            root.BringToFront();
        }

        // ---------- Step 1: folder & config ----------

        void BrowseFolder()
        {
            using (var dlg = new FolderBrowserDialog { Description = "Choose the folder that contains your Delft3D-FLOW model (.mdf)", ShowNewFolderButton = false })
            {
                if (Directory.Exists(folderBox.Text)) dlg.SelectedPath = folderBox.Text;
                if (dlg.ShowDialog(this) == DialogResult.OK) SelectFolder(dlg.SelectedPath);
            }
        }

        void SelectFolder(string folder)
        {
            folderBox.Text = folder;
            mdfCombo.Items.Clear();
            var mdfs = Directory.Exists(folder)
                ? Directory.GetFiles(folder, "*.mdf").Select(Path.GetFileName).OrderBy(n => n).ToArray()
                : new string[0];
            foreach (var m in mdfs) mdfCombo.Items.Add(m);
            if (mdfs.Length == 0)
            {
                folderStatus.Text = "No Delft3D-FLOW model (.mdf) found in this folder.";
                folderStatus.ForeColor = Color.Firebrick;
                createConfigButton.Visible = false;
                runButton.Enabled = false;
                return;
            }
            mdfCombo.SelectedIndex = 0;
            SaveLastFolder(folder);
        }

        void UpdateConfigState()
        {
            string folder = folderBox.Text, mdf = mdfCombo.SelectedItem as string;
            if (mdf == null) return;
            string config = Path.Combine(folder, "config_d_hydro.xml");
            if (!File.Exists(config))
            {
                folderStatus.Text = "Found " + mdf + ", but config_d_hydro.xml is missing. Click \"Create it for me\".";
                folderStatus.ForeColor = Color.DarkOrange;
                createConfigButton.Visible = true;
                runButton.Enabled = false;
                return;
            }
            string configured = ReadConfiguredMdf(config);
            if (configured != null && !configured.Equals(mdf, StringComparison.OrdinalIgnoreCase))
            {
                folderStatus.Text = "config_d_hydro.xml points to " + configured + ", not " + mdf + ". Click \"Create it for me\" to switch.";
                folderStatus.ForeColor = Color.DarkOrange;
                createConfigButton.Visible = true;
            }
            else
            {
                folderStatus.Text = "Ready: " + mdf + " with config_d_hydro.xml.";
                folderStatus.ForeColor = Color.ForestGreen;
                createConfigButton.Visible = false;
            }
            runButton.Enabled = !IsRunning;
        }

        static string ReadConfiguredMdf(string config)
        {
            try
            {
                var m = Regex.Match(File.ReadAllText(config), @"<mdfFile>\s*(.+?)\s*</mdfFile>");
                return m.Success ? m.Groups[1].Value : null;
            }
            catch { return null; }
        }

        void CreateConfig()
        {
            string folder = folderBox.Text, mdf = mdfCombo.SelectedItem as string;
            if (mdf == null) return;
            string config = Path.Combine(folder, "config_d_hydro.xml");
            if (File.Exists(config))
            {
                string backup = config + ".bak";
                File.Copy(config, backup, true);
                AppendLog("Existing config_d_hydro.xml saved as config_d_hydro.xml.bak", Color.Khaki);
            }
            File.WriteAllText(config, ConfigXml(mdf), Encoding.GetEncoding("iso-8859-1"));
            AppendLog("Created " + config + " for " + mdf, Color.LightGreen);
            UpdateConfigState();
        }

        static string ConfigXml(string mdf)
        {
            return
"<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n" +
"<deltaresHydro xmlns=\"http://schemas.deltares.nl/deltaresHydro\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://schemas.deltares.nl/deltaresHydro http://content.oss.deltares.nl/schemas/d_hydro-1.00.xsd\">\r\n" +
"    <documentation>\r\n" +
"        File created by    : Delft3D 4 Community Launcher\r\n" +
"        File creation date : " + DateTime.Now.ToString("dd MMMM yyyy, HH:mm:ss") + "\r\n" +
"        File version       : 1.00\r\n" +
"    </documentation>\r\n" +
"    <control>\r\n" +
"        <sequence>\r\n" +
"            <start>myNameFlow</start>\r\n" +
"        </sequence>\r\n" +
"    </control>\r\n" +
"    <flow2D3D name=\"myNameFlow\">\r\n" +
"        <library>flow2d3d</library>\r\n" +
"        <mdfFile>" + mdf + "</mdfFile>\r\n" +
"    </flow2D3D>\r\n" +
"</deltaresHydro>\r\n";
        }

        // ---------- Step 2: run ----------

        bool IsRunning { get { return engine != null && !engine.HasExited; } }

        void RunExample()
        {
            if (IsRunning) return;
            string src = Path.Combine(installDir, "example", "f34");
            string dst = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Delft3D4-example");
            try
            {
                Directory.CreateDirectory(dst);
                foreach (var f in Directory.GetFiles(src))
                {
                    string name = Path.GetFileName(f);
                    if (name.EndsWith(".sh", StringComparison.OrdinalIgnoreCase)) continue;
                    File.Copy(f, Path.Combine(dst, name), true);
                }
            }
            catch (Exception ex)
            {
                SetResult(false, "Could not copy the example: " + ex.Message);
                return;
            }
            SelectFolder(dst);
            AppendLog("Example copied to " + dst, Color.LightSkyBlue);
            StartRun(dst, "f34.mdf");
        }

        void StartRun(string folder, string mdf)
        {
            if (IsRunning || string.IsNullOrEmpty(folder) || mdf == null) return;
            string bin = Path.Combine(installDir, "bin"), lib = Path.Combine(installDir, "lib"), share = Path.Combine(installDir, "share");
            string exe = Path.Combine(bin, "d_hydro.exe");
            if (!File.Exists(exe)) { SetResult(false, "Engine not found: " + exe); return; }
            if (!File.Exists(Path.Combine(folder, "config_d_hydro.xml"))) { SetResult(false, "config_d_hydro.xml is missing. Use \"Create it for me\" first."); return; }

            runFolder = folder;
            stopRequested = false;
            logBox.Clear();
            progress.Value = 0;
            progressLabel.Text = "Starting...";
            SetResult(null, "Running " + mdf + "...");
            AppendLog("Model folder : " + folder, Color.LightSkyBlue);
            AppendLog("Engine       : " + exe, Color.LightSkyBlue);
            AppendLog("", Color.Gainsboro);

            var psi = new ProcessStartInfo(exe, "config_d_hydro.xml")
            {
                WorkingDirectory = folder,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.Default,
                StandardErrorEncoding = Encoding.Default
            };
            // Same environment as bin\run_dflow2d3d.bat: PATH=bin;share;lib (kept first so our runtime DLLs win).
            psi.EnvironmentVariables["PATH"] = bin + ";" + share + ";" + lib + ";" + Environment.GetEnvironmentVariable("PATH");
            psi.EnvironmentVariables["D3D_HOME"] = installDir;
            psi.EnvironmentVariables["FORT_BUFFERED"] = "FALSE"; // flush Fortran output immediately so progress is live

            engine = new Process { StartInfo = psi, EnableRaisingEvents = true };
            engine.OutputDataReceived += (s, e) => { if (e.Data != null) BeginInvoke((Action)(() => OnEngineLine(e.Data))); };
            engine.ErrorDataReceived += (s, e) => { if (e.Data != null) BeginInvoke((Action)(() => AppendLog(e.Data, Color.Salmon))); };
            engine.Exited += (s, e) => BeginInvoke((Action)OnEngineExited);
            try
            {
                engine.Start();
                engine.BeginOutputReadLine();
                engine.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                engine = null;
                SetResult(false, "Could not start the engine: " + ex.Message);
                return;
            }
            SetRunningUi(true);
        }

        void OnEngineLine(string line)
        {
            var m = ProgressRx.Match(line);
            if (m.Success)
            {
                double p;
                if (double.TryParse(m.Groups["p"].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out p))
                {
                    progress.Value = Math.Max(0, Math.Min(1000, (int)(p * 10)));
                    progressLabel.Text = p.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "% done, about " + m.Groups["t"].Value.Trim() + " left";
                }
                return; // don't flood the log with progress lines
            }
            Color c = Color.Gainsboro;
            if (Regex.IsMatch(line, @"\b0 errors\b.*\b0 warnings\b")) c = Color.LightGreen;
            else if (line.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) >= 0) c = Color.Salmon;
            else if (line.IndexOf("WARNING", StringComparison.OrdinalIgnoreCase) >= 0) c = Color.Khaki;
            else if (line.IndexOf("FINISHED", StringComparison.Ordinal) >= 0) c = Color.LightGreen;
            AppendLog(line, c);
        }

        void OnEngineExited()
        {
            int code = -1;
            try { code = engine.ExitCode; } catch { }
            SetRunningUi(false);

            string log = logBox.Text;
            var counts = Regex.Match(log, @"(\d+)\s+errors\s+and\s+(\d+)\s+warnings");
            bool finished = log.Contains("FINISHED") && log.Contains("Delft3D-FLOW");
            int errors = counts.Success ? int.Parse(counts.Groups[1].Value) : -1;
            int warnings = counts.Success ? int.Parse(counts.Groups[2].Value) : -1;

            if (stopRequested)
            {
                SetResult(false, "Stopped by you.");
            }
            else if (finished && errors == 0)
            {
                progress.Value = 1000;
                progressLabel.Text = "100% done";
                SetResult(true, "Finished: 0 errors and " + Math.Max(0, warnings) + " warnings. Results are in the model folder.");
            }
            else
            {
                progressLabel.Text = "";
                bool diagErrors = AppendDiagErrors();
                string detail = errors > 0 ? errors + " error(s), shown below"
                              : diagErrors ? "the errors are shown below"
                              : "the engine stopped unexpectedly, code " + code;
                SetResult(false, "The model did not finish (" + detail + "). Details are also in the tri-diag file in the model folder.");
            }
            openResultsButton.Enabled = true;
            saveLogButton.Enabled = true;
            engine = null;
        }

        bool AppendDiagErrors()
        {
            try
            {
                var diag = Directory.GetFiles(runFolder, "tri-diag.*").OrderByDescending(File.GetLastWriteTime).FirstOrDefault();
                if (diag == null) return false;
                var lines = File.ReadAllLines(diag).Where(l => l.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) >= 0).Take(20).ToArray();
                if (lines.Length == 0) return false;
                AppendLog("", Color.Gainsboro);
                AppendLog("Errors from " + Path.GetFileName(diag) + ":", Color.Salmon);
                foreach (var l in lines) AppendLog(l, Color.Salmon);
                return true;
            }
            catch { return false; }
        }

        void KillEngine()
        {
            try { if (IsRunning) engine.Kill(); } catch { }
        }

        void SetRunningUi(bool running)
        {
            runButton.Enabled = !running && mdfCombo.SelectedItem != null && File.Exists(Path.Combine(folderBox.Text, "config_d_hydro.xml"));
            exampleButton.Enabled = !running;
            stopButton.Enabled = running;
            createConfigButton.Enabled = !running;
            if (running) { openResultsButton.Enabled = false; saveLogButton.Enabled = false; }
            UseWaitCursor = false;
        }

        // ---------- helpers ----------

        void SetResult(bool? ok, string text)
        {
            resultLabel.Text = text;
            resultLabel.ForeColor = ok == null ? Color.DimGray : ok.Value ? Color.ForestGreen : Color.Firebrick;
            resultHelpLink.Visible = ok == false;
        }

        void AppendLog(string line, Color color)
        {
            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;
            logBox.SelectionColor = color;
            logBox.AppendText(line + Environment.NewLine);
            logBox.ScrollToCaret();
        }

        void SaveLog()
        {
            using (var dlg = new SaveFileDialog { FileName = "delft3d-run-log.txt", Filter = "Text file (*.txt)|*.txt", InitialDirectory = runFolder })
                if (dlg.ShowDialog(this) == DialogResult.OK) File.WriteAllText(dlg.FileName, logBox.Text);
        }

        void LoadLastFolder()
        {
            try
            {
                if (!File.Exists(settingsFile)) return;
                string f = File.ReadAllText(settingsFile).Trim();
                if (Directory.Exists(f)) SelectFolder(f);
            }
            catch { }
        }

        void SaveLastFolder(string folder)
        {
            try { Directory.CreateDirectory(Path.GetDirectoryName(settingsFile)); File.WriteAllText(settingsFile, folder); } catch { }
        }

        void ShowAbout()
        {
            MessageBox.Show(this,
                "Delft3D 4 Community Launcher 1.0\n\n" +
                "Unofficial community build of Delft3D 4. Not affiliated with or endorsed by Deltares.\n" +
                "\"Delft3D\" and \"Deltares\" are trademarks of Stichting Deltares.\n\n" +
                "Engines built unmodified from " + BuildInfo.SourceUrl + "\n" +
                "Release: " + BuildInfo.Release + "\nCommit: " + BuildInfo.Commit + "\nBuilt: " + BuildInfo.BuildDate +
                " with Visual Studio 2022 + Intel oneAPI 2024.2.1\n\n" +
                "Delft3D: GPL-3.0 / LGPL-2.1 / AGPL-3.0. This launcher: GPL-3.0.\n" +
                "Intel and Microsoft runtime libraries: see the licenses folder.\n\n" +
                BuildInfo.Site,
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        static void OpenUrl(string url) { try { Process.Start(url); } catch { } }
        static void OpenPath(string path) { try { if (!string.IsNullOrEmpty(path)) Process.Start("explorer.exe", "\"" + path + "\""); } catch { } }
    }
}
