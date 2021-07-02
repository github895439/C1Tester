//この行は処理に必要
//C1Tester no trace start
//  ブロックコメントはスクリプトによって置換される。
const c1TesterFs = require("fs");
const c1TesterParse = require('csv-parse/lib/sync')
const c1TesterStringify = require('csv-stringify/lib/sync')

function prepareStackTraceForC1Tester(error, structuredStackTrace)
{
    let tmp = structuredStackTrace[0].getFileName().split("\\");
    c1tester.filename = tmp[tmp.length - 1];
    c1tester.lineNumber = structuredStackTrace[0].getLineNumber();
    c1tester.functionName = structuredStackTrace[0].getFunctionName();
}

class C1Tester
{
    constructor()
    {
        this.enabledTrace = true;
        this.pass = new Map();
        this.passFile = "";
        this.reportFile = "";
        this.filename = "";
        this.lineNumber = "";
        this.functionName = "";
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

        let backup = Error.prepareStackTrace;
        Error.captureStackTrace(this, this.initialize);
        Error.prepareStackTrace = prepareStackTraceForC1Tester;
        this.stack;
        Error.prepareStackTrace = backup;
        let currentFolder = path.dirname(process.argv[2]);
        this.passFile = "/* replace_pass_filename_format */";
        this.passFile = this.passFile.replace("\{0\}", "/* replace_product */");
        this.passFile = this.passFile.replace("\{1\}", this.filename);
        this.passFile = currentFolder + "/" + this.passFile;
        this.reportFile = "/* replace_report_filename_format */";
        this.reportFile = this.reportFile.replace("\{0\}", "/* replace_product */");
        this.reportFile = this.reportFile.replace("\{1\}", this.filename);
        this.reportFile = currentFolder + "/" + this.reportFile;

        try
        {
            c1TesterFs.unlinkSync(this.passFile);
        }
        catch (error)
        {
            //nop
        }

        //報告ファイルが有るか
        if (c1TesterFs.existsSync(this.reportFile))
        {
            let content = c1TesterFs.readFileSync(this.reportFile, "utf-8");
            this.csv = c1TesterParse(content,
                {
                    columns: ["traceId", "report", "timestamp"]
                });

            //処理方法を向上するためのハッシュテーブルを作成するループ
            for (let index = 0; index < this.csv.length; index++)
            {
                this.reportHash.set(this.csv[index].traceId, index);
            }

            try
            {
                c1TesterFs.unlinkSync("_" + this.reportFile);
                c1TesterFs.renameSync(this.reportFile, "_" + this.reportFile);
            }
            catch (error)
            {
                //nop
            }

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
            let backup = Error.prepareStackTrace;
            Error.captureStackTrace(this, this.trace);
            Error.prepareStackTrace = this.prepareStackTraceForC1Tester;
            this.stack;
            Error.prepareStackTrace = backup;
            let tmpDate = new Date();
            let content = this.filename + ":" + this.lineNumber + "(" + this.functionName + ") " + tmpDate.toLocaleString() + "\n";
            c1TesterFs.writeFile(this.passFile, traceId + "," + content, { flag: "a+" }, (err) => { });
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
                        this.csv[index].report = content;
                    }

                    let contentCsv = c1TesterStringify(this.csv, { quoted: true, header: true });
                    c1TesterFs.writeFile(this.reportFile, contentCsv, { flag: "w" }, (err) => { });
                }
            }
        }
    }
}
//C1Tester no trace end

var c1tester = new C1Tester();
c1tester.initialize();
