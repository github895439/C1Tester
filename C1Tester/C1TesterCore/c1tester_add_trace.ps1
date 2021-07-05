Param
(
    [switch]$force_write_backup,
    [switch]$silent,
    [switch]$add_object_code,
    [switch]$print_supporting_extension,
    [switch]$no_remain_backup,
    [switch]$report
)

# インデックスがエラーコード
$c__error =
@(
    'トレース追加と番号出力を完了しました。',
    '対応していない拡張子です。',
    'ソースファイルが存在しません。',
    'バックアップファイルが存在します。',
    'トレース追加済みです。'
)

$c__setting =
@{
    'product'                = 'c1tester';
    'pass_filename_format'   = '{0}_{1}_pass.csv';
    'report_filename_format' = '{0}_{1}_report.csv';
}

$c__extension =
@{
    '.cs'  =
    @{
        'regex_search'         = '^m_C1Tester\.Trace';
        'regex_trace_position' = '^[ ]*{';
        'add'                  = 'm_C1Tester.Trace("{0}");';
        'char_code_page'       = 'utf8BOM';
        'object_code'          = 'c1tester_object_code.cs';
        'line_comment'         = '^[ ]*//';
        'regex_ignore'         =
        @(
            '^[ ]*namespace',
            '^[ ]*using',
            '^[ ]*try',
            '^[ ]*switch',
            ' class',
            ' struct',
            ' enum'
        )
    };
    '.ps1' =
    @{
        'regex_search'         = '^\$c1tester\.trace';
        'regex_trace_position' = '^[ ]*{';
        'add'                  = '$c1tester.trace("{0}")';
        'char_code_page'       = 'utf8BOM';
        'object_code'          = 'c1tester_object_code.ps1';
        'line_comment'         = '^[ ]*# ';
        'regex_ignore'         =
        @(
            '^[ ]*try',
            '^[ ]*switch',
            '^[ ]*class',
            ' enum'
        )
    };
    '.js'  =
    @{
        'regex_search'         = '^c1tester\.trace';
        'regex_trace_position' = '^[ ]*{';
        'add'                  = 'c1tester.trace("{0}");';
        'char_code_page'       = 'utf8';
        'object_code'          = 'c1tester_object_code.js';
        'line_comment'         = '^[ ]*//';
        'regex_ignore'         =
        @(
            '^[ ]*try',
            '^[ ]*switch',
            '^[ ]*class',
            ' enum'
        )
    };
}

# 共通終了処理関数
function Exiter($code, $error_info)
{
    # silentオプション無しか
    if (-not $silent)
    {
        Write-Host $c__error[$code]

        # 異常終了か
        if ($code -ne 0)
        {
            Write-Host 'オプションは「pwsh -c Get-Help <パス付きスクリプトファイル名>」を参照して下さい。'

            # 想定外か
            if ($code -eq -1)
            {
                $tmp = "エラー情報:" + $error_info
                Write-Host $tmp
                Get-PSCallStack | Write-Host
            }
        }
    }

    exit $code
}

# powershell専用処理関数
function for_ps1($work)
{
    # 処理分岐
    switch ($work)
    {
        # オブジェクトコード追加
        0
        {
            $source_new = Get-Content $source_path

            # 実行前に追加済みか判断するループ
            for ($i = 0; $i -lt $source_new.Count; $i++)
            {
                # オブジェクトコードのコメントか
                if ($source_new[$i] -match '^# c1tester no trace start')
                {
                    return
                }
            }

            Remove-Item $source_path
            $object_path = (Convert-Path .) + '\ObjectCode\' + $c__extension[$extension]['object_code']
            $object_code = Get-Content $object_path

            # オブジェクトコード追加ループ
            for ($i = 0; $i -lt $object_code.Count; $i++)
            {
                $tmp = $object_code[$i] -replace '<# replace_product #>', $c__setting['product']
                $tmp = $tmp -replace '<# replace_pass_filename_format #>', $c__setting['pass_filename_format']
                $tmp = $tmp -replace '<# replace_report_filename_format #>', $c__setting['report_filename_format']
                $tmp | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']
            }

            # 追加前コード追加ループ
            for ($i = 0; $i -lt $source_new.Count; $i++)
            {
                $source_new[$i] | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']
            }

            break
        }
        1
        {
            # 不トレースブロックの開始か
            if ($line2 -match '^# c1tester no trace start')
            {
                $global:no_trace = $true
                break
            }

            # 不トレースブロックの終了か
            if ($line2 -match '^# c1tester no trace end')
            {
                $global:no_trace = $false
                break
            }

            break
        }
        Default
        {
            Exiter -1 $work
            break
        }
    }
}

