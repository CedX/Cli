# Software Installation

## Microsoft Build of OpenJDK
Download and install the [Microsoft Build of OpenJDK](https://www.microsoft.com/openjdk):

```powershell
# Install OpenJDK to the default location.
Install-Jdk

# Install OpenJDK to a custom location.
Install-Jdk -Path "C:\Program Files\OpenJDK"

# Installs a specific OpenJDK version instead of the latest release.
Install-Jdk -Version 25
```

## Node.js
Download and install the latest [Node.js](https://nodejs.org) release:

```powershell
# Install Node.js to the default location.
Install-Node

# Install Node.js to a custom location.
Install-Node -Path "C:\Program Files\Node.js"
```

## PHP
Download and install the latest [PHP](https://www.php.net) release:

```powershell
# Install PHP to the default location.
Install-Php

# Install PHP to a custom location.
Install-Php -Path "C:\Program Files\PHP"
```

> [!NOTE]
> The `Install-Php` command only supports the Windows platform.
