# Check if azure session exists, if not then prompt for login
function Check-Session () {
    $Error.Clear()

    #if context already exist
    Get-AzureRmContext -ErrorAction Continue
    foreach ($eacherror in $Error) {
        if ($eacherror.Exception.ToString() -like "*Running Login-AzureRmAccount to login.*") {
            Login-AzureRmAccount
        }
    }

    $Error.Clear();
}

# Retrieve a previously persisted azure session if it exists
Select-AzureRmProfile -Path .\.azSession

# Check if the azure session exists
Check-Session
Write-Verbose "User has a valid azure session"

# Check the Greenzorro subscription
Select-AzureRmSubscription -SubscriptionName "First Visual Studio Enterprise: BizSpark"

# Persist the azure session for the above subscription
Save-AzureRMProfile -Path .\.azSession

# Attention: know what you are doing! Production side-effect:
# Swap Stage with Production Slots
Swap-AzureRmWebAppSlot -Name "greenzorro" -SourceSlotName "sgn" -DestinationSlotName "production" -ResourceGroupName "GreenzorroBizSpark"
