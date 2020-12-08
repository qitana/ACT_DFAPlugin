Param()

# 現在のディレクトリを取得する
$startdir = Get-Location
$cd = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $cd

$script:saintCoinach = ".\SaintCoinachData"
$script:jsonDataDir = ".\docs\data"

function GetDungeonData($csvFile) {
    return Get-Content ($csvFile) `
    | ConvertFrom-Csv `
    | Where-Object { [int]::TryParse($_.key, [ref]$null) } `
    | Select-Object key, "37" `
    | Sort-Object { [int]$_.key }
}

function GetRouletteData($csvFile) {
    return Get-Content ($csvFile) `
    | ConvertFrom-Csv `
    | Where-Object { [int]::TryParse($_.key, [ref]$null) } `
    | Select-Object key, "0" `
    | Sort-Object { [int]$_.key }
}


$dungeon_data_en = GetDungeonData(Join-Path $script:saintCoinach "exd-all\ContentFinderCondition.en.csv");
$dungeon_data_fr = GetDungeonData(Join-Path $script:saintCoinach "exd-all\ContentFinderCondition.fr.csv");
$dungeon_data_de = GetDungeonData(Join-Path $script:saintCoinach "exd-all\ContentFinderCondition.de.csv");
$dungeon_data_ja = GetDungeonData(Join-Path $script:saintCoinach "exd-all\ContentFinderCondition.ja.csv");

$dungeon_en = [ordered] @{ }
$dungeon_fr = [ordered] @{ }
$dungeon_de = [ordered] @{ }
$dungeon_ja = [ordered] @{ }

foreach ($x in $dungeon_data_en) { if ($x.37 -ne "") { $dungeon_en.Add($x.key, $x.37) } }
foreach ($x in $dungeon_data_fr) { if ($x.37 -ne "") { $dungeon_fr.Add($x.key, $x.37) } }
foreach ($x in $dungeon_data_de) { if ($x.37 -ne "") { $dungeon_de.Add($x.key, $x.37) } }
foreach ($x in $dungeon_data_ja) { if ($x.37 -ne "") { $dungeon_ja.Add($x.key, $x.37) } }

$dungeon = [ordered] @{ };
$dungeon.Add("English", $dungeon_en);
$dungeon.Add("French", $dungeon_fr);
$dungeon.Add("German", $dungeon_de);
$dungeon.Add("Japanese", $dungeon_ja);

$dungeon | ConvertTo-Json -Depth 100 | Out-File -Encoding UTF8 (Join-Path $script:jsonDataDir "dungeon.json")




$roulette_data_en = GetRouletteData(Join-Path $script:saintCoinach "exd-all\ContentRoulette.en.csv");
$roulette_data_fr = GetRouletteData(Join-Path $script:saintCoinach "exd-all\ContentRoulette.fr.csv");
$roulette_data_de = GetRouletteData(Join-Path $script:saintCoinach "exd-all\ContentRoulette.de.csv");
$roulette_data_ja = GetRouletteData(Join-Path $script:saintCoinach "exd-all\ContentRoulette.ja.csv");

$roulette_en = [ordered] @{ }
$roulette_fr = [ordered] @{ }
$roulette_de = [ordered] @{ }
$roulette_ja = [ordered] @{ }

foreach ($x in $roulette_data_en) { if ($x.0 -ne "") { $roulette_en.Add($x.key, $x.0) } }
foreach ($x in $roulette_data_fr) { if ($x.0 -ne "") { $roulette_fr.Add($x.key, $x.0) } }
foreach ($x in $roulette_data_de) { if ($x.0 -ne "") { $roulette_de.Add($x.key, $x.0) } }
foreach ($x in $roulette_data_ja) { if ($x.0 -ne "") { $roulette_ja.Add($x.key, $x.0) } }

$roulette = [ordered] @{ };
$roulette.Add("English", $roulette_en);
$roulette.Add("French", $roulette_fr);
$roulette.Add("German", $roulette_de);
$roulette.Add("Japanese", $roulette_ja);

$roulette | ConvertTo-Json -Depth 100 | Out-File -Encoding UTF8 (Join-Path $script:jsonDataDir "roulette.json")

Set-Location $startdir
