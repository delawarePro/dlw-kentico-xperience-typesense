# PowerShell script to manage version updates for Kentico.Xperience.Typesense
param(
    [switch]$AutoBeta,
    [switch]$AutoRelease,
    [string]$CustomVersion,
    [switch]$AutoGit,
    [switch]$SkipConfirmation,
    [switch]$LocalNuget,
    [switch]$Help
)

# Show help information
if ($Help) {
    Write-Host "🚀 Kentico.Xperience.Typesense Version Manager - Aide" -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📋 UTILISATION :" -ForegroundColor White
    Write-Host "   .\PublishAndRelease.ps1 [OPTIONS]" -ForegroundColor Gray
    Write-Host ""
    Write-Host "📌 OPTIONS DISPONIBLES :" -ForegroundColor White
    Write-Host ""
    Write-Host "   -AutoBeta" -ForegroundColor Yellow -NoNewline
    Write-Host "          Créer automatiquement la prochaine version beta" -ForegroundColor Gray
    Write-Host "   -AutoRelease" -ForegroundColor Yellow -NoNewline  
    Write-Host "       Créer automatiquement la prochaine version release" -ForegroundColor Gray
    Write-Host "   -CustomVersion" -ForegroundColor Yellow -NoNewline
    Write-Host "     Spécifier une version personnalisée (ex: '2.0.0-beta-1')" -ForegroundColor Gray
    Write-Host "   -AutoGit" -ForegroundColor Yellow -NoNewline
    Write-Host "          Exécuter automatiquement les commandes Git" -ForegroundColor Gray
    Write-Host "   -SkipConfirmation" -ForegroundColor Yellow -NoNewline
    Write-Host "   Ignorer les demandes de confirmation" -ForegroundColor Gray
    Write-Host "   -LocalNuget" -ForegroundColor Yellow -NoNewline
    Write-Host "        Créer le package NuGet localement dans d:\LocalNugets\" -ForegroundColor Gray
    Write-Host "   -Help" -ForegroundColor Yellow -NoNewline
    Write-Host "             Afficher cette aide" -ForegroundColor Gray
    Write-Host ""
    Write-Host "💡 EXEMPLES D'UTILISATION :" -ForegroundColor White
    Write-Host ""
    Write-Host "   Mode interactif (par défaut) :" -ForegroundColor Cyan
    Write-Host "   .\PublishAndRelease.ps1" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   Nouvelle version beta automatique :" -ForegroundColor Cyan
    Write-Host "   .\PublishAndRelease.ps1 -AutoBeta" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   Version release avec Git automatique :" -ForegroundColor Cyan
    Write-Host "   .\PublishAndRelease.ps1 -AutoRelease -AutoGit" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   Version personnalisée complètement automatique :" -ForegroundColor Cyan
    Write-Host "   .\PublishAndRelease.ps1 -CustomVersion '2.0.0' -AutoGit -SkipConfirmation" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   Version beta avec NuGet local :" -ForegroundColor Cyan
    Write-Host "   .\PublishAndRelease.ps1 -AutoBeta -LocalNuget" -ForegroundColor Gray
    Write-Host ""
    Write-Host "📦 WORKFLOW COMPLET :" -ForegroundColor White
    Write-Host "   1. Mise à jour de Directory.Build.props" -ForegroundColor Gray
    Write-Host "   2. git add . puis git commit -m 'vX.X.X'" -ForegroundColor Gray
    Write-Host "   3. git push origin" -ForegroundColor Gray
    Write-Host "   4. git tag vX.X.X" -ForegroundColor Gray
    Write-Host "   5. git push origin --tags" -ForegroundColor Gray
    Write-Host "   6. dotnet pack vers d:\LocalNugets\ (si -LocalNuget)" -ForegroundColor Gray
    Write-Host ""
    exit 0
}

# Function to read current version from Directory.Build.props
function Get-CurrentVersion {
    $propsFile = Join-Path $PSScriptRoot "Directory.Build.props"
    
    if (-not (Test-Path $propsFile)) {
        throw "Directory.Build.props file not found at: $propsFile"
    }
    
    [xml]$xml = Get-Content $propsFile
    
    # Find the PropertyGroup that contains VersionPrefix
    foreach ($propertyGroup in $xml.Project.PropertyGroup) {
        if ($propertyGroup.VersionPrefix) {
            return $propertyGroup.VersionPrefix.ToString().Trim()
        }
    }
    
    throw "VersionPrefix not found in Directory.Build.props"
}

