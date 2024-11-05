import json
import re
import os

# Sketchy regex to find version
pattern = re.compile(r'\[assembly: AssemblyVersion\("(.*)"\)\]')

with open('AssemblyInfo.cs', 'r') as assembly_info_file:
	for line in assembly_info_file.readlines():
		m = pattern.match(line)
		if m:
			version = m.group(1)

if not version:
	print("Unable to generate manifest.json. Version not found in AssemblyInfo.cs")
	exit(1)

print("Generating manifest for version", version)

# Ensure version has at most three parts
s = version.split('.')
if len(s) > 3:
	version = '.'.join(s[:3])

manifest_json = {
	"Name": "Clear Glasses",
	"Author": "aurpine",
	"Version": version,
	"Description": "Steps up the visuals of your game by using pixel scaling techniques and filters.",
	"UniqueID": "aurpine.ClearGlasses",
	"EntryDll": "ClearGlasses.dll",
	"MinimumApiVersion": "4.0.0",
	"UpdateKeys": ["Nexus:21090", "GitHub:aurpine/Stardew-SpriteMaster"],
	"PrivateAssemblies": [
		{
			"Name": "CommunityToolkit.HighPerformance",
		},
		{
			"Name": "FastExpressionCompiler.LightExpression",
		},
		{
			"Name": "LinqFasterer",
		},
		{
			"Name": "Pastel",
		},
		{
			"Name": "Tomlyn",
		},
		{
			"Name": "ZstdNet",
		}
	],
}

with open('manifest.json', 'w') as manifest_file:
	json.dump(manifest_json, manifest_file)

	print("Successfully created", os.path.realpath(manifest_file.name))
