using C1Tester.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace C1Tester
{
    public enum Progress
    {
        Status
    }

    public partial class Form1 : Form
    {
        private string m_ConfigFullPath = string.Empty;
        private string m_Progress;

        private Config m_Config = new Config();

        public Form1()
        {
            InitializeComponent();

            textBox2.Text = m_Config.m_PowershellPath;
            toolStripStatusLabel1.Text = string.Empty;
        }

        private void SaveConfig()
        {
            //フォーム→設定オブジェクトに設定を格納
            SetFromFormToConfig();

            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using (StreamWriter sw = new StreamWriter(m_ConfigFullPath, false, new UTF8Encoding(false)))
            {
                serializer.Serialize(sw, m_Config);
                sw.Close();
            }
            MessageBox.Show(Resources.MSGBOX_INFO_WRITE_CONFIG_FILE);
        }

        /// <summary>
        /// 「書込」ボタンハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = @".";
                saveFileDialog.Filter = "XML files (*.xml)|*.xml";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                //キャンセルしたか
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                m_ConfigFullPath = saveFileDialog.FileName;
            }

            textBox1.Text = m_ConfigFullPath;

            //フォーム→設定オブジェクトに設定を格納
            SetFromFormToConfig();

            //設定をファイルに保存
            SaveConfig();
        }

        /// <summary>
        /// 「読込」ボタンハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult rtn = MessageBox.Show(Resources.MSGBOX_CONFIRM_REMOVE_FORM_CONFIG, "確認", MessageBoxButtons.YesNo);

            //フォーム設定を破棄してはダメか
            if (rtn == DialogResult.No)
            {
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @".";
                openFileDialog.Filter = "XML files (*.xml)|*.xml";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                //キャンセルしたか
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                m_ConfigFullPath = openFileDialog.FileName;
            }

            textBox1.Text = m_ConfigFullPath;
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using (StreamReader sw = new StreamReader(textBox1.Text, new UTF8Encoding(false)))
            {
                m_Config = (Config)serializer.Deserialize(sw);
                sw.Close();
            }

            //フォーム設定を初期化
            InitializeForm();

            //オブジェクト→フォームに設定を格納
            SetFromConfigToForm();

            MessageBox.Show(Resources.MSGBOX_INFO_READ_CONFIG_FILE);
        }

        private void InitializeForm()
        {
            textBox2.Text = string.Empty;
            textBox4.Text = string.Empty;
            dataGridView1.Rows.Clear();
        }

        private void SetFromConfigToForm()
        {
            textBox2.Text = m_Config.m_PowershellPath;
            textBox4.Text = m_Config.m_SourceFolderPath;
            checkBox2.Checked = m_Config.m_ScriptOption_Overwrite;
            checkBox6.Checked = m_Config.m_ScriptOption_NotRemainBackup;
            checkBox3.Checked = m_Config.m_ScriptOption_Report;

            //グリッドへ格納ループ
            for (int i = 0; i < m_Config.m_TargetList.Count; i++)
            {
                dataGridView1.Rows.Add();
                DataGridViewRow row = dataGridView1.Rows[i];
                row.Cells[0].ValueType = typeof(bool);

                //1列目が選択状態か
                if (m_Config.m_TargetList[i].m_Target)
                {
                    row.Cells[0].Value = true;
                }
                else
                {
                    row.Cells[0].Value = false;
                }

                row.Cells[1].ValueType = typeof(bool);

                //2列目が選択状態か
                if (m_Config.m_TargetList[i].m_AddObj)
                {
                    row.Cells[1].Value = true;
                }
                else
                {
                    row.Cells[1].Value = false;
                }

                row.Cells[2].ValueType = typeof(string);
                row.Cells[2].Value = m_Config.m_TargetList[i].m_FilePath;
            }
        }

        private void SetFromFormToConfig()
        {
            DataGridViewCellCollection cell;

            //設定オブジェクトを初期化
            m_Config.Initialize();

            m_Config.m_PowershellPath = textBox2.Text;
            m_Config.m_SourceFolderPath = textBox4.Text;
            m_Config.m_ScriptOption_Overwrite = checkBox2.Checked;
            m_Config.m_ScriptOption_NotRemainBackup = checkBox6.Checked;
            m_Config.m_ScriptOption_Report = checkBox3.Checked;

            //グリッドの行ループ
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                cell = dataGridView1.Rows[i].Cells;
                Target target = new Target();
                target.m_Target = Convert.ToBoolean(cell[0].Value);
                target.m_AddObj = Convert.ToBoolean(cell[1].Value);
                target.m_FilePath = (string)cell[2].Value;
                m_Config.m_TargetList.Add(target);
            }
        }

        /// <summary>
        /// 「上書き」ボタンハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click_1(object sender, EventArgs e)
        {
            //設定ファイルが指定されていないか
            if (textBox1.Text == string.Empty)
            {
                MessageBox.Show(Resources.MSGBOX_ERROR_NOT_SPECIFIED_CONFIG_FILE);
                return;
            }

            m_ConfigFullPath = textBox1.Text;

            //設定をファイルに保存
            SaveConfig();
        }

        /// <summary>
        /// 「フォルダ選択」ボタンハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            string sourceFolderPath;

            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.ShowNewFolderButton = false;

                //既にフォルダが指定されているか
                if (textBox4.Text != string.Empty)
                {
                    folderBrowserDialog.SelectedPath = textBox4.Text;
                }

                //フォルダが選択されなかったか
                if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                DialogResult result = MessageBox.Show(Resources.MSGBOX_CONFIRM_REMOVE_GRID_CONFIG, "確認", MessageBoxButtons.YesNo);

                //No応答か
                if (result == DialogResult.No)
                {
                    return;
                }

                sourceFolderPath = folderBrowserDialog.SelectedPath;
            }

            //フォーム無効化
            DisableForm();

            textBox4.Text = sourceFolderPath;
            dataGridView1.Rows.Clear();
            button7.Enabled = true;
            backgroundWorker1.RunWorkerAsync(sourceFolderPath);
        }

        private void DisableForm()
        {
            textBox1.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            textBox2.Enabled = false;
            checkBox2.Enabled = false;
            checkBox6.Enabled = false;
            button5.Enabled = false;
            textBox4.Enabled = false;
            checkBox1.Enabled = false;
            dataGridView1.Enabled = false;
            button6.Enabled = false;
        }

        /// <summary>
        /// 対象ソースファイル探索サブスレッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int rtn;

            BackgroundWorker self = (BackgroundWorker)sender;
            List<string> targetList = new List<string>();
            string sourceFolderPath = (string)e.Argument;

            //対応拡張子取得
            rtn = GetSupportingExtension();

            //失敗したか
            if (rtn != 0)
            {
                MessageBox.Show(Resources.MSGBOX_ERROR_FAILED_RUN_POWERSHELL_SCRIPT + "(" + rtn.ToString() + ")");
                self.CancelAsync();
                return;
            }

            //対象ソースファイル探索
            SearchTarget(self, sourceFolderPath, ref targetList);

            //中止ではないか
            if (!self.CancellationPending)
            {
                e.Result = targetList;
            }
        }

        private int GetSupportingExtension()
        {
            int rtn = 0;
            Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = m_Config.m_PowershellPath;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.CreateNoWindow = true;
            //パスに空白がある場合に備えて「@」は使わない
            //(「@」を使うと「"」を「\」でエスケープできなくなる。)
            p.StartInfo.Arguments = "\".\\" + Resources.POWERSHELL_SCRIPT + "\" -print_supporting_extension";
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            rtn = p.ExitCode;
            p.Close();

            //成功したか
            if (rtn == 0)
            {
                m_Config.m_Extension = output.Split(new char[] { ' ' });

                //改行削除ループ
                for (int i = 0; i < m_Config.m_Extension.Length; i++)
                {
                    m_Config.m_Extension[i] = m_Config.m_Extension[i].Replace(Environment.NewLine, string.Empty);
                }
            }

            return rtn;
        }

        private void SearchTarget(BackgroundWorker self, string folder, ref List<string> targetList)
        {
            //中止か
            if (self.CancellationPending)
            {
                return;
            }

            m_Progress = folder;
            self.ReportProgress((int)Progress.Status);

            string[] subFolder = Directory.GetDirectories(folder);

            //サブフォルダー探索ループ
            for (int i = 0; i < subFolder.Length; i++)
            {
                SearchTarget(self, subFolder[i], ref targetList);
            }

            string[] file;

            //各拡張子探索ループ
            for (int i = 0; i < m_Config.m_Extension.Length; i++)
            {
                file = Directory.GetFiles(folder, "*" + m_Config.m_Extension[i]);

                //対象ソースファイル追加ループ
                for (int j = 0; j < file.Length; j++)
                {
                    targetList.Add(file[j]);
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorker self = (BackgroundWorker)sender;

            //進捗選択
            switch ((Progress)e.ProgressPercentage)
            {
                case Progress.Status:
                {
                    toolStripStatusLabel1.Text = m_Progress;
                    break;
                }
                default:
                {
                    throw new Exception();
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker self = (BackgroundWorker)sender;

            //異常終了か
            if (e.Error != null)
            {
                //###
            }
            //キャンセルか
            else if (self.CancellationPending)
            {
                //###
            }
            else
            {
                //結果があるか
                if (e.Result != null)
                {
                    List<string> tmp = (List<string>)e.Result;

                    //グリッドに追加ループ
                    for (int i = 0; i < tmp.Count; i++)
                    {
                        dataGridView1.Rows.Add();
                        dataGridView1.Rows[i].Cells[0].ValueType = typeof(bool);
                        dataGridView1.Rows[i].Cells[0].Value = false;
                        dataGridView1.Rows[i].Cells[1].ValueType = typeof(bool);
                        dataGridView1.Rows[i].Cells[1].Value = false;
                        dataGridView1.Rows[i].Cells[2].ValueType = typeof(string);
                        dataGridView1.Rows[i].Cells[2].Value = tmp[i];
                    }
                }
            }

            button7.Enabled = false;
            toolStripStatusLabel1.Text = string.Empty;

            //フォーム有効化
            EnableForm();

	        MessageBox.Show(Resources.MSGBOX_INFO_SEARCH_FILE);
        }

        private void EnableForm()
        {
            textBox1.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            textBox2.Enabled = true;
            checkBox2.Enabled = true;
            checkBox6.Enabled = true;
            button5.Enabled = true;
            textBox4.Enabled = true;
            checkBox1.Enabled = true;
            dataGridView1.Enabled = true;
            button6.Enabled = true;
        }

        /// <summary>
        /// 「ファイル探索中止」ボタンハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        /// <summary>
        /// 「全対象」チェックボックスハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_Click(object sender, EventArgs e)
        {
            CheckBox self = (CheckBox)sender;

            //リストが列挙されていないか
            if (dataGridView1.Rows.Count == 0)
            {
                self.Checked = false;
                return;
            }

            //全対象を変更するループ
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                //「全対象」チェックボックスがチェック入りか
                if (self.Checked)
                {
                    dataGridView1.Rows[i].Cells[0].Value = true;
                }
                else
                {
                    dataGridView1.Rows[i].Cells[0].Value = false;
                    dataGridView1.Rows[i].Cells[1].Value = false;
                }
            }
        }

        /// <summary>
        /// 「トレース埋込実行」ボタンハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            //フォーム→設定オブジェクトに設定を格納
            SetFromFormToConfig();

            //リストが空か
            if (m_Config.m_TargetList.Count == 0)
            {
                MessageBox.Show(Resources.MSGBOX_ERROR_NOT_SELECT_SOURCE);
                return;
            }

            List<Target> tmp = new List<Target>();

            //選択抽出ループ
            for (int i = 0; i < m_Config.m_TargetList.Count; i++)
            {
                //選択されているか、オブジェクト追加か
                if (m_Config.m_TargetList[i].m_Target || m_Config.m_TargetList[i].m_AddObj)
                {
                    tmp.Add(m_Config.m_TargetList[i]);
                }
            }

            //ソースが選択されていないか
            if (tmp.Count == 0)
            {
                MessageBox.Show(Resources.MSGBOX_ERROR_NOT_SELECT_SOURCE);
                return;
            }

            //フォーム無効化
            DisableForm();

            button4.Enabled = true;
            backgroundWorker2.RunWorkerAsync(tmp);
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            int rtn;

            BackgroundWorker self = (BackgroundWorker)sender;
            List<Target> targetList = (List<Target>)e.Argument;

            //トレース埋込ループ
            for (int i = 0; i < targetList.Count; i++)
            {
                m_Progress = i.ToString() + "/" + targetList.Count.ToString() + " " + targetList[i].m_FilePath;
                self.ReportProgress((int)Progress.Status);

                //トレース埋込
                rtn = AddTrace(targetList[i]);

                //失敗したか
                if (rtn != 0)
                {
                    string message =
                        Resources.MSGBOX_ERROR_FAILED_RUN_POWERSHELL_SCRIPT + Environment.NewLine +
                        String.Format(Resources.MSGBOX_ERROR_LINE_1, targetList[i].m_FilePath) + Environment.NewLine +
                        String.Format(Resources.MSGBOX_ERROR_LINE_2, rtn);
                    MessageBox.Show(message);
                    DialogResult result = MessageBox.Show(Resources.MSGBOX_CONFIRM_STOP_ADD_TRACE, "確認", MessageBoxButtons.YesNo);

                    //Yes応答か
                    if (result == DialogResult.Yes)
                    {
                        self.CancelAsync();
                        return;
                    }
                }
            }
        }

        private int AddTrace(Target target)
        {
            int rtn = 0;
            Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = m_Config.m_PowershellPath;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.CreateNoWindow = true;
            //パスに空白がある場合に備えて「@」は使わない
            //(「@」を使うと「"」を「\」でエスケープできなくなる。)
            p.StartInfo.Arguments = "\".\\" + Resources.POWERSHELL_SCRIPT + "\" -silent";

            //force_write_backupオプション有りか
            if (m_Config.m_ScriptOption_Overwrite)
            {
                p.StartInfo.Arguments += " -force_write_backup";
            }

            //no_remain_backupオプション有りか
            if (m_Config.m_ScriptOption_NotRemainBackup)
            {
                p.StartInfo.Arguments += " -no_remain_backup";
            }

            //reportオプション有りか
            if (m_Config.m_ScriptOption_Report)
            {
                p.StartInfo.Arguments += " -report";
            }

            //add_object_codeオプション有りか
            if (target.m_AddObj)
            {
                p.StartInfo.Arguments += " -add_object_code";
            }

            p.StartInfo.Arguments += " " + "\"" + target.m_FilePath + "\"";
            p.Start();
            p.WaitForExit();
            rtn = p.ExitCode;
            p.Close();
            return rtn;
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorker self = (BackgroundWorker)sender;

            //進捗選択
            switch ((Progress)e.ProgressPercentage)
            {
                case Progress.Status:
                {
                    toolStripStatusLabel1.Text = m_Progress;
                    break;
                }
                default:
                {
                    throw new Exception();
                }
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker self = (BackgroundWorker)sender;

            //異常終了か
            if (e.Error != null)
            {
                //###
            }
            //キャンセルか
            else if (self.CancellationPending)
            {
                //###
            }
            else
            {
                //###
            }

            button4.Enabled = false;
            toolStripStatusLabel1.Text = string.Empty;

            //フォーム有効化
            EnableForm();

	        MessageBox.Show(Resources.MSGBOX_INFO_ADD_TRACE);
        }
    }
}