# Function to parse version components
function Parse-Version {
    param([string]$version)
    
    # Match pattern like "1.0.25-beta-2" or "1.0.25"
    if ($version -match '^(\d+)\.(\d+)\.(\d+)(?:-([a-zA-Z]+)-(\d+))?$') {
        return @{
            Major = [int]$Matches[1]
            Minor = [int]$Matches[2]
            Patch = [int]$Matches[3]
            PreReleaseType = $Matches[4]
            PreReleaseNumber = if ($Matches[5]) { [int]$Matches[5] } else { $null }
            IsPreRelease = $null -ne $Matches[4]
        }
    } elseif ($version -match '^(\d+)\.(\d+)\.(\d+)-([a-zA-Z]+)(\d+)$') {
        # Alternative pattern for "1.0.25-beta2" (without dash before number)
        return @{
            Major = [int]$Matches[1]
            Minor = [int]$Matches[2]
            Patch = [int]$Matches[3]
            PreReleaseType = $Matches[4]
            PreReleaseNumber = [int]$Matches[5]
            IsPreRelease = $true
        }
    } else {
        throw "Invalid version format: $version. Expected formats: 1.0.25, 1.0.25-beta-2, or 1.0.25-beta2"
    }
}

# Function to update version in Directory.Build.props
function Update-Version {
    param([string]$newVersion)
    
    $propsFile = Join-Path $PSScriptRoot "Directory.Build.props"
    [xml]$xml = Get-Content $propsFile
    
    # Find the PropertyGroup that contains VersionPrefix and update it
    foreach ($propertyGroup in $xml.Project.PropertyGroup) {
        if ($propertyGroup.VersionPrefix) {
            $propertyGroup.VersionPrefix = $newVersion
            break
        }
    }
    
    $xml.Save($propsFile)
    
    Write-Host "✅ Version updated to: $newVersion" -ForegroundColor Green
}

# Function to generate next beta version
function Get-NextBetaVersion {
    param($versionInfo)
    
    if ($versionInfo.IsPreRelease -and $versionInfo.PreReleaseType -eq "beta") {
        # Increment beta number
        $newBetaNumber = $versionInfo.PreReleaseNumber + 1
        return "$($versionInfo.Major).$($versionInfo.Minor).$($versionInfo.Patch)-beta-$newBetaNumber"
    } else {
        # Create first beta of current version
        return "$($versionInfo.Major).$($versionInfo.Minor).$($versionInfo.Patch)-beta-1"
    }
}

# Function to generate next release version
function Get-NextReleaseVersion {
    param($versionInfo)
    
    if ($versionInfo.IsPreRelease) {
        # Remove pre-release suffix
        return "$($versionInfo.Major).$($versionInfo.Minor).$($versionInfo.Patch)"
    } else {
        # Increment patch version
        $newPatch = $versionInfo.Patch + 1
        return "$($versionInfo.Major).$($versionInfo.Minor).$newPatch"
    }
}

# Function to generate next major/minor version
function Get-NextMajorMinorVersion {
    param($versionInfo, [string]$type)
    
    switch ($type) {
        "major" {
            $newMajor = $versionInfo.Major + 1
            return "$newMajor.0.0-beta-1"
        }
        "minor" {
            $newMinor = $versionInfo.Minor + 1
            return "$($versionInfo.Major).$newMinor.0-beta-1"
        }
        default {
            throw "Invalid version type: $type"
        }
    }
}