# C#専用処理関数
function for_cs($work)
{
    # 処理分岐
    switch ($work)
    {
        # オブジェクトコード追加
        0
        {
            # add_object_codeオプションが有るか
            if ($add_object_code)
            {
                $source_new = Get-Content $source_path

                # 実行前に追加済みか判断するループ
                for ($i = 0; $i -lt $source_new.Count; $i++)
                {
                    # オブジェクトコードのコメントか
                    if ($source_new[$i] -match '^[ ]*//C1Tester no trace start')
                    {
                        return
                    }
                }

                Remove-Item $source_path
                $source_new[0] | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']
                $done = $false

                # オブジェクトコード追加位置探索ループ
                for ($i = 1; $i -lt $source_new.Count - 1; $i++)
                {
                    $source_new[$i] | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']
                    $line1 = $source_new[$i - 1]
                    $line2 = $source_new[$i]

                    # 追加場所、かつ、未追加か
                    if (($line1 -match '^namespace') -and ($line2 -match '^\{') -and (-not $done))
                    {
                        $object_path = (Convert-Path .) + '\ObjectCode\' + $c__extension[$extension]['object_code']
                        $object_code = Get-Content $object_path
                        
                        # オブジェクトコード追加ループ
                        for ($j = 0; $j -lt $object_code.Count; $j++)
                        {
                            $tmp = $object_code[$j] -replace '/\* replace_product \*/', $c__setting['product']
                            $tmp = $tmp -replace '/\* replace_pass_filename_format \*/', $c__setting['pass_filename_format']
                            $tmp = $tmp -replace '/\* replace_report_filename_format \*/', $c__setting['report_filename_format']
                            $tmp | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']
                        }
                    }
                }

                $source_new[$i] | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']
            }

            break
        }
        1
        {
            # 不トレースブロックの開始か
            if ($line2 -match '^[ ]*//C1Tester no trace start')
            {
                $global:no_trace = $true
                break
            }

            # 不トレースブロックの終了か
            if ($line2 -match '^[ ]*//C1Tester no trace end')
            {
                $global:no_trace = $false
                break
            }

            break
        }
        2
        {
            $line3b = ''

            # 3行目以降のコメントではない行を探索するループ
            for ($j = 1; $i + $j -lt $source.length; $j++)
            {
                $line = $source[$i + $j]
        
                # コメントではないか
                if ($line -notmatch $c__extension[$extension]['line_comment'])
                {
                    $line3b = $line
                    break
                }
            }

            return (($line3b -ne '') -and ($line3b -match '[,:\]\}]$'))
        }
        Default
        {
            Exiter -1 $work
            break
        }
    }
}

# Javascript専用処理関数
function for_js($work)
{
    # 処理分岐
    switch ($work)
    {
        # オブジェクトコード追加
        0
        {
            # add_object_codeオプションが有るか
            if ($add_object_code)
            {
                $source_new = Get-Content $source_path

                # 実行前に追加済みか判断するループ
                for ($i = 0; $i -lt $source_new.Count; $i++)
                {
                    # オブジェクトコードのコメントか
                    if ($source_new[$i] -match '^[ ]*//C1Tester no trace start')
                    {
                        return
                    }
                }

                Remove-Item $source_path
                $object_path = (Convert-Path .) + '\ObjectCode\' + $c__extension[$extension]['object_code']
                $object_code = Get-Content $object_path

                # オブジェクトコード追加ループ
                for ($i = 0; $i -lt $object_code.Count; $i++)
                {
                    $tmp = $object_code[$i] -replace '/\* replace_product \*/', $c__setting['product']
                    $tmp = $tmp -replace '/\* replace_pass_filename_format \*/', $c__setting['pass_filename_format']
                    $tmp = $tmp -replace '/\* replace_report_filename_format \*/', $c__setting['report_filename_format']
                    $tmp | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']
                }

                # 追加前コード追加ループ
                for ($i = 0; $i -lt $source_new.Count; $i++)
                {
                    $source_new[$i] | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']
                }
            }

            break
        }
        1
        {
            # 不トレースブロックの開始か
            if ($line2 -match '^[ ]*//C1Tester no trace start')
            {
                $global:no_trace = $true
                break
            }

            # 不トレースブロックの終了か
            if ($line2 -match '^[ ]*//C1Tester no trace end')
            {
                $global:no_trace = $false
                break
            }

            break
        }
        2
        {
            $line3b = ''

            # 3行目以降のコメントではない行を探索するループ
            for ($j = 1; $i + $j -lt $source.length; $j++)
            {
                $line = $source[$i + $j]
        
                # コメントではないか
                if ($line -notmatch $c__extension[$extension]['line_comment'])
                {
                    $line3b = $line
                    break
                }
            }

            return (($line3b -ne '') -and ($line3b -match '[,:\]\}]$'))
        }
        Default
        {
            Exiter -1 $work
            break
        }
    }
}

