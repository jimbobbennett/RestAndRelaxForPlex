Param
(
    [Parameter(Mandatory=$True)][string]$version,
    [bool]$push = $false,
    [string]$copyTo
) 

& 'C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe' .\RestAndRelaxForPlex.sln /property:Configuration=Release /Target:Rebuild
& 'nuget.exe' pack .\RestAndRelaxForPlex.nuspec\JimBobBennett.RestAndRelaxForPlex.nuspec -Properties Configuration=Release -Version $version -Symbols

if ($copyTo)
{
    cp .\JimBobBennett.RestAndRelaxForPlex.$version.nupkg $copyTo
    cp .\JimBobBennett.RestAndRelaxForPlex.$version.symbols.nupkg $copyTo
}


if ($push)
{
    nuget.exe push .\JimBobBennett.RestAndRelaxForPlex.$version.nupkg
}