# Function to execute Git commands
function Invoke-GitCommands {
    param([string]$version)
    
    Write-Host "`n🚀 Exécution des commandes Git..." -ForegroundColor Yellow
    
    try {
        # Check if we're in a git repository
        git status 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Ce dossier n'est pas un dépôt Git valide"
        }
        
        # Step 1: Add and commit changes
        Write-Host "`n📝 1. Commit des changements..." -ForegroundColor Cyan
        git add .
        git commit -m "v$version"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Commit réussi" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Aucun changement à commiter ou erreur de commit" -ForegroundColor Yellow
        }
        
        # Step 2: Push changes
        Write-Host "`n🌐 2. Push des changements vers origin..." -ForegroundColor Cyan
        git push origin
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Push des changements réussi" -ForegroundColor Green
        } else {
            Write-Host "❌ Erreur lors du push des changements" -ForegroundColor Red
        }
        
        # Step 3: Create and push tag
        Write-Host "`n🏷️  3. Création du tag v$version..." -ForegroundColor Cyan
        git tag "v$version"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Tag créé avec succès" -ForegroundColor Green
            
            Write-Host "`n🌐 4. Push du tag vers origin..." -ForegroundColor Cyan
            git push origin --tags
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Tag poussé avec succès" -ForegroundColor Green
            } else {
                Write-Host "❌ Erreur lors du push du tag" -ForegroundColor Red
            }
        } else {
            Write-Host "❌ Erreur lors de la création du tag (il existe peut-être déjà)" -ForegroundColor Red
        }
        
        Write-Host "`n🎉 PROCESSUS GIT TERMINÉ !" -ForegroundColor Green -BackgroundColor DarkGreen
        
    } catch {
        Write-Host "`n❌ Erreur Git : $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Vous devrez exécuter les commandes manuellement." -ForegroundColor Gray
    }
}

# Function to create local NuGet packages
function Invoke-LocalNugetPack {
    param([string]$version)
    
    Write-Host "`n📦 Création des packages NuGet locaux..." -ForegroundColor Yellow
    
    $localNugetPath = "d:\LocalNugets"
    
    try {
        # Create directory if it doesn't exist
        if (-not (Test-Path $localNugetPath)) {
            Write-Host "📁 Création du dossier $localNugetPath..." -ForegroundColor Cyan
            New-Item -ItemType Directory -Path $localNugetPath -Force | Out-Null
            Write-Host "✅ Dossier créé avec succès" -ForegroundColor Green
        }
        
        # Find all .csproj files in the solution
        $projectFiles = Get-ChildItem -Path $PSScriptRoot -Recurse -Name "*.csproj" | Where-Object {
            $_ -notlike "*Test*" -and $_ -notlike "*Example*" -and $_ -notlike "*DancingGoat*"
        }
        
        if ($projectFiles.Count -eq 0) {
            Write-Host "⚠️  Aucun fichier .csproj trouvé pour la création des packages" -ForegroundColor Yellow
            return
        }
        
        Write-Host "📋 Projets trouvés pour la création des packages :" -ForegroundColor Cyan
        $projectFiles | ForEach-Object {
            Write-Host "   • $_" -ForegroundColor Gray
        }
        
        # Pack each project
        $successCount = 0
        $failureCount = 0
        
        foreach ($projectFile in $projectFiles) {
            $projectPath = Join-Path $PSScriptRoot $projectFile
            $projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectFile)
            
            Write-Host "`n🔨 Création du package pour $projectName..." -ForegroundColor Cyan
            
            # Run dotnet pack
            $packResult = dotnet pack $projectPath --configuration Release --output $localNugetPath --no-restore
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Package créé avec succès pour $projectName" -ForegroundColor Green
                $successCount++
                
                # Find the created package
                $packagePattern = "$projectName.$version*.nupkg"
                $createdPackages = Get-ChildItem -Path $localNugetPath -Name $packagePattern
                
                if ($createdPackages) {
                    $createdPackages | ForEach-Object {
                        Write-Host "   📦 Package créé : $_" -ForegroundColor Gray
                    }
                }
            } else {
                Write-Host "❌ Erreur lors de la création du package pour $projectName" -ForegroundColor Red
                $failureCount++
                Write-Host "   Détails de l'erreur :" -ForegroundColor Red
                Write-Host "   $packResult" -ForegroundColor Red
            }
        }
        
        Write-Host "`n📊 RÉSUMÉ DE LA CRÉATION DES PACKAGES :" -ForegroundColor White -BackgroundColor DarkBlue
        Write-Host "   ✅ Succès : $successCount" -ForegroundColor Green
        Write-Host "   ❌ Échecs : $failureCount" -ForegroundColor Red
        Write-Host "   📁 Dossier : $localNugetPath" -ForegroundColor Gray
        
        if ($successCount -gt 0) {
            Write-Host "`n🎉 PACKAGES NUGET CRÉÉS AVEC SUCCÈS !" -ForegroundColor Green -BackgroundColor DarkGreen
            Write-Host "   Vous pouvez maintenant utiliser ces packages depuis $localNugetPath" -ForegroundColor Gray
        }
        
    } catch {
        Write-Host "`n❌ Erreur lors de la création des packages NuGet : $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Vous devrez créer les packages manuellement." -ForegroundColor Gray
    }
}

