    //C1Tester no trace start
    //  ブロックコメントはスクリプトによって置換される。
    public class C1Tester
    {
        private bool m_EnabledTrace = true;
        private string m_PassFile;
        private string m_ReportFile;
        private bool m_EnabledReport;
        private Dictionary<string, bool> m_Pass;
        private Dictionary<string, Dictionary<string, string>> m_Csv = new Dictionary<string, Dictionary<string, string>>();
        private string m_CsvHeader;

        private void ParseCsv(string[] content)
        {
            string[] tmp;
            Dictionary<string, string> tmp2;

            m_CsvHeader = content[0];

            //CSV読み取りループ
            for (int i = 1; i < content.Length; i++)
            {
                tmp = content[i].Split(',');
                tmp2 = new Dictionary<string, string>();
                tmp2["/* replace_report_member */"] = tmp[1].Trim('"');
                tmp2["/* replace_filename_member */"] = tmp[2].Trim('"');
                tmp2["/* replace_line_number_member */"] = tmp[3].Trim('"');
                tmp2["/* replace_function_name_member */"] = tmp[4].Trim('"');
                tmp2["/* replace_timestamp_member */"] = tmp[5].Trim('"');
                m_Csv[tmp[0].Trim('"')] = tmp2;
            }
        }

        public C1Tester(
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            string[] tmp;
            string[] fileContent;
            Dictionary<string, string> tmp2;

            //トレースが無効か
            if (!m_EnabledTrace)
            {
                return;
            }

            m_Pass = new Dictionary<string, bool>();
            string selfFilename = Path.GetFileName(sourceFilePath);
            m_PassFile = String.Format("/* replace_pass_filename_format */", "/* replace_product */", selfFilename);
            File.Delete(m_PassFile);
            m_ReportFile = String.Format("/* replace_report_filename_format */", "/* replace_product */", selfFilename);
            m_EnabledReport = false;

            //報告ファイルが有るか
            if (File.Exists(m_ReportFile))
            {
                fileContent = File.ReadAllLines(m_ReportFile);

                ParseCsv(fileContent);

                File.Delete("_" + m_ReportFile);
                File.Move(m_ReportFile, "_" + m_ReportFile);
                m_EnabledReport = true;
            }
        }

        public void Trace(string traceId,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            string[] content;
            string line;

            //トレースが無効か
            if (!m_EnabledTrace)
            {
                return;
            }

            //未出力か
            if (!m_Pass.Keys.Contains(traceId))
            {
                content = new string[]
                {
                    Path.GetFileName(sourceFilePath),
                    sourceLineNumber.ToString(),
                    memberName,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                };
#if NET
                File.AppendAllTextAsync(m_PassFile, tmp);
#else
                lock (m_PassFile)
                {
                    File.AppendAllText(m_PassFile, traceId + "," + String.Join(",", content) + Environment.NewLine);
                }
#endif
                m_Pass[traceId] = true;

                //報告するか
                if (m_EnabledReport)
                {
                    //エントリーが有るか
                    if (m_Csv.Keys.Contains(traceId))
                    {
                        //未パスか
                        if (m_Csv[traceId]["/* replace_report_member */"] == "-")
                        {
                            m_Csv[traceId]["/* replace_report_member */"] = "P";
                            m_Csv[traceId]["/* replace_filename_member */"] = content[0];
                            m_Csv[traceId]["/* replace_line_number_member */"] = content[1];
                            m_Csv[traceId]["/* replace_function_name_member */"] = content[2];
                            m_Csv[traceId]["/* replace_timestamp_member */"] = content[3];
                        }

                        lock (m_ReportFile)
                        {
                            File.WriteAllText(m_ReportFile, m_CsvHeader + Environment.NewLine);

                            //CSV書き込みループ
                            foreach (string item in m_Csv.Keys)
                            {
                                line = "\"" + item + "\"";

                                //ダブルクォート追加ループ
                                foreach (string item2 in m_Csv[item].Keys)
                                {
                                    line += ",\"" + m_Csv[item][item2] + "\"";
                                }

                                File.AppendAllText(m_ReportFile, line + Environment.NewLine);
                            }
                        }
                    }
                }
            }
        }
    }
    //C1Tester no trace end
