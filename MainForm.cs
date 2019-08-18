﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Arc.YTSubConverter.Formats;
using Arc.YTSubConverter.Formats.Ass;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter
{
    public partial class MainForm : Form
    {
        private Dictionary<string, AssStyleOptions> _styleOptions;
        private Dictionary<string, AssStyle> _styles;

        public MainForm()
        {
            InitializeComponent();

            LocalizeUI();

            _styleOptions = AssStyleOptionsList.Load().ToDictionary(o => o.Name);
            ExpandCollapseStyleOptions();
            ClearUi();
        }

        private void LocalizeUI()
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Text = $"YTSubConverter {version}";

            _chkStyleOptions.Text = Resources.StyleOptions;
            _lblShadowTypes.Text = Resources.ShadowTypes;
            _chkGlow.Text = Resources.Glow;
            _chkBevel.Text = Resources.Bevel;
            _chkSoftShadow.Text = Resources.SoftShadow;
            _chkHardShadow.Text = Resources.HardShadow;
            _chkKaraoke.Text = Resources.UseForKaraoke;
            _chkHighlightCurrentWord.Text = Resources.HighlightCurrentWord;
            _lblCurrentWordTextColor.Text = Resources.KaraokeTextColor;
            _lblCurrentWordOutlineColor.Text = Resources.KaraokeOutlineColor;
            _lblCurrentWordShadowColor.Text = Resources.KaraokeShadowColor;
            _btnConvert.Text = Resources.Convert;
            _dlgOpenFile.Filter = Resources.FileFilter;
        }

        private AssStyleOptions SelectedStyleOptions
        {
            get { return (AssStyleOptions)_lstStyles.SelectedItem; }
        }

        private void _chkStyleOptions_CheckedChanged(object sender, EventArgs e)
        {
            ExpandCollapseStyleOptions();
        }

        private void ExpandCollapseStyleOptions()
        {
            Height = _chkStyleOptions.Checked ? 488 : 142;
        }

        private void _btnBrowse_Click(object sender, EventArgs e)
        {
            if (_dlgOpenFile.ShowDialog() != DialogResult.OK)
                return;

            LoadFile(_dlgOpenFile.FileName);
        }

        private void LoadFile(string filePath)
        {
            ClearUi();

            try
            {
                SubtitleDocument doc = SubtitleDocument.Load(filePath);
                PopulateUi(filePath, doc);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Resources.FailedToLoadFile0, ex.Message), Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void PopulateUi(string filePath, SubtitleDocument doc)
        {
            _txtInputFile.Text = filePath;

            AssDocument assDoc = doc as AssDocument;
            if (assDoc != null)
            {
                _grpStyleOptions.Enabled = true;

                _styles = assDoc.Styles.ToDictionary(s => s.Name);
                foreach (AssStyle style in assDoc.Styles)
                {
                    if (!_styleOptions.ContainsKey(style.Name))
                        _styleOptions.Add(style.Name, new AssStyleOptions(style));
                }

                _lstStyles.DataSource = assDoc.Styles.Select(s => _styleOptions[s.Name]).ToList();
                if (_lstStyles.Items.Count > 0)
                    _lstStyles.SelectedIndex = 0;
            }

            _btnConvert.Enabled = true;
        }

        private void ClearUi()
        {
            _styles = null;

            _txtInputFile.Text = Resources.DragNDropTip;
            _grpStyleOptions.Enabled = false;
            _lstStyles.DataSource = null;
            UpdateStylePreview();
            _btnConvert.Enabled = false;
        }

        private void _lstStyles_SelectedIndexChanged(object sender, EventArgs e)
        {
            AssStyleOptions options = SelectedStyleOptions;
            if (options == null)
            {
                _spltStyleOptions.Panel2.Enabled = false;
                _brwPreview.DocumentText = string.Empty;
                return;
            }

            AssStyle style = _styles[options.Name];

            _spltStyleOptions.Panel2.Enabled = true;
            _pnlShadowType.Enabled = style.HasShadow;

            if (style.HasOutline && !style.HasOutlineBox)
            {
                _chkGlow.Checked = true;
                _chkGlow.Enabled = false;
            }
            else
            {
                _chkGlow.Checked = options.ShadowTypes.Contains(ShadowType.Glow);
                _chkGlow.Enabled = true;
            }

            _chkBevel.Checked = options.ShadowTypes.Contains(ShadowType.Bevel);
            _chkSoftShadow.Checked = options.ShadowTypes.Contains(ShadowType.SoftShadow);
            _chkHardShadow.Checked = options.ShadowTypes.Contains(ShadowType.HardShadow);

            Color currentWordTextColor = options.CurrentWordTextColor;
            Color currentWordOutlineColor = options.CurrentWordOutlineColor;
            Color currentWordShadowColor = options.CurrentWordShadowColor;

            _chkKaraoke.Checked = options.IsKaraoke;
            _chkHighlightCurrentWord.Checked = !currentWordTextColor.IsEmpty;

            _txtCurrentWordTextColor.Enabled = _chkHighlightCurrentWord.Checked;
            _txtCurrentWordTextColor.Text = _txtCurrentWordTextColor.Enabled ? ColorUtil.ToHtml(currentWordTextColor) : string.Empty;
            _btnPickTextColor.Enabled = _txtCurrentWordTextColor.Enabled;

            _txtCurrentWordOutlineColor.Enabled = _chkHighlightCurrentWord.Checked && style.HasOutline && !style.HasOutlineBox;
            _txtCurrentWordOutlineColor.Text = _txtCurrentWordOutlineColor.Enabled ? ColorUtil.ToHtml(currentWordOutlineColor) : string.Empty;
            _btnPickOutlineColor.Enabled = _txtCurrentWordOutlineColor.Enabled;

            _txtCurrentWordShadowColor.Enabled = _chkHighlightCurrentWord.Checked && style.HasShadow;
            _txtCurrentWordShadowColor.Text = _txtCurrentWordShadowColor.Enabled ? ColorUtil.ToHtml(currentWordShadowColor) : string.Empty;
            _btnPickShadowColor.Enabled = _txtCurrentWordShadowColor.Enabled;

            UpdateStylePreview();
        }

        private void _chkGlow_CheckedChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.Glow, _chkGlow.Checked);
            UpdateStylePreview();
        }

        private void _chkBevel_CheckedChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.Bevel, _chkBevel.Checked);
            UpdateStylePreview();
        }

        private void _chkSoftShadow_CheckedChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.SoftShadow, _chkSoftShadow.Checked);
            UpdateStylePreview();
        }

        private void _chkHardShadow_CheckedChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.SetShadowTypeEnabled(ShadowType.HardShadow, _chkHardShadow.Checked);
            UpdateStylePreview();
        }

        private void _chkKaraoke_CheckedChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.IsKaraoke = _chkKaraoke.Checked;
            _chkHighlightCurrentWord.Enabled = _chkKaraoke.Checked;
            _chkHighlightCurrentWord.Checked = false;
            UpdateStylePreview();
        }

        private void _chkHighlightCurrentWord_CheckedChanged(object sender, EventArgs e)
        {
            AssStyle style = _styles?[SelectedStyleOptions.Name];

            _txtCurrentWordTextColor.Enabled = _chkHighlightCurrentWord.Checked;
            _txtCurrentWordTextColor.Text = _txtCurrentWordTextColor.Enabled ? ColorUtil.ToHtml(style.PrimaryColor) : string.Empty;
            _btnPickTextColor.Enabled = _txtCurrentWordTextColor.Enabled;

            _txtCurrentWordOutlineColor.Enabled = _chkHighlightCurrentWord.Checked && style.HasOutline && !style.HasOutlineBox;
            _txtCurrentWordOutlineColor.Text = _txtCurrentWordOutlineColor.Enabled ? ColorUtil.ToHtml(style.OutlineColor) : string.Empty;
            _btnPickOutlineColor.Enabled = _txtCurrentWordOutlineColor.Enabled;

            _txtCurrentWordShadowColor.Enabled = _chkHighlightCurrentWord.Checked && style.HasShadow;
            _txtCurrentWordShadowColor.Text = _txtCurrentWordShadowColor.Enabled ? ColorUtil.ToHtml(style.ShadowColor) : string.Empty;
            _btnPickShadowColor.Enabled = _txtCurrentWordShadowColor.Enabled;

            UpdateStylePreview();
        }

        private void _txtCurrentWordTextColor_TextChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.CurrentWordTextColor = ColorUtil.FromHtml(_txtCurrentWordTextColor.Text);
            UpdateStylePreview();
        }

        private void _btnPickTextColor_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = ColorUtil.FromHtml(_txtCurrentWordTextColor.Text);
            if (_dlgColor.ShowDialog() == DialogResult.OK)
                _txtCurrentWordTextColor.Text = ColorUtil.ToHtml(_dlgColor.Color);
        }

        private void _txtCurrentWordOutlineColor_TextChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.CurrentWordOutlineColor = ColorUtil.FromHtml(_txtCurrentWordOutlineColor.Text);
            UpdateStylePreview();
        }

        private void _btnPickOutlineColor_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = ColorUtil.FromHtml(_txtCurrentWordOutlineColor.Text);
            if (_dlgColor.ShowDialog() == DialogResult.OK)
                _txtCurrentWordOutlineColor.Text = ColorUtil.ToHtml(_dlgColor.Color);
        }

        private void _txtCurrentWordShadowColor_TextChanged(object sender, EventArgs e)
        {
            SelectedStyleOptions.CurrentWordShadowColor = ColorUtil.FromHtml(_txtCurrentWordShadowColor.Text);
            UpdateStylePreview();
        }

        private void _btnPickShadowColor_Click(object sender, EventArgs e)
        {
            _dlgColor.Color = ColorUtil.FromHtml(_txtCurrentWordShadowColor.Text);
            if (_dlgColor.ShowDialog() == DialogResult.OK)
                _txtCurrentWordShadowColor.Text = ColorUtil.ToHtml(_dlgColor.Color);
        }

        private void UpdateStylePreview()
        {
            AssStyle style = _styles?[SelectedStyleOptions.Name];
            _brwPreview.DocumentText = StylePreviewGenerator.GenerateHtml(style, SelectedStyleOptions);
        }

        private async void _btnConvert_Click(object sender, EventArgs e)
        {
            try
            {
                string outputFilePath;
                if (Path.GetExtension(_txtInputFile.Text).Equals(".ass", StringComparison.InvariantCultureIgnoreCase))
                {
                    AssDocument inputDoc = new AssDocument(_txtInputFile.Text, (List<AssStyleOptions>)_lstStyles.DataSource);
                    YttDocument outputDoc = new YttDocument(inputDoc);
                    outputFilePath = Path.ChangeExtension(_txtInputFile.Text, ".ytt");
                    outputDoc.Save(outputFilePath);
                }
                else
                {
                    SubtitleDocument inputDoc = SubtitleDocument.Load(_txtInputFile.Text);
                    SrtDocument outputDoc = new SrtDocument(inputDoc);
                    outputFilePath = Path.ChangeExtension(_txtInputFile.Text, ".srt");
                    outputDoc.Save(outputFilePath);
                }

                _lblConversionSuccess.Text = string.Format(Resources.SuccessfullyCreated0, Path.GetFileName(outputFilePath));
                _lblConversionSuccess.Visible = true;
                await Task.Delay(4000);
                _lblConversionSuccess.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (GetDroppedFilePath(e) != null)
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            LoadFile(GetDroppedFilePath(e));
        }

        private static string GetDroppedFilePath(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return null;

            string[] filePaths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (filePaths == null || filePaths.Length != 1)
                return null;

            string filePath = filePaths[0];
            string extension = Path.GetExtension(filePath) ?? string.Empty;
            if (!extension.Equals(".ass", StringComparison.InvariantCultureIgnoreCase) && !extension.Equals(".sbv", StringComparison.InvariantCultureIgnoreCase))
                return null;

            return filePath;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AssStyleOptionsList.Save(_styleOptions.Values);
        }
    }
}
