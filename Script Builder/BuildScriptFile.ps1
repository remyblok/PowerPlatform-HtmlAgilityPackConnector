param( 
	[Parameter(Position=0)][String]$outFile,
	[Parameter(ValueFromRemainingArguments=$true)][String[]]$files)

Set-Content $outfile "" -NoNewLine
foreach($file in $files)
{
	$file
	Add-Content $outFile (Get-Content $file)
}