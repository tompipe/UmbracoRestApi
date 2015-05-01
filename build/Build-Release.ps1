param (
	[Parameter(Mandatory=$true)]
	[ValidatePattern("^\d\.\d\.(?:\d\.\d$|\d$)")]
	[string]
	$ReleaseVersionNumber,
	[Parameter(Mandatory=$true)]
	[string]
	[AllowEmptyString()]
	$PreReleaseName
)

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path);
$RepoRoot = (get-item $PSScriptFilePath).Directory.Parent.FullName;
$SolutionRoot = Join-Path -Path $RepoRoot "src";
$NuGetPackagesPath = Join-Path -Path $SolutionRoot "packages"

#trace
"Solution Root: $SolutionRoot"

$MSBuild = "$Env:SYSTEMROOT\Microsoft.NET\Framework\v4.0.30319\msbuild.exe";

# Go get nuget.exe if we don't hae it
$NuGet = "$BuildFolder\nuget.exe"
$FileExists = Test-Path $NuGet 
If ($FileExists -eq $False) {
	$SourceNugetExe = "http://nuget.org/nuget.exe"
	Invoke-WebRequest $SourceNugetExe -OutFile $NuGet
}

# Restore NuGet packages
# New-Item -ItemType Directory -Force -Path $NuGetPackagesPath
# .\NuGet.exe install $SolutionRoot\UmbracoExamine.PDF\packages.config -OutputDirectory  $NuGetPackagesPath

# Make sure we don't have a release folder for this version already
$BuildFolder = Join-Path -Path $RepoRoot -ChildPath "build";
$ReleaseFolder = Join-Path -Path $BuildFolder -ChildPath "Releases\v$ReleaseVersionNumber$PreReleaseName";
if ((Get-Item $ReleaseFolder -ErrorAction SilentlyContinue) -ne $null)
{
	Write-Warning "$ReleaseFolder already exists on your local machine. It will now be deleted."
	Remove-Item $ReleaseFolder -Recurse
}
New-Item $ReleaseFolder -Type directory

#trace
"Release path: $ReleaseFolder"

# Set the version number in SolutionInfo.cs
$AssemblyInfoPath = Join-Path -Path $SolutionRoot -ChildPath "Umbraco.Web.Rest\Properties\AssemblyInfo.cs"
(gc -Path $AssemblyInfoPath) `
	-replace "(?<=AssemblyFileVersion\(`")[.\d]*(?=`"\))", $ReleaseVersionNumber |
	sc -Path $AssemblyInfoPath -Encoding UTF8;
(gc -Path $AssemblyInfoPath) `
	-replace "(?<=AssemblyInformationalVersion\(`")[.\w-]*(?=`"\))", "$ReleaseVersionNumber$PreReleaseName" |
	sc -Path $AssemblyInfoPath -Encoding UTF8;
# Set the copyright
$Copyright = "Copyright © Umbraco " + (Get-Date).year;
(gc -Path $AssemblyInfoPath) `
	-replace "(?<=AssemblyCopyright\(`").*(?=`"\))", $Copyright |
	sc -Path $AssemblyInfoPath -Encoding UTF8;

# Build the solution in release mode
$SolutionPath = Join-Path -Path $SolutionRoot -ChildPath "Umbraco.Web.Rest.sln";

# clean sln for all deploys
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount /t:Clean
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

#build
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

$include = @('Umbraco.Web.Rest.dll','Umbraco.Web.Rest.pdb')
$BinFolder = Join-Path -Path $SolutionRoot -ChildPath "Umbraco.Web.Rest\bin\Release";
New-Item "$ReleaseFolder\net45\" -Type directory
Copy-Item "$BinFolder\*.*" -Destination "$ReleaseFolder\net45\" -Include $include

# COPY THE README OVER
Copy-Item "$BuildFolder\Readme.txt" -Destination $ReleaseFolder

# COPY OVER THE NUSPEC AND BUILD THE NUGET PACKAGE
Copy-Item "$BuildFolder\Umbraco.Web.Rest.nuspec" -Destination $ReleaseFolder
$NuSpec = Join-Path -Path $ReleaseFolder -ChildPath "Umbraco.Web.Rest.nuspec";

Write-Output "DEBUGGING: " $NuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName
& $NuGet pack $NuSpec -OutputDirectory $ReleaseFolder -Version $ReleaseVersionNumber$PreReleaseName


#TODO: Create an Umbraco package too!!!