# c1tester no trace start
#   ブロックコメントはスクリプトによって置換される。
class c1tester
{
    hidden [bool] $h_enabled_trace = $true
    hidden [string] $h_pass_file
    hidden [string] $h_report_file
    hidden [bool] $h_enabled_report
    hidden $h_pass
    hidden $h_csv
    hidden $h_report_hash

    c1tester()
    {
        # トレースが無効か
        if (-not $this.h_enabled_trace)
        {
            return
        }

        $this.h_pass = @{}
        $this.h_report_hash = @{}
        $stack = Get-PSCallStack
        $selffilename = Split-Path $stack[1].ScriptName -Leaf
        $this.h_pass_file = '<# replace_pass_filename_format #>' -f '<# replace_product #>', $selffilename

        #　通過ファイルが有るか
        if (Test-Path $this.h_pass_file)
        {
            Remove-Item $this.h_pass_file
        }

        $this.h_report_file = '<# replace_report_filename_format #>' -f '<# replace_product #>', $selffilename
        $this.h_enabled_report = $false

        # 報告ファイルが有るか
        if (Test-Path $this.h_report_file)
        {
            $this.h_csv = Import-Csv $this.h_report_file -Encoding utf8

            # 処理方法を向上するためのハッシュテーブルを作成するループ
            for ($i = 0; $i -lt $this.h_csv.Count; $i++)
            {
                $this.h_report_hash[$this.h_csv[$i].trace_id] = $i
            }

            # 報告ファイルのバックアップが有るか
            if (Test-Path ('_' + $this.h_report_file))
            {
                Remove-Item ('_' + $this.h_report_file)
            }

            Rename-Item $this.h_report_file ('_' + $this.h_report_file)
            $this.h_enabled_report = $true
        }
    }

    [void] trace($trace_id)
    {
        # トレースが無効か
        if (-not $this.h_enabled_trace)
        {
            return
        }

        # 未出力か
        if ($this.h_pass.Keys -notcontains $trace_id)
        {
            $tmp = Get-PSCallStack
            $filename = Split-Path $tmp[1].ScriptName -Leaf
            $content = $filename + ":" + $tmp[1].ScriptLineNumber + "(" + $tmp[1].FunctionName + ") " + (Get-Date -Format 'yyyy/MM/dd HH:mm:ss')
            $trace_id + "," + $content | Out-File ($this.h_pass_file) -Append -Encoding utf8
            $this.h_pass[$trace_id] = $true

            # 報告するか
            if ($this.h_enabled_report)
            {
                # エントリーが有るか
                if ($this.h_report_hash.Keys -contains $trace_id)
                {
                    $index = $this.h_report_hash[$trace_id]

                    # 未パスか
                    if ($this.h_csv[$index].report -eq '-')
                    {
                        $this.h_csv[$index].report = 'P'
                        $this.h_csv[$index].timestamp = $content
                        $this.h_csv | Select-Object -Property trace_id,report,timestamp | Export-Csv -Path $this.h_report_file -Encoding utf8 -NoTypeInformation
                    }
                }
            }
        }
    }
}
# c1tester no trace end

[c1tester]$c1tester = [c1tester]::new()
