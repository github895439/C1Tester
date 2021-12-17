//この行は処理に必要
//C1Tester no trace start
//  ブロックコメントはスクリプトによって置換される。
class C1Tester
{
    constructor()
    {
        this.enabledTrace = true;
        this.pass = new Map();
        this.passOut;
        this.reportOut;
        this.enabledReport = false;
        this.csv = new Map();
        this.csvHeader;
    }

    trimX(str)
    {
        let tmp = "";
        let rtn = "";

        //前ダブルクォート削除ループ
        for (let index = 0, noflag = true; index < str.length; index++)
        {
            //ダブルクォート、かつ、データ内ではないか
            if ((str[index] == "\"") && noflag)
            {
                continue;
            }

            tmp += str[index];
            noflag = false;
        }

        //後ダブルクォート削除ループ
        for (let index = tmp.length - 1, noflag = true; index >= 0; index--)
        {
            //ダブルクォート、かつ、データ内ではないか
            if ((tmp[index] == "\"") && noflag)
            {
                continue;
            }

            rtn = tmp[index] + rtn;
            noflag = false;
        }
        
        return rtn;
    }

    c1TesterParseX(content)
    {
        let tmp;
        let tmp2;

        this.csvHeader = content[0];

        //CSV読み取りループ
        for (let index = 1; index < content.length; index++)
        {
            //空行か
            if (content[index] == "")
            {
                continue;
            }

            tmp = content[index].split(',');
            tmp2 = new Map();
            tmp2.set("report", this.trimX(tmp[1]));
            tmp2.set("filename", this.trimX(tmp[2]));
            tmp2.set("line_number", this.trimX(tmp[3]));
            tmp2.set("function_name", this.trimX(tmp[4]));
            tmp2.set("timestamp", this.trimX(tmp[5]));
            this.csv.set(this.trimX(tmp[0]), tmp2);
        }
    }

    initialize()
    {
        //トレースが無効か
        if (!this.enabledTrace)
        {
            return;
        }

        this.passOut = document.getElementById("c1TesterPass");
        this.passOut.value = "";
        this.reportOut = document.getElementById("c1TesterReport");

        //報告先が有るか
        if ((this.reportOut != undefined) && (this.reportOut.value != ""))
        {
            let content = this.reportOut.value.split("\n");

            this.c1TesterParseX(content);

            this.reportOut.value = "";
            this.enabledReport = true;
        }
    }

    c1TesterStringifyX()
    {
        let line;
        let rtn = "";
        rtn += this.csvHeader + "\n";

        for (let [key, value] of this.csv)
        {
            line = "\"" + key + "\"";

            for (let value2 of value.values())
            {
                line += ",\"" + value2 + "\"";
            }

            rtn += line + "\n";
        }

        return rtn;
    }

    trace(traceId)
    {
        //トレースが無効か
        if (!this.enabledTrace)
        {
            return;
        }

        //未出力か
        if (!this.pass.has(traceId))
        {
            let stack = new Error().stack;
            let tmp1 = stack.split("\n");
            let tmp2 = tmp1[2].split(" ");
            let functionName = tmp2[5];
            let tmp3 = tmp2[6].split(":");
            let lineNumber = tmp3[3];
            let tmp4 = tmp3[2].split("/");
            let filename = tmp4[tmp4.length - 1];
            let tmpDate = new Date();
            let content =
            [
                filename,
                lineNumber,
                functionName,
                tmpDate.toLocaleString()
            ];
            this.passOut.value += traceId + "," + content.join(",") + "\n";
            this.pass.set(traceId, true);

            //報告するか
            if (this.enabledReport)
            {
                //エントリーが有るか
                if (this.csv.has(traceId))
                {
                    let report = this.csv.get(traceId);

                    //未パスか
                    if (report.get("/* replace_report_member */") == "-")
                    {
                        report.set("/* replace_report_member */", "P");
                        report.set("/* replace_filename_member */", content[0]);
                        report.set("/* replace_line_number_member */", content[1]);
                        report.set("/* replace_function_name_member */", content[2]);
                        report.set("/* replace_timestamp_member */", content[3]);
                    }

                    let contentCsv = this.c1TesterStringifyX();
                    this.reportOut.value = contentCsv;
                }
            }
        }
    }
}
//C1Tester no trace end

var c1tester = new C1Tester();
c1tester.initialize();
