.\build sample

Add-Type -AssemblyName "System.Core"
function Call-Agent($s,$n,$msg){
    #[void][Reflection.Assembly]::LoadWithPartialName('System') | out-null
    $c=New-Object System.IO.Pipes.NamedPipeClientStream($s,$n,3)
    $c.Connect(100)

    $w=New-Object System.IO.StreamWriter($c)
    $r=New-Object System.IO.StreamReader($c)

    $w.WriteLine($msg)
    $w.Flush()

    do{
        $i=$r.ReadLine();
        Write-Host $i
    }While(![System.String]::IsNullOrEmpty($i))
}

start .\build\Master\Master.exe
start .\build\App\App.exe
Start-Sleep -s 1

Call-Agent 'localhost' 'App' ''