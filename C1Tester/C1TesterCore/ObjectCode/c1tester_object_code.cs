    //C1Tester no trace start
    //  ブロックコメントはスクリプトによって置換される。
    public class C1Tester
    {
        private bool m_EnabledTrace = true;
        private string m_PassFile;
        private string m_ReportFile;
        private bool m_EnabledReport;
        private Dictionary<string, bool> m_Pass;
        private Dictionary<string, Report> m_Csv = new Dictionary<string, Report>();
        private string m_CsvHeader;

        private class Report
        {
            public string m_Report;
            public string m_Timestamp;
        }

        public C1Tester(
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            string[] tmp;
            string[] fileContent;
            Report tmp2;

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
                m_CsvHeader = fileContent[0];

                //CSV読み取りループ
                for (int i = 1; i < fileContent.Length; i++)
                {
                    tmp = fileContent[i].Split(',');
                    tmp2 = new Report() { m_Report = tmp[1].Trim('"'), m_Timestamp = tmp[2].Trim('"') };
                    m_Csv[tmp[0].Trim('"')] = tmp2;
                }

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
            string content;
            string line;

            //トレースが無効か
            if (!m_EnabledTrace)
            {
                return;
            }

            //未出力か
            if (!m_Pass.Keys.Contains(traceId))
            {
                content = Path.GetFileName(sourceFilePath) + ":" + sourceLineNumber + "(" + memberName + ") " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
#if NET
                File.AppendAllTextAsync(m_PassFile, tmp);
#else
                lock (m_PassFile)
                {
                    File.AppendAllText(m_PassFile, traceId + "," + content + Environment.NewLine);
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
                        if (m_Csv[traceId].m_Report == "-")
                        {
                            m_Csv[traceId].m_Report = "P";
                            m_Csv[traceId].m_Timestamp = content;
                        }

                        lock (m_ReportFile)
                        {
                            File.WriteAllText(m_ReportFile, m_CsvHeader + Environment.NewLine);

                            //CSV書き込みループ
                            foreach (string item in m_Csv.Keys)
                            {
                                line = "\"" + item + "\",\"" + m_Csv[item].m_Report + "\",\"" + m_Csv[item].m_Timestamp + "\"";
                                File.AppendAllText(m_ReportFile, line + Environment.NewLine);
                            }
                        }
                    }
                }
            }
        }
    }
    //C1Tester no trace end
