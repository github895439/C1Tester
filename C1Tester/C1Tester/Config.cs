using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace C1Tester
{
    public class Config
    {
        public string m_PowershellPath = @"D:\TOOL\PowerShell-7.0.6-win-x64\pwsh.exe";
        public bool m_ScriptOption_Overwrite = false;
        public bool m_ScriptOption_NotRemainBackup = false;
        public bool m_ScriptOption_Report = false;
        public bool m_ScriptOption_NotNode = false;
        public string m_SourceFolderPath = string.Empty;
        public string[] m_Extension;
        public List<Target> m_TargetList = new List<Target>();

        public void Initialize()
        {
            m_SourceFolderPath = string.Empty;
            m_TargetList.Clear();
        }
    }
}
