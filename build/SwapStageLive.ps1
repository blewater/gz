# Check if azure session exists, if not then prompt for login
function Check-Session () {
    $Error.Clear()

    #if context already exist
    Get-AzureRmContext -ErrorAction Continue
    foreach ($eacherror in $Error) {
        if ($eacherror.Exception.ToString() -like "*Running Login-AzureRmAccount to login.*") {
            Login-AzureRmAccount
			Get-AzureRmSubscription -SubscriptionId "d92ca232-a672-424c-975d-1dcf45a58b0b" | Select-AzureRmSubscription
			Save-AzureRMProfile -Path .\.azSession -Force
        }
    }

    $Error.Clear();
}

# Retrieve a previously persisted azure session if it exists
Select-AzureRmProfile -Path .\.azSession

# Check if the azure session exists
Check-Session
Write-Verbose "User has a valid azure session"

# Attention: know what you are doing! Production side-effect:
# Swap Stage with Production Slots
Write-Verbose "Swapping stage with production slot"
Swap-AzureRmWebAppSlot -Name "greenzorro" -SourceSlotName "sgn" -DestinationSlotName "production" -ResourceGroupName "GreenzorroBizSpark"
