using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Framework.Core.Utils
{
    public class ValidationsHelper
    {
       
        #region Props
        public static readonly string IDNumberPattern = @"^\d{5,9}$";
        public static readonly string EmailPattern = @"^[-a-zA-Z0-9~!$%^&*_=+}{\'?]+(\.[-a-zA-Z0-9~!$%^&*_=+}{\'?]+)*@([a-zA-Z0-9_][-a-zA-Z0-9_]*(\.[-a-zA-Z0-9_]+)*\.(aero|arpa|biz|com|coop|edu|gov|info|int|mil|museum|name|net|org|pro|travel|club|family|mobi|[a-zA-Z][a-zA-Z])|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,5})?$";
        public static readonly string PhonePattern = @"^\d{7,7}$";
        public static readonly string PhoneAreaCodePattern = @"^\d{2,3}$";
        
        #endregion

        #region Validations

        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, EmailPattern, RegexOptions.IgnoreCase);              
        }

        public static bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(phone, PhonePattern);
        }

        public static bool IsValidPhoneAreaCode(string areaCode)
        {
            return Regex.IsMatch(areaCode, PhoneAreaCodePattern);
        }
      
        public static bool IsValidIDNumber(string idNumber)
        {
            // Validate correct input 
            if (!Regex.IsMatch(idNumber, IDNumberPattern))
                return false;
            // The number is too short - add leading 0000 
            if (idNumber.Length < 9)
            {
                while (idNumber.Length < 9)
                {
                    idNumber =
                    '0' + idNumber;
                }
            }
            // CHECK THE ID NUMBER 
            int mone = 0;
            int incNum;
            for (int i = 0; i < 9; i++)
            {
                incNum =
                Convert.ToInt32(idNumber[i].ToString());
                incNum *= (i % 2) + 1;
                if (incNum > 9)
                    incNum -= 9;
                mone += incNum;
            }
            if (mone % 10 == 0)
                return true;
            else
                return false;
        }


        #endregion


    }
}
