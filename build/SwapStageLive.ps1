# Check if azure session exists, if not then prompt for login
function Check-Session () {
    $Error.Clear()

    #if context already exist
    Select-AzureRmProfile -Path .\.azSession -ErrorAction Continue
    #Get-AzureRmContext -ErrorAction Continue
    foreach ($eacherror in $Error) {
        if ($eacherror.Exception.ToString() -like "*Running Login-AzureRmAccount to login.*") {
            Login-AzureRmAccount
			#Get-AzureRmSubscription -SubscriptionId "d92ca232-a672-424c-975d-1dcf45a58b0b" | Select-AzureRmSubscription
            Get-AzureRmSubscription -SubscriptionId "500c96ff-15a2-4861-8a33-8872bdcb6b58" | Select-AzureRmSubscription
			Save-AzureRMProfile -Path .\.azSession -Force
        }
    }

    $Error.Clear();
}

# Retrieve a previously persisted azure session if it exists
# Check if the azure session exists
Check-Session
echo "User has a valid azure session"

#Select-AzureRmProfile -Path .\.azSession
#Get-AzureRmSubscription -SubscriptionId "500c96ff-15a2-4861-8a33-8872bdcb6b58" | Select-AzureRmSubscription

# Attention: know what you are doing! Production side-effect:
# Swap Stage with Production Slots
echo "Swapping stage with production slot"
#Swap-AzureRmWebAppSlot -Name "greenzorro" -SourceSlotName "sgn" -DestinationSlotName "production" -ResourceGroupName "GreenzorroBizSpark"
Swap-AzureRmWebAppSlot -Name "greenzorro" -SourceSlotName "sgn" -DestinationSlotName "production" -ResourceGroupName "2ndSub_All_BizSpark_RG"

echo "Opening greenzorro in browser"
[System.Diagnostics.Process]::Start("https://www.greenzorro.com")
