$outdir_path = "..\..\bin\Deploy"
mkdir $outdir_path 
$images = @();
cat stack-fq.services.yml | ?{$_ -match "image:.*:latest.*$"} | %{$images += ($_ -replace "image: ", "").Trim()}; 
echo $images;
docker save -o $outdir_path\stack-fq.services.img $images;

$7zipPath = "${env:ProgramFiles(x86)}\7-Zip\7z.exe"
if (-not (Test-Path -Path $7zipPath -PathType Leaf)) {
    throw "7 zip file '$7zipPath' not found"
}

Set-Alias 7zip $7zipPath

7zip u -tgzip -mx9 -bsp1 $outdir_path\stack-fq.services.tgz $outdir_path\stack-fq.services.img

pause