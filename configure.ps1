Param()

$startdir = Get-Location
$cd = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $cd

Start-Transcript configure.log | Out-Null

function EndConfigure() {
    Stop-Transcript | Out-Null
    ''
    Read-Host "終了するには何かキーを押してください..."
    exit
}


$7zPath = ".\tools\7za.exe"
$msSdkDir = "C:\Program Files (x86)\Microsoft SDKs"
$actURL = "https://advancedcombattracker.com/includes/page-download.php?id=57"

Write-Output ("*** Configure start.")
Write-Output ""
Write-Output ""

Write-Output ("環境を確認しています...")
Write-Output ""


# Test 7zip
if (!(Test-Path $7zPath)) {
    Write-Output ("エラー: " + $7zPath + " が見つかりませんでした。")
    EndConfigure
}
$7z = Get-Item -Path $7zPath
if ($7z.Length -lt 10000) {
    Write-Output ("エラー: " + $7zPath + " が壊れている可能性があります。")
    Write-Output ("GitHubからソースコードをzipファイルでダウンロードした場合、正しくビルドできません。")
    Write-Output ("git コマンドを使用してレポジトリ全体をダウンロードして下さい。")
    EndConfigure
}
Write-Output("7zip: " + $7z.FullName)


# Test ildasm
if (!(Test-Path $msSdkDir)) {
    Write-Output ("エラー: " + $msSdkDir + " が見つかりませんでした。")
    Write-Output ("Microsoft Windows 10 SDK をインストールして下さい。")
    EndConfigure
}
$ildasm_exes = Get-ChildItem "C:\Program Files (x86)\Microsoft SDKs" -Recurse -Filter "ildasm.exe" | Sort-Object -Property LastWriteTime -Descending
if($ildasm_exes.Length -eq 0) {
    Write-Output ("エラー: ildasm.exe が見つかりませんでした。")
    Write-Output ("Microsoft Windows 10 SDK をインストールして下さい。")
    EndConfigure
}
$ildasm = Get-Item $ildasm_exes[0].FullName
Write-Output("ildasm: " + $ildasm.FullName)


Write-Output ("必要なファイルを集めています...")
Write-Output ""

$TempFolder = New-TemporaryFile | ForEach-Object { Remove-Item -Path $_; New-Item -Path $_ -ItemType Directory }

# Advanced Combat Tracker
if (!(Test-Path ".\ThirdParty\ACT\Advanced Combat Tracker.exe")) {
    Write-Output ""
    Write-Output "-----------------------------------------------------------------------"
    Write-Output "Advanced Combat Tracker をダウンロードしています..."
    Write-Output ($actURL)
    $actFile = Join-Path $TempFolder "ACT.zip"
    Invoke-WebRequest -Uri $actURL -OutFile $actFile
    & $7z e -y -o".\ThirdParty\ACT\" -ir!"Advanced Combat Tracker.exe" $actFile
    Write-Output "完了."
}

# OverlayPlugin
if ((!(Test-Path ".\ThirdParty\OverlayPlugin\OverlayPlugin.Common.dll")) -or
    (!(Test-Path ".\ThirdParty\OverlayPlugin\OverlayPlugin.Core.dll"))) {
        Write-Output ""
        Write-Output "-----------------------------------------------------------------------"
        Write-Output "OverlayPlugin をダウンロードしています..."
    
    $overlayPluginRepo = "ngld/OverlayPlugin"
    $overlayPluginLatest = "https://api.github.com/repos/$overlayPluginRepo/releases"
    $overlayPlugin_download_url = (Invoke-WebRequest -Uri $overlayPluginLatest -UseBasicParsing | ConvertFrom-Json)[0].assets[0].browser_download_url
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    if ($overlayPlugin_download_url.Length -gt 1) {
        $overlayPluginArchiveFileName = [System.IO.Path]::GetFileName($overlayPlugin_download_url)
        $overlayPluginArchiveFile = Join-Path $TempFolder $overlayPluginArchiveFileName
        Invoke-WebRequest -Uri $overlayPlugin_download_url -OutFile $overlayPluginArchiveFile
        & $7z e -y -o".\Thirdparty\OverlayPlugin\" -ir!"OverlayPlugin.Common.dll" $overlayPluginArchiveFile
        & $7z e -y -o".\Thirdparty\OverlayPlugin\" -ir!"OverlayPlugin.Core.dll" $overlayPluginArchiveFile
    }
    else {
        Write-Output ("エラー: 最新リリースの取得に失敗しました")
        EndConfigure
    }
    Write-Output "完了."
}

