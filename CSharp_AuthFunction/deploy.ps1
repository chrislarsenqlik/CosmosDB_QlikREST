param
(
    $FunctionFilePath,
    $ArmTemplateFilePath,
    $ResourceGroupName,
    $ResourceGroupLocation
)

Connect-AzureRmAccount

function Get-ScriptDirectory { Split-Path $MyInvocation.ScriptName }
$pathToCode = Join-Path (Get-ScriptDirectory) $FunctionFilePath
$pathToTemplate = Join-Path (Get-ScriptDirectory) $ArmTemplateFilePath

# Ensure the resource group exists
New-AzureRmResourceGroup `
    -Name $ResourceGroupName `
    -Location $ResourceGroupLocation `
    -Force

# Read the contents of the function file and assemble deployment parameters
$functionFileContents = [System.IO.File]::ReadAllText($pathToCode)
$templateParameters = @{}
$templateParameters.Add("functionFile", $functionFileContents)



# Deploy the ARM template
New-AzureRmResourceGroupDeployment `
    -TemplateFile $pathToTemplate `
    -ResourceGroupName $ResourceGroupName `
    -TemplateParameterObject $templateParameters `
    -Verbose
