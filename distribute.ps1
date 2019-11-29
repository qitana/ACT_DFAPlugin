# ビルド
./build.bat

# バージョン取得
$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\DFAPlugin\bin\Release\DFAPlugin.dll").FileVersion

# フォルダ名
$buildFolder = ".\DFAPlugin\bin\Release"
$fullFolder = ".\Distribute\DFAPlugin-" + $version

# フォルダが既に存在するなら消去
if ( Test-Path $fullFolder -PathType Container ) {
	Remove-Item -Recurse -Force $fullFolder
}

# フォルダ作成
New-Item -ItemType directory -Path $fullFolder

# full
xcopy /Y /R /S /EXCLUDE:full.exclude "$buildFolder\*" "$fullFolder"

cd Distribute
$folder = "DFAPlugin-" + $version

# アーカイブ
& "C:\Program Files\7-Zip\7z.exe" "a" "$folder.7z" "$folder"

pause