# FFXIV_ACT_Plugin SDK
if ((!(Test-Path ".\ThirdParty\FFXIV_ACT_Plugin\FFXIV_ACT_Plugin.dll")) -or
(!(Test-Path ".\ThirdParty\FFXIV_ACT_Plugin\FFXIV_ACT_Plugin.Common.dll"))) {
    Write-Output ""
    Write-Output "-----------------------------------------------------------------------"
    Write-Output "FFXIV_ACT_Plugin SDK をダウンロードしています..."

    $ffxivActPluginRepo = "ravahn/FFXIV_ACT_Plugin"
    $ffxivActPluginDllFileName = "FFXIV_ACT_Plugin.dll"
    $ffxivActPluginLiFileName = "FFXIV_ACT_Plugin.li"
    $ffxivActPluginLatest = "https://api.github.com/repos/$ffxivActPluginRepo/releases/latest"
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $ffxivActPlugin_download_url = (Invoke-WebRequest -Uri $ffxivActPluginLatest -UseBasicParsing | ConvertFrom-Json)[0].assets[0].browser_download_url
    if ($ffxivActPlugin_download_url.Length -gt 1) {
        $ffxivActPluginArchiveFileName = [System.IO.Path]::GetFileName($ffxivActPlugin_download_url)
        $ffxivActPluginArchiveFile = Join-Path $TempFolder $ffxivActPluginArchiveFileName
        Invoke-WebRequest -Uri $ffxivActPlugin_download_url -OutFile $ffxivActPluginArchiveFile
        $ffxivActPluginExtractDir = Join-Path $TempFolder "FFXIVActPluginExtract"
        & $7z e -y -o"$ffxivActPluginExtractDir" -ir!"$ffxivActPluginDllFileName" $ffxivActPluginArchiveFile
        $ffxivActPluginDllFile = Join-Path $ffxivActPluginExtractDir $ffxivActPluginDllFileName
        $ffxivActPluginLiFile = Join-Path $ffxivActPluginExtractDir $ffxivActPluginLiFileName
        if(!(Test-Path $ffxivActPluginDllFile)) {
            Write-Output ("エラー: 最新リリースの取得に失敗しました")
            EndConfigure
        }
        & $ildasm $ffxivActPluginDllFile /out="$ffxivActPluginLiFile"

        $fileCosturaDecompress = "./tools/CosturaDecompress.cs"
        $srcCosturaDecompress = Get-Content $fileCosturaDecompress | Out-String
        Add-Type -TypeDefinition $srcCosturaDecompress -Language CSharp

        Copy-Item $ffxivActPluginDllFile ".\Thirdparty\FFXIV_ACT_Plugin\"

        [ACT.Hojoring.ATDExtractor.CosturaDecompress]::DecompressFile((Join-Path $ffxivActPluginExtractDir "costura.ffxiv_act_plugin.common.dll.compressed"), (Join-Path $cd ".\Thirdparty\FFXIV_ACT_Plugin\FFXIV_ACT_Plugin.Common.dll"))
    }
    else {
        Write-Output ("エラー: 最新リリースの取得に失敗しました")
        EndConfigure
    }
    Write-Output "完了."
}

Remove-Item -Path $TempFolder -Recurse -Force;

Write-Output ""
Write-Output ("*** Configure complete.")
Write-Output ""

Set-Location $startdir
EndConfigure
