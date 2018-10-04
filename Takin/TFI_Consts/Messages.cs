using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

namespace TFI.Consts
{
    public class Messages
    {
        public const string PaymentNotMatch = "The payment you send is not match the payment calculated in the server";
        public const string DetailsInvalidPassword = "Details are invalid. password can't be updated";
        public const string DetailsInvalidEmail = "The e-mail address is invalid.";
        public const string LicenseExpired = "Your license has expired. You can no longer build new portfolios. Please contact us at info@gocherries.com to renew your license.";
        public const string InvalidPasswordCombination = "Invaild password. password must be at 8 letters and numbers combination";
        public const string UserExists = "An account has already been created with this email address.";
        public const string CalculationMethodNotUsed = "You didn't use calculation method to get the total, please use the propper method";
        public const string ConfirmationCodeInvalid = "Confirmation code is invalid";
        public const string UserNotExist = "User Not Exists";
        public const string UserAlreayLoggedin = "User is already logged in from another device.";
        public const string UserOrPasswordInvalid = "Username or password is invalid";
        public const string SecuritiesAreLoading = "Securities are loading to the system please try again in a few minutes";
    }
}
