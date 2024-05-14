param($htmlAgilityPackSourceLocation, $outFile)

$excludedFiles = "crc32.cs","HtmlCmdLine.cs", "HtmlConsoleListener.cs","HtmlNode.Encapsulator.cs","HtmlWeb*.cs","InvalidProgramException.cs","IOLibrary.cs","MimeTypeMap.cs","MixedCodeDocument*.cs","NameValuePair.cs","Trace.FullFramework.cs", "Utilities.cs"
$files = Get-ChildItem "$htmlAgilityPackSourceLocation\HtmlAgilityPack.Shared\*.cs" -Exclude $excludedFiles

$firstFile = $true;
$fullContent = @();
foreach ($file in $files) {
    $file
    $started = $false
    $content = Get-Content $file
    foreach($line in $content) {
        if (-not $started) {
            #copy preable from first file
            if ($firstFile -and $line.StartsWith("//")) {
                $fullContent += $line
            }
            #skip usings and namespace declaration
            #first { marks start of namespace
            if ($line -eq "{") {
                $started = $true
                $firstFile = $false;
            }
            continue
        }
        #last } marks namespace scope end
        elseif ($line -eq "}") {
            break
        }
        
        if ($line.TrimStart().StartsWith("//") -or #skip lines with comments and documentation
            $line.Contains("#region") -or $line.Contains("#endregion") -or #skip region block start/end line
            $line.Trim() -eq "") #skip empty lines
        {
            continue
        }
        #make the file smaller by
        #- removing first indentation
        if ($line.IndexOf("    ") -eq 0) {
            $line = $line.Substring(4)
        }
        elseif($line.IndexOf("`t") -eq 0) {
            $line = $line.Substring(1)
        }
        #- using tabs in stead of spaces
        $line= $line.Replace("    ", "`t")

        $fullContent += $line
    }
}
Set-Content $outFile $fullContent