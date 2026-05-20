"Deploying the assets..."
foreach ($type in "Binary", "Text") {
	$file = "$($type.ToLowerInvariant())-extensions"
	$path = "sindresorhus/$file/main/$file.json"
	Invoke-WebRequest "https://raw.githubusercontent.com/$path" -OutFile "Resources/Text/${type}Extensions.json"
}
