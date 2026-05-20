using module Belin.Cli

# Install OpenJDK to the default location.
Install-Jdk

# Install OpenJDK to a custom location.
Install-Jdk -Path "C:\Program Files\OpenJDK"

# Installs a specific OpenJDK version instead of the latest release.
Install-Jdk -Version 25

# Install Node.js to the default location.
Install-Node

# Install Node.js to a custom location.
Install-Node -Path "C:\Program Files\Node.js"

# Install PHP to the default location.
Install-Php

# Install PHP to a custom location.
Install-Php -Path "C:\Program Files\PHP"
