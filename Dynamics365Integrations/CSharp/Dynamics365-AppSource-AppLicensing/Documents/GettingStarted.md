# How do I get started? #

### Technology Stack Used

	** Azure Services

		* Azure Function :  Azure function should be hosted in Partner instance to validate the customer license. Learn more about Azure Function https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview
		
		* Key Vault : In this example we are using Key Vault to store the customer Keys where Azure Function would call this key Vault and fetch the key and validate the customer license.
		Learn more about Key Vault https://docs.microsoft.com/en-us/azure/key-vault/key-vault-whatis

	
	** Dynamics 365 Services
	
		* Custom Workflow Activity : To call Azure function and validate the customer license, we are using Custom Workflow Activity. We have made Workflow Activity to just simply call the Azure Function and validate and raise notification if the License is not valid. 
		
		* Plug-In : To check the Expiry details , we are using Plug-In to check whether the given license is expired. If yes, it will raise the notification alert.
		
		* WebResource : To show client side License Notification, In this example we have used Webresource and used the OOB notification.
		
		* Customization : There is an entity (License Details : You can have your own entity) created to store the license XML information (After XML is signed).
			 Download and modify the solution accordingly.	


![](https://github.com/Azure/AzureTestDrive/blob/master/AzureTestDriveImages/HowDoesItWork5.5.png)


*	<b>Approval Email _**[Deprecated]**_</b> – The email address(es) that approval requests will be sent to – for customers who have exceeded the test drive launch limit.
