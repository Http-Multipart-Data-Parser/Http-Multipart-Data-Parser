# Inspired by: https://github.com/PowerShell/PSScriptAnalyzer/blob/master/tools/appveyor.psm1

$ErrorActionPreference = 'Stop'

# Implements the AppVeyor 'install' step and installs the desired .NET SDK if not already installed.
function Invoke-AppVeyorInstall {

    Write-Verbose -Verbose "Determining the desired version of .NET SDK"
    $globalDotJson = Get-Content (Join-Path $PSScriptRoot 'global.json') -Raw | ConvertFrom-Json
    $desiredDotNetCoreSDKVersion = $globalDotJson.sdk.version
    Write-Verbose -Verbose "We have determined that the desired version of the .NET SDK is $desiredDotNetCoreSDKVersion"

    Write-Verbose -Verbose "Checking availability of .NET SDK $desiredDotNetCoreSDKVersion"
    $desiredDotNetCoreSDKVersionPresent = (dotnet --list-sdks) -match $desiredDotNetCoreSDKVersion

    if (-not $desiredDotNetCoreSDKVersionPresent) {
        Write-Verbose -Verbose "We have determined that the desired version of the .NET SDK is not present on this machine"
        Write-Verbose -Verbose "Installing .NET SDK $desiredDotNetCoreSDKVersion"
        $originalSecurityProtocol = [Net.ServicePointManager]::SecurityProtocol
        try {
            [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
            if ($IsLinux -or $isMacOS) {
                Invoke-WebRequest 'https://dot.net/v1/dotnet-install.sh' -OutFile dotnet-install.sh

                # Normally we would execute dotnet-install.sh like so:
                # bash dotnet-install.sh --version $desiredDotNetCoreSDKVersion
                #
                # and we would also update the PATH environment variable like so:
                # $OLDPATH = [System.Environment]::GetEnvironmentVariable("PATH")
                # $NEWPATH = "/home/appveyor/.dotnet$([System.IO.Path]::PathSeparator)$OLDPATH"
                # [Environment]::SetEnvironmentVariable("PATH", "$NEWPATH")
                #
                # This is supposed to result in the desired .NET SDK to be installed side-by-side
                # with the other version(s) of the SDK already installed. However, my experience
                # on Ubuntu images in Appveyor has been that the recently installed SDK is the only
                # one detected and the previous versions are no longer detected as being installed.
                #
                # This whole thing is problematic because GitVersion.Tool 5.7 is not compatible with
                # .NET 6 (in fact, it doesn't even install) and you must have .NET 5 installed side-by-side
                # with .NET 6 in order to install and use GitVersion.Tool
                #
                # I spent a whole day trying to find a solution but ultimately the only reliable solution
                # I was able to come up with is to install in the default location (which is /usr/share/dotnet)
                # using 'sudo' because you need admin privileges to access the default install location.

                sudo bash dotnet-install.sh --version $desiredDotNetCoreSDKVersion --install-dir /usr/share/dotnet
            }
            else {
                Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile dotnet-install.ps1
                .\dotnet-install.ps1 -Version $desiredDotNetCoreSDKVersion
            }
        }
        finally {
            [Net.ServicePointManager]::SecurityProtocol = $originalSecurityProtocol
            Remove-Item .\dotnet-install.*
        }
        Write-Verbose -Verbose "Installed .NET SDK $desiredDotNetCoreSDKVersion"
    }
    else {
        Write-Verbose -Verbose "We have determined that the desired version of the .NET SDK is already installed on this machine"
    }
}
