function Get-CompactDirectoryStructure {
    param (
        [Parameter(Mandatory = $true)]
        [string]$path,
        [string]$rootPath
    )

    $relativePath = if ($path -eq $rootPath) { "" } else { $path.Substring($rootPath.Length).TrimStart('\') }
    $structure = @()

    # Get all items in the directory
    $items = Get-ChildItem -Path $path

    foreach ($item in $items) {
        $itemRelativePath = if ($relativePath -eq "") { $item.Name } else { Join-Path -Path $relativePath -ChildPath $item.Name }

        if ($item.PSIsContainer) {
            # Add directory info
            $structure += @{
                "Path" = $itemRelativePath
                "Type" = "Directory"
            }
            # Recurse into subdirectories
            $structure += Get-CompactDirectoryStructure -path $item.FullName -rootPath $rootPath
        }
        else {
            # Add file info
            $structure += @{
                "Path" = $itemRelativePath
                "Type" = "File"
            }
        }
    }

    return $structure
}

# Define the path to the root of the project
$rootPath = "C:\Users\User\Documents\Desktop\MonsterTradingCardsGame\MonsterTradingCardsGame\src"


# Get the compact structure of the directory
$compactStructure = Get-CompactDirectoryStructure -path $rootPath -rootPath $rootPath

# Convert the structure to JSON
$json = $compactStructure | ConvertTo-Json -Compress

# Output the JSON to a file
$json | Out-File -FilePath "C:\Users\User\Documents\Desktop\MonsterTradingCardsGame\project_structure.json"


# Print the JSON to the console
Write-Output "Compact project structure has been saved to compact_project_structure.json"