// JavaScript source code
function ShowNotification() {
    var outputText = "Your Dynamics365 App License has expired. To continue to use this App, please renew your license from Contoso Inc.\n";
    var query = "?$select=cxp_licensename,cxp_licensedetails&$filter=(cxp_licensename eq 'Expired' and cxp_licensedetails eq 'Yes')";

    Xrm.WebApi.retrieveMultipleRecords("cxp_appicationlicense", query).then(
        function success(result) {
            //for (var accountRecordsCount = 0; accountRecordsCount < result.entities.length; accountRecordsCount++) {
            if (result.entities.length >= 0) {
                //outputText += "---" + result.entities[0].new_name;
                //outputText += "---" + result.entities.length;
                //Xrm.Utility.alertDialog(outputText, null);

                Xrm.Page.ui.setFormNotification("Your Dynamics365 App License has expired. To continue to use this App, please renew your license from Contoso Inc.", "INFO", "2001");

                var alertStrings = { confirmButtonLabel: "Ok", text: outputText };
                var alertOptions = { height: 200, width: 350 };
                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);

            }

            //outputText += result.entities[accountRecordsCount].new_name + "\t\t" + result.entities[accountRecordsCount].new_value + "\n";
            //}    
        },
        function (error) {
            // Handle error conditions
            Xrm.Utility.alertDialog(error.message, null);
        });
}