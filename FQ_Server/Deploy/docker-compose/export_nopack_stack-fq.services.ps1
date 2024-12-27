$outdir_path = "..\..\bin\Deploy\pack"
$outdir_archive_path = "..\..\bin\Deploy"
mkdir $outdir_path 
mkdir $outdir_archive_path 

$images = @();
cat stack-fq.services.yml | ?{$_ -match "image:.*:latest.*$"} | %{$images += ($_ -replace "image: ", "").Trim()};  
echo $images;
ForEach ($image in $images) { $imageName = ($image -replace ":latest", "").Trim(); echo $imageName; docker save -o $outdir_path\$imageName.img $image; }

#$7zipPath = "${env:ProgramFiles(x86)}\7-Zip\7z.exe"
#if (-not (Test-Path -Path $7zipPath -PathType Leaf)) {
#    throw "7 zip file '$7zipPath' not found"
#}
#
#Set-Alias 7zip $7zipPath
#7zip u -tgzip -mx9 -aoa -bsp1 stack-fq.services_nopack.tgz $outdir_path

pause
