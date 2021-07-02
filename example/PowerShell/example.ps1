function f1($number)
{
    for ($i = 0; $i -le $number; $i++)
    {
        if ($i % 2 -eq 0)
        {
            Write-Host ([string]$i + " is even.")
        }
        else
        {
            Write-Host ([string]$i + " is odd.")
        }
        
        switch ($i)
        {
            1
            {
                Write-Host ([string]$i + " is one.")
                break
            }
            2
            {
                Write-Host ([string]$i + " is two.")
                break
            }
            Default
            {
                break
            }
        }
    }
}

if (([int]$args[0] -lt 0) -or ([int]$args[0] -gt 5))
{
    Write-Host ("argument must be 0-5.")
    exit(1)
}

f1([int]$args[0])
