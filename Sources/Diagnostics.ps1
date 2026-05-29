using namespace System.IO
using module ./Architecture.psm1

<#
.SYNOPSIS
	Gets the architecture of the specified Windows executable.
.INPUTS
	The path of the executable to inspect.
.OUTPUTS
	The architecture of the specified Windows executable.
#>
function Get-ExecutableArchitecture {
	[CmdletBinding()]
	[OutputType([Architecture])]
	param (
		# The path of the executable to inspect.
		[Parameter(Mandatory, Position = 0, ValueFromPipeline)]
		[ValidateScript({ Test-Path $_ -PathType Leaf }, ErrorMessage = "The specified file does not exist.")]
		[string] $Path
	)

	begin {
		if (-not $IsWindows) { throw [PlatformNotSupportedException] "This command only supports the Windows platform." }
	}

	process {
		$stream = [File]::OpenRead($Path)
		$reader = [BinaryReader] $stream

		try {
			$stream.Seek(0x3C, [SeekOrigin]::Begin) | Out-Null
			$stream.Seek($reader.ReadInt32() + 4, [SeekOrigin]::Begin) | Out-Null
			switch ($reader.ReadUInt16()) {
				0x014C { return [Architecture]::x86 }
				0x8664 { return [Architecture]::x64 }
				default { throw [NotSupportedException] "Unsupported machine type: 0x{0:X4}" -f $_ }
			}
		}
		finally {
			$reader.Close()
			$stream.Close()
		}
	}
}
