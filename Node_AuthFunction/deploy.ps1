param
(
    $FunctionFilePath,
    $PackageJsonFilePath,
    $ArmTemplateFilePath,
    $ResourceGroupName,
    $ResourceGroupLocation
)

Connect-AzureRmAccount

function Get-ScriptDirectory { Split-Path $MyInvocation.ScriptName }
$pathToFuncCode = Join-Path (Get-ScriptDirectory) $FunctionFilePath
$pathToPackageCode = Join-Path (Get-ScriptDirectory) $PackageJsonFilePath
$pathToTemplate = Join-Path (Get-ScriptDirectory) $ArmTemplateFilePath

# Ensure the resource group exists
New-AzureRmResourceGroup `
    -Name $ResourceGroupName `
    -Location $ResourceGroupLocation `
    -Force

# Read the contents of the function file and assemble deployment parameters
$functionFileContents = [System.IO.File]::ReadAllText($pathToFuncCode)
$packageFileContents = [System.IO.File]::ReadAllText($pathToPackageCode)
$templateParameters = @{}
$templateParameters.Add("functionFile", $functionFileContents)
$templateParameters.Add("packageFunctionFile", $packageFileContents)

# Deploy the ARM template
New-AzureRmResourceGroupDeployment `
    -TemplateFile $pathToTemplate `
    -ResourceGroupName $ResourceGroupName `
    -TemplateParameterObject $templateParameters `
    -Verbose