# Main script logic
try {
    Write-Host "🚀 Kentico.Xperience.Typesense Version Manager" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    
    # Get current version
    $currentVersion = Get-CurrentVersion
    $versionInfo = Parse-Version $currentVersion
    
    Write-Host "📋 Current version: $currentVersion" -ForegroundColor Yellow
    
    # Handle custom version
    if ($CustomVersion) {
        $parsedCustom = Parse-Version $CustomVersion
        Update-Version $CustomVersion
        
        if ($LocalNuget) {
            Invoke-LocalNugetPack $CustomVersion
        }
        
        exit 0
    }
    
    # Handle auto modes
    if ($AutoBeta) {
        $newVersion = Get-NextBetaVersion $versionInfo
        Write-Host "🤖 Mode automatique : Nouvelle version BETA ($newVersion)" -ForegroundColor Green
        
        if (-not $SkipConfirmation) {
            $confirm = Read-Host "Confirmer la mise à jour vers $newVersion (O/n)"
            if (-not ($confirm -eq "o" -or $confirm -eq "O" -or $confirm -eq "oui" -or $confirm -eq "OUI")) {
                Write-Host "❌ Opération annulée" -ForegroundColor Yellow
                exit 0
            }
        }
        
        Update-Version $newVersion
        
        if ($AutoGit) {
            Invoke-GitCommands $newVersion
        }
        
        if ($LocalNuget) {
            Invoke-LocalNugetPack $newVersion
        }
        
        exit 0
    }
    
    if ($AutoRelease) {
        $newVersion = Get-NextReleaseVersion $versionInfo
        Write-Host "🤖 Mode automatique : Version de RELEASE ($newVersion)" -ForegroundColor Green
        
        if (-not $SkipConfirmation) {
            $confirm = Read-Host "Confirmer la mise à jour vers $newVersion (O/n)"
            if (-not ($confirm -eq "o" -or $confirm -eq "O" -or $confirm -eq "oui" -or $confirm -eq "OUI")) {
                Write-Host "❌ Opération annulée" -ForegroundColor Yellow
                exit 0
            }
        }
        
        Update-Version $newVersion
        
        if ($AutoGit) {
            Invoke-GitCommands $newVersion
        }
        
        if ($LocalNuget) {
            Invoke-LocalNugetPack $newVersion
        }
        
        exit 0
    }
    
    # Interactive mode
    Write-Host "`n" -NoNewline
    Write-Host "🎯 CHOISISSEZ LE TYPE DE MISE À JOUR DE VERSION" -ForegroundColor White -BackgroundColor DarkBlue
    Write-Host "`n"
    
    Write-Host "Voici les options disponibles pour mettre à jour la version :" -ForegroundColor Cyan
    Write-Host ""
    
    # Calculate next versions for display
    $nextBeta = Get-NextBetaVersion $versionInfo
    $nextRelease = Get-NextReleaseVersion $versionInfo
    $nextMinor = Get-NextMajorMinorVersion $versionInfo 'minor'
    $nextMajor = Get-NextMajorMinorVersion $versionInfo 'major'
    
    Write-Host "  [1] 🚧 Nouvelle version BETA" -ForegroundColor Yellow
    Write-Host "      Version actuelle : $currentVersion" -ForegroundColor Gray
    Write-Host "      Nouvelle version : $nextBeta" -ForegroundColor Green
    Write-Host "      📝 Utilisation : Corrections de bugs, nouvelles fonctionnalités en test" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "  [2] 🏁 Version de RELEASE" -ForegroundColor Magenta
    Write-Host "      Version actuelle : $currentVersion" -ForegroundColor Gray
    Write-Host "      Nouvelle version : $nextRelease" -ForegroundColor Green
    Write-Host "      📝 Utilisation : Version stable prête pour production" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "  [3] 📈 Nouvelle version MINOR" -ForegroundColor Blue
    Write-Host "      Version actuelle : $currentVersion" -ForegroundColor Gray
    Write-Host "      Nouvelle version : $nextMinor" -ForegroundColor Green
    Write-Host "      📝 Utilisation : Nouvelles fonctionnalités importantes" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "  [4] 🚀 Nouvelle version MAJOR" -ForegroundColor Red
    Write-Host "      Version actuelle : $currentVersion" -ForegroundColor Gray
    Write-Host "      Nouvelle version : $nextMajor" -ForegroundColor Green
    Write-Host "      📝 Utilisation : Changements majeurs, breaking changes" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "  [5] ✏️  Version PERSONNALISÉE" -ForegroundColor DarkYellow
    Write-Host "      📝 Utilisation : Spécifier manuellement une version" -ForegroundColor Gray
    Write-Host ""
    
    Write-Host "  [6] ❌ ANNULER et quitter" -ForegroundColor Red
    Write-Host ""
    
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    
    do {
        Write-Host "`n💡 " -NoNewline -ForegroundColor Yellow
        $choice = Read-Host "Entrez votre choix (1-6)"
        
        switch ($choice) {
            "1" {
                $newVersion = Get-NextBetaVersion $versionInfo
                Write-Host "✅ Vous avez choisi : Nouvelle version BETA ($newVersion)" -ForegroundColor Green
                break
            }
            "2" {
                $newVersion = Get-NextReleaseVersion $versionInfo
                Write-Host "✅ Vous avez choisi : Version de RELEASE ($newVersion)" -ForegroundColor Green
                break
            }
            "3" {
                $newVersion = Get-NextMajorMinorVersion $versionInfo "minor"
                Write-Host "✅ Vous avez choisi : Nouvelle version MINOR ($newVersion)" -ForegroundColor Green
                break
            }
            "4" {
                $newVersion = Get-NextMajorMinorVersion $versionInfo "major"
                Write-Host "✅ Vous avez choisi : Nouvelle version MAJOR ($newVersion)" -ForegroundColor Green
                break
            }
            "5" {
                Write-Host "`n📝 Veuillez entrer une version personnalisée :" -ForegroundColor Cyan
                Write-Host "   Format attendu : X.Y.Z ou X.Y.Z-beta-N" -ForegroundColor Gray
                Write-Host "   Exemples : 2.0.0, 1.5.3-beta-1, 3.0.0-alpha-2" -ForegroundColor Gray
                $customVer = Read-Host "`n🎯 Version personnalisée"
                
                try {
                    $parsedCustom = Parse-Version $customVer
                    $newVersion = $customVer
                    Write-Host "✅ Version personnalisée validée : $newVersion" -ForegroundColor Green
                } catch {
                    Write-Host "❌ Format de version invalide : $customVer" -ForegroundColor Red
                    Write-Host "   Erreur : $($_.Exception.Message)" -ForegroundColor Red
                    continue
                }
                break
            }
            "6" {
                Write-Host "`n👋 Opération annulée par l'utilisateur" -ForegroundColor Yellow
                Write-Host "   Aucune modification n'a été apportée." -ForegroundColor Gray
                exit 0
            }
            default {
                Write-Host "`n❌ Choix invalide : '$choice'" -ForegroundColor Red
                Write-Host "   Veuillez entrer un nombre entre 1 et 6." -ForegroundColor Gray
                continue
            }
        }
        break
    } while ($true)
    
    # Confirm update
    Write-Host "`n" -NoNewline
    Write-Host "� CONFIRMATION DE LA MISE À JOUR" -ForegroundColor White -BackgroundColor DarkGreen
    Write-Host "`n"
    
    Write-Host "📋 Résumé des modifications :" -ForegroundColor Cyan
    Write-Host "   📍 Version actuelle  : " -NoNewline -ForegroundColor Gray
    Write-Host "$currentVersion" -ForegroundColor Red
    Write-Host "   🎯 Nouvelle version  : " -NoNewline -ForegroundColor Gray  
    Write-Host "$newVersion" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "⚠️  Cette action va modifier le fichier Directory.Build.props" -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "❓ Voulez-vous continuer avec cette mise à jour ?" -ForegroundColor White
    Write-Host "   [O] Oui, mettre à jour" -ForegroundColor Green
    Write-Host "   [N] Non, annuler (par défaut)" -ForegroundColor Red
    Write-Host ""
    
    $confirm = Read-Host "💡 Votre choix (O/n)"
    
    if ($confirm -eq "o" -or $confirm -eq "O" -or $confirm -eq "oui" -or $confirm -eq "OUI") {
        Write-Host "`n🔄 Mise à jour en cours..." -ForegroundColor Yellow
        Update-Version $newVersion
        
        Write-Host "`n🎉 MISE À JOUR TERMINÉE AVEC SUCCÈS !" -ForegroundColor Green -BackgroundColor DarkGreen
        
        # Ask if user wants to execute git commands automatically
        Write-Host "`n� Voulez-vous exécuter automatiquement les commandes Git ?" -ForegroundColor Cyan
        Write-Host "   Cela va :" -ForegroundColor Gray
        Write-Host "   • Commiter les changements avec le message 'v$newVersion'" -ForegroundColor Gray
        Write-Host "   • Pousser les changements vers origin" -ForegroundColor Gray  
        Write-Host "   • Créer et pousser le tag v$newVersion" -ForegroundColor Gray
        Write-Host ""
        Write-Host "   [O] Oui, exécuter automatiquement" -ForegroundColor Green
        Write-Host "   [N] Non, je ferai manuellement (par défaut)" -ForegroundColor Yellow
        Write-Host ""
        
        $executeGit = Read-Host "💡 Exécuter les commandes Git automatiquement (O/n)"
        
        if ($executeGit -eq "o" -or $executeGit -eq "O" -or $executeGit -eq "oui" -or $executeGit -eq "OUI") {
            Invoke-GitCommands $newVersion
        } else {
            Write-Host "`n📋 Commandes à exécuter manuellement :" -ForegroundColor Cyan
        }
        
        # Ask if user wants to create local NuGet packages
        Write-Host "`n📦 Voulez-vous créer les packages NuGet localement ?" -ForegroundColor Cyan
        Write-Host "   Cela va :" -ForegroundColor Gray
        Write-Host "   • Créer les packages .nupkg dans d:\LocalNugets\" -ForegroundColor Gray
        Write-Host "   • Permettre l'utilisation locale des packages" -ForegroundColor Gray
        Write-Host ""
        Write-Host "   [O] Oui, créer les packages localement" -ForegroundColor Green
        Write-Host "   [N] Non, je ferai manuellement (par défaut)" -ForegroundColor Yellow
        Write-Host ""
        
        $createNuget = Read-Host "💡 Créer les packages NuGet localement (O/n)"
        
        if ($createNuget -eq "o" -or $createNuget -eq "O" -or $createNuget -eq "oui" -or $createNuget -eq "OUI") {
            Invoke-LocalNugetPack $newVersion
        }
        
        # Show manual steps (always shown for reference)
        Write-Host "`n📋 Commandes Git de référence :" -ForegroundColor Cyan
        Write-Host "   1️⃣  git add . puis git commit -m `"v$newVersion`"" -ForegroundColor Yellow
        Write-Host "   2️⃣  git push origin" -ForegroundColor Yellow
        Write-Host "   3️⃣  git tag v$newVersion" -ForegroundColor Yellow
        Write-Host "   4️⃣  git push origin --tags" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "📦 Prochaines étapes (compilation et publication) :" -ForegroundColor Cyan
        Write-Host "   5️⃣  dotnet pack --output d:\LocalNugets\" -ForegroundColor Yellow
        Write-Host "   6️⃣  dotnet nuget push (vers NuGet.org)" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "✨ Version $newVersion prête pour le déploiement !" -ForegroundColor Green
        
    } else {
        Write-Host "`n❌ MISE À JOUR ANNULÉE" -ForegroundColor Red
        Write-Host "   Aucune modification n'a été apportée au projet." -ForegroundColor Gray
        Write-Host "   La version reste : $currentVersion" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

