//この行は処理に必要
//C1Tester no trace start
//  ブロックコメントはスクリプトによって置換される。
const c1TesterPath = require('path');
const c1TesterFs = require("fs");
const c1TesterParse = require('csv-parse/lib/sync')
const c1TesterStringify = require('csv-stringify/lib/sync')

class C1Tester
{
    constructor()
    {
        this.enabledTrace = true;
        this.pass = new Map();
        this.passFile = "";
        this.reportFile = "";
        this.filename = "";
        this.enabledReport = false;
        this.csv = null;
        this.reportHash = new Map();
    }

    initialize()
    {
        //トレースが無効か
        if (!this.enabledTrace)
        {
            return;
        }

        let tmp = __filename.split("\\");
        this.filename = tmp[tmp.length - 1];
        let selfFolder = c1TesterPath.dirname(process.argv[1]);
        this.passFile = "/* replace_pass_filename_format */";
        this.passFile = this.passFile.replace("\{0\}", "/* replace_product */");
        this.passFile = this.passFile.replace("\{1\}", this.filename);
        this.passFile = selfFolder + "/" + this.passFile;
        this.reportFile = "/* replace_report_filename_format */";
        this.reportFile = this.reportFile.replace("\{0\}", "/* replace_product */");
        this.reportFile = this.reportFile.replace("\{1\}", this.filename);
        let reportFileBackup = selfFolder + "/_" + this.reportFile;
        this.reportFile = selfFolder + "/" + this.reportFile;
        c1TesterFs.unlinkSync(this.passFile);

        //報告ファイルが有るか
        if (c1TesterFs.existsSync(this.reportFile))
        {
            let content = c1TesterFs.readFileSync(this.reportFile, "utf-8");
            this.csv = c1TesterParse(content, {columns: true});

            //処理速度を向上するためのハッシュテーブルを作成するループ
            for (let index = 0; index < this.csv.length; index++)
            {
                this.reportHash.set(this.csv[index].trace_id, index);
            }

            c1TesterFs.unlinkSync(reportFileBackup);
            c1TesterFs.renameSync(this.reportFile, reportFileBackup);
            this.enabledReport = true;
        }
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
            let lineNumber = tmp3[2];
            let tmp4 = tmp3[1].split("\\");
            let filename = tmp4[tmp4.length - 1];
            let tmpDate = new Date();
            let content =
            [
                filename,
                lineNumber,
                functionName,
                tmpDate.toLocaleString()
            ];
            c1TesterFs.writeFileSync(this.passFile, traceId + "," + content.join(",") + "\n", { flag: "a+" });
            this.pass.set(traceId, true);

            //報告するか
            if (this.enabledReport)
            {
                //エントリーが有るか
                if (this.reportHash.has(traceId))
                {
                    let index = this.reportHash.get(traceId);

                    //未パスか
                    if (this.csv[index].report == "-")
                    {
                        this.csv[index].report = "P";
                        this.csv[index].filename = content[0];
                        this.csv[index].line_number = content[1];
                        this.csv[index].function_name = content[2];
                        this.csv[index].timestamp = content[3];
                    }

                    let contentCsv = c1TesterStringify(this.csv, { quoted: true, header: true });
                    c1TesterFs.writeFileSync(this.reportFile, contentCsv, { flag: "w" });
                }
            }
        }
    }
}
//C1Tester no trace end

var c1tester = new C1Tester();
c1tester.initialize();
