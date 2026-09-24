"Deploying the assets..."
"Binary", "Text" | ForEach-Object {
	$file = "$($_.ToLowerInvariant())-extensions"
	$path = "sindresorhus/$file/main/$file.json"
	Invoke-WebRequest "https://raw.githubusercontent.com/$path" -OutFile "Resources/Text/${_}Extensions.json"
}
