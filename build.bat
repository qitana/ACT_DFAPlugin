@echo off

if not exist "%~dp0\Thirdparty\ACT\Advanced Combat Tracker.exe" (
	echo エラー: "Thirdparty" ディレクトリに "Advanced Combat Tracker.exe" をコピーしてください。
	goto END
)

if not exist "%~dp0\Thirdparty\OverlayPlugin\OverlayPlugin.dll" (
	echo エラー: "Thirdparty\OverlayPlugin" ディレクトリに "OverlayPlugin.dll" をコピーしてください。
	goto END
)

if not exist "%~dp0\Thirdparty\OverlayPlugin\OverlayPlugin.Core.dll" (
	echo エラー: "Thirdparty\OverlayPlugin" ディレクトリに "OverlayPlugin.Core.dll" をコピーしてください。
	goto END
)

if not exist "%~dp0\Thirdparty\OverlayPlugin\OverlayPlugin.Common.dll" (
	echo エラー: "Thirdparty\OverlayPlugin" ディレクトリに "OverlayPlugin.Common.dll" をコピーしてください。
	goto END
)

if not exist "%~dp0\Thirdparty\OverlayPlugin\HtmlRenderer.dll" (
	echo エラー: "Thirdparty\OverlayPlugin" ディレクトリに "HtmlRenderer.dll" をコピーしてください。
	goto END
)

if not exist "%~dp0\Thirdparty\OverlayPlugin\Xilium.CefGlue.dll" (
	echo エラー: "Thirdparty\OverlayPlugin" ディレクトリに "Xilium.CefGlue.dll" をコピーしてください。
	goto END
)

if not exist "%~dp0\Thirdparty\FFXIV_ACT_Plugin\FFXIV_ACT_Plugin.dll" (
	echo エラー: "Thirdparty\OverlayPlugin" ディレクトリに "FFXIV_ACT_Plugin.dll" をコピーしてください。
	goto END
)

if not exist "%~dp0\Thirdparty\FFXIV_ACT_Plugin\FFXIV_ACT_Plugin.Memory.dll" (
	echo エラー: "Thirdparty\OverlayPlugin" ディレクトリに "FFXIV_ACT_Plugin.Memory.dll" をコピーしてください。
	goto END
)

if not exist "%~dp0\Thirdparty\FFXIV_ACT_Plugin\FFXIV_ACT_Plugin.Network.dll" (
	echo エラー: "Thirdparty\OverlayPlugin" ディレクトリに "FFXIV_ACT_Plugin.Network.dll" をコピーしてください。
	goto END
)

if not exist "%~dp0\Thirdparty\FFXIV_ACT_Plugin\Machina.dll" (
	echo エラー: "Thirdparty\OverlayPlugin" ディレクトリに "Machina.dll" をコピーしてください。
	goto END
)

if not exist "%~dp0\Thirdparty\FFXIV_ACT_Plugin\Machina.FFXIV.dll" (
	echo エラー: "Thirdparty\OverlayPlugin" ディレクトリに "Machina.FFXIV.dll" をコピーしてください。
	goto END
)

set DOTNET_PATH=%windir%\Microsoft.NET\Framework\v4.0.30319
if not exist %DOTNET_PATH% (
	echo エラー: .NET Framework のディレクトリが見つかりません。ビルドを実行するためには .NET Framework 4.6以上がインストールされている必要があります。
	goto END
)

set MSBUILD_PATH="%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current"
if not exist %MSBUILD_PATH% (
	echo エラー: Microsoft Build Tool のディレクトリが見つかりません。ビルドを実行するためには Microsoft Build Tool がインストールされている必要があります。
	goto END
)

if exist "%~dp0\Build" (
	rd /s /q "%~dp0\Build
)

%MSBUILD_PATH%\Bin\msbuild /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath="%~dp0\Build" "%~dp0\DFAPlugin.sln"

:END