# print_supporting_extensionオプションが有るか
if ($print_supporting_extension)
{
    Write-Host $c__extension.Keys
    exit
}

$extension = Split-Path $args[0] -Extension

# 対応していない拡張子か
if (-not ($c__extension.keys -contains $extension))
{
    Exiter(1)
}

# 指定がカレントパスか
if (-not (Split-Path $args[0] -IsAbsolute))
{
    $backup_folder = Convert-Path $args[0]
}
else
{
    $backup_folder = $args[0]
}

$backup_folder = Split-Path $backup_folder -Parent
$source_filename = Split-Path $args[0] -Leaf
$backup_filename = '_' + $source_filename
$source_path = $backup_folder + '\' + $source_filename
$backup_path = $backup_folder + '\' + $backup_filename

# reportオプションが有るか
if ($report)
{
    $report_file = $c__setting['report_filename_format'] -f $c__setting['product'], $source_filename
    $report_path = $backup_folder + '\' + $report_file

    # 報告ファイルが有るか
    if (Test-Path $report_path)
    {
        Remove-Item $report_path
    }

    '"trace_id","report","timestamp"' | Out-File ($report_path) -Append -Encoding utf8
}

# ファイルが無いか
if (-not (Test-Path $source_path))
{
    Exiter(2)
}

# バックアップファイルが有るか
if (Test-Path $backup_path)
{
    # 中止するオプション指定か
    if ((-not $no_remain_backup) -and (-not $force_write_backup))
    {
        Exiter(3)
    }
    else
    {
        Remove-Item $backup_path
    }
}

Rename-Item $source_path $backup_path
$source = Get-Content $backup_path -Encoding $c__extension[$extension]['char_code_page']
$is_exist_trace = $false

# トレースコード探索ループ
for ($i = 0; $i -lt $source.length; $i++)
{
    if ($source[$i] -match $c__extension[$extension]['regex_search'])
    {
        $is_exist_trace = $true
    }
}

# 既存トレースがあるか
if ($is_exist_trace)
{
    Exiter(4)
}

$trace_id = [int]1
$source[0] | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']
$global:no_trace = $false

# トレース追加ループ
for ($i = 1; $i -lt $source.length - 1; $i++)
{
    $source[$i] | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']
    $line1 = $source[$i - 1]
    $line2 = $source[$i]
    $line3 = $source[$i + 1]

    # 言語依存分岐
    switch ($extension)
    {
        '.ps1'
        {
            for_ps1(1)
            break
        }
        '.cs'
        {
            for_cs(1)
            break
        }
        '.js'
        {
            for_js(1)
            break
        }
        Default
        {
            Exiter -1 $work
            break
        }
    }

    # 不トレースブロックか
    if ($global:no_trace)
    {
        continue
    }

    # 現行が波カッコ開始ではないか
    if ($line2 -notmatch $c__extension[$extension]['regex_trace_position'])
    {
        continue
    }

    # 後行がトレースか
    if ($line3 -match $c__extension[$extension]['regex_search'])
    {
        continue
    }

    $ignore = $false

    # 前行がどれかの除外条件にマッチするかのループ
    foreach ($item in $c__extension[$extension]['regex_ignore'])
    {
        # 除外条件にマッチするか
        if ($line1 -match $item)
        {
            $ignore = $true
            break
        }
    }

    # 1行目が除外条件にマッチしていたか
    if ($ignore)
    {
        continue
    }

    # 言語依存分岐
    switch ($extension)
    {
        '.ps1'
        {
            $rtn = $false
            break
        }
        '.cs'
        {
            $rtn = for_cs(2)
            break
        }
        '.js'
        {
            $rtn = for_js(2)
            break
        }
        Default
        {
            Exiter -1 $work
            break
        }
    }

    # 後処理不要か
    if ($rtn)
    {
        continue
    }
    
    $c__extension[$extension]['add'] -f $trace_id | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']

    # reportオプション有りか
    if ($report)
    {
        $default_report = '"' + [string]$trace_id + '"' + ',"-","-"'
        $default_report | Out-File ($report_path) -Append -Encoding utf8
    }

    $trace_id++
}

$source[$i] | Out-File ($source_path) -Append -Encoding $c__extension[$extension]['char_code_page']

# 言語依存分岐
switch ($extension)
{
    '.ps1'
    {
        for_ps1(0)
        break
    }
    '.cs'
    {
        for_cs(0)
        break
    }
    '.js'
    {
        for_js(0)
        break
    }
    Default
    {
        Exiter -1 $work
        break
    }
}

# no_remain_backupオプションがあるか
if ($no_remain_backup)
{
    Remove-Item $backup_path
}

Exiter(0)
