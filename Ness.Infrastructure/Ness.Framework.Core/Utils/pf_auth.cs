//
// ---------------
// 
// Copyright (c) 2008-2015 PhoneFactor, Inc.
// 
// Permission is hereby granted, free of charge, to any person
// obtaining  a copy of this software and associated documentation
// files (the "Software"),  to deal in the Software without
// restriction, including without limitation the  rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT  SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,  ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER  DEALINGS IN THE SOFTWARE.
// 
// ---------------
//

// 
// pf_auth.cs: An SDK for authenticating with PhoneFactor for .NET 2.0.
// version: 2.20
//

using System;
using System.Xml;
using System.Net;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections;
using System.Threading;

namespace Framework.Core.Utils
{
    public class PfAuthParams
    {
        public string Username = "";
        public string CountryCode = "972";
        public string Phone = "";
        public string Extension = "";
        public bool AllowInternationalCalls = false;
        public string Mode = pf_auth.MODE_STANDARD;
        public string Pin = "";
        public string Sha1PinHash = "";
        public string Sha1Salt = "";
        public string Language = "he-IL";
        public string SmsText = "<$otp$>";
        public string ApplicationName = "";
        public string DeviceToken = "";
        public string NotificationType = "";
        public string AccountName = "";
        public string Hostname = "TestHostName";
        public string IpAddress = "255.255.255.255";
        public string CertFilePath = "c:\\cert_key.p12";
        public string RequestId = "";
        public string ResponseUrl = "";
    }

    public class ResetVoiceprintParams
    {
        public string Username = "";
        public string Hostname = "TestHostName";
        public string IpAddress = "255.255.255.255";
        public string CertFilePath = "c:\\cert_key.p12";
    }

    public class ValidateDeviceTokenParams
    {
        public string Username = "";
        public string DeviceToken = "";
        public string NotificationType = "";
        public string AccountName = "";
        public string Hostname = "TestHostName";
        public string IpAddress = "255.255.255.255";
        public string CertFilePath = "c:\\cert_key.p12";
    }

    public static class pf_auth
    {
        public const string MODE_STANDARD = "standard";
        public const string MODE_PIN = "pin";
        public const string MODE_VOICEPRINT = "voiceprint";
        public const string MODE_SMS_TWO_WAY_OTP = "sms_two_way_otp";
        public const string MODE_SMS_TWO_WAY_OTP_PLUS_PIN = "sms_two_way_otp_plus_pin";
        public const string MODE_SMS_ONE_WAY_OTP = "sms_one_way_otp";
        public const string MODE_SMS_ONE_WAY_OTP_PLUS_PIN = "sms_one_way_otp_plus_pin";
        public const string MODE_PHONE_APP_STANDARD = "phone_app_standard";
        public const string MODE_PHONE_APP_PIN = "phone_app_pin";

        public const string NOTIFICATION_TYPE_APNS = "apns"; // iOS
        public const string NOTIFICATION_TYPE_C2DM = "c2dm"; // Android
        public const string NOTIFICATION_TYPE_GCM = "gcm";   // Android
        public const string NOTIFICATION_TYPE_MPNS = "mpns"; // Windows
        public const string NOTIFICATION_TYPE_BPS = "bps";   // Blackberry

        private const string LICENSE_KEY = "JREKRQPCPOMM";
        private const string GROUP_KEY = "b0da06b6df082165776ddde6d357d18e";
        private const string CERT_PASSWORD = "AKGUZ1HNOIU57FFY";

        private static string mCurrentTarget = "https://pfd.phonefactor.net/pfd/pfd.pl";
        private static Queue mTargets = new Queue(new object[] { "https://pfd2.phonefactor.net/pfd/pfd.pl" });
        private static Mutex mMutex = new Mutex();

        // 
        // pf_authenticate, pf_authenticate_async: authenticates using PhoneFactor.
        // 
        // Arguments:
        //     1) username: the username to be auth'd
        //     2) country_code: the country code to be used for the call.  defaults to 1.
        //     3) phone: the phone number to PhoneFactor authenticate
        //     4) extension: the extension to dial after the call is connected
        //     5) allow_int_calls: a boolean value that determines whether international
        //        calls should be allowed.  defaults to false.  note that this only needs to 
        //        be set to true if the call you are making is international, and thus could
        //        cost you money.  see www.phonefactor.net for the PhoneFactor rate table
        //        that shows which calling zones will cost money and which are free.
        //     6) mode: specify whether to use "standard", "pin", "voiceprint",
        //        "sms_two_way_otp" or "sms_two_way_otp_plus_pin" mode
        //        standard: user presses #
        //        pin: user enters their pin and #
        //        voiceprint: user says their passphrase and their voice is matched
        //        sms_two_way_otp: user responds to text with OTP
        //        sms_two_way_otp_plus_pin: user responds to text with OTP + PIN
        //        phone_app_standard: user selects Authenticate in phone app
        //        phone_app_pin: user enters their pin and selects Authenticate in the phone app
        //     7) pin: PIN the user must enter during the call
        //     8) sha1_pin_hash: SHA1 encrypted PIN the user must enter during the call
        //     9) sha1_salt: SHA1 salt appended to PIN prior to encryption
        //   	 10) $application_name: Specify the appliation name to display on reports in the
        //         PhoneFactor Management Portal.
        //   	 11) $device_token: Specify the device token of the device to send the
        //		     phone app notification to.
        //	   12) $account_name: Specify the account name to display in the phone app.
        //     13) hostname: the hostname this authentication is being sent from.
        //           defaults to 'pfsdk-hostname'
        //     14) ip: the ip address this authentication is being sent from.
        //           defaults to '255.255.255.255'
        //     15) cert_file_path: the path and file name of the certificate file
        //     16) request_id: a string identifying the request that can be used to match
        //         the asynchronous response to the original request
        //     17) response_url: the URL that the final response for an asynchronous request
        //         should be sent to
        //     18) otp: One-time passcode (OTP) included in SMS text and returned for
        //         one-way SMS.
        //     19) call_status: an integer code returned representing the status of the
        //         phonecall.
        //     20) error_id: an integer code returned if the connection to the PhoneFactor
        //         backend failed.
        // 
        // Return value:
        //     A boolean value representing whether the auth was successful or not.
        //     If true, then the call_status and error_id output arguments can safely be
        //     ignored.
        //

        // New implementations using class to pass parameters.
        public static bool pf_authenticate(
          PfAuthParams pfAuthParams,
          out string otp,
          out int callStatus,
          out int errorId)
        {
            return pf_authenticate_internal(
              pfAuthParams,
              false,
              out otp,
              out callStatus,
              out errorId);
        }

        public static bool pf_authenticate(
          PfAuthParams pfAuthParams,
          out int callStatus,
          out int errorId)
        {
            return pf_authenticate_internal(
              pfAuthParams,
              false,
              out callStatus,
              out errorId);
        }

        public static bool pf_authenticate_async(
          PfAuthParams pfAuthParams,
          out int callStatus,
          out int errorId)
        {
            return pf_authenticate_internal(
              pfAuthParams,
              true,
              out callStatus,
              out errorId);
        }

        public static bool reset_voiceprint(
          ResetVoiceprintParams resetVoiceprintParams,
          out int result,
          out int error_id)
        {
            return reset_voiceprint_internal(
              resetVoiceprintParams,
              out result,
              out error_id);
        }

        public static bool validate_device_token(
          ValidateDeviceTokenParams validateDeviceTokenParams,
          out int result,
          out int error_id)
        {
            return validate_device_token_internal(
              validateDeviceTokenParams,
              out result,
              out error_id);
        }

        // Standard Mode.
        public static bool pf_authenticate(
          string username,
          string phone,
          string country_code,
          bool allow_int_calls,
          string hostname,
          string ip,
          string cert_file_path,
          out int call_status,
          out int error_id)
        {
            PfAuthParams pfAuthParams = new PfAuthParams();
            pfAuthParams.Username = username;
            pfAuthParams.CountryCode = country_code;
            pfAuthParams.Phone = phone;
            pfAuthParams.AllowInternationalCalls = allow_int_calls;
            pfAuthParams.Mode = MODE_STANDARD;
            pfAuthParams.Hostname = hostname;
            pfAuthParams.IpAddress = ip;
            pfAuthParams.CertFilePath = cert_file_path;

            return pf_authenticate_internal(
              pfAuthParams,
              false,
              out call_status,
              out error_id);
        }

        // Standard Mode Asynchronous.
        public static bool pf_authenticate_async(
          string request_id,
          string username,
          string phone,
          string country_code,
          bool allow_int_calls,
          string hostname,
          string ip,
          string cert_file_path,
          string response_url,
          out int call_status,
          out int error_id)
        {
            PfAuthParams pfAuthParams = new PfAuthParams();
            pfAuthParams.Username = username;
            pfAuthParams.CountryCode = country_code;
            pfAuthParams.Phone = phone;
            pfAuthParams.AllowInternationalCalls = allow_int_calls;
            pfAuthParams.Mode = MODE_STANDARD;
            pfAuthParams.Hostname = hostname;
            pfAuthParams.IpAddress = ip;
            pfAuthParams.CertFilePath = cert_file_path;
            pfAuthParams.RequestId = request_id;
            pfAuthParams.ResponseUrl = response_url;

            return pf_authenticate_internal(
              pfAuthParams,
              true,
              out call_status,
              out error_id);
        }

        // Deprecated - for backward compatibility only
        public static bool pf_authenticate_async(
          string username,
          string phone,
          string country_code,
          bool allow_int_calls,
          string hostname,
          string ip,
          string cert_file_path,
          string response_url,
          out int call_status,
          out int error_id)
        {
            PfAuthParams pfAuthParams = new PfAuthParams();
            pfAuthParams.Username = username;
            pfAuthParams.CountryCode = country_code;
            pfAuthParams.Phone = phone;
            pfAuthParams.AllowInternationalCalls = allow_int_calls;
            pfAuthParams.Mode = MODE_STANDARD;
            pfAuthParams.Hostname = hostname;
            pfAuthParams.IpAddress = ip;
            pfAuthParams.CertFilePath = cert_file_path;
            pfAuthParams.ResponseUrl = response_url;

            return pf_authenticate_internal(
              pfAuthParams,
              true,
              out call_status,
              out error_id);
        }

        // PIN Mode with clear text PIN.
        public static bool pf_authenticate(
          string username,
          string phone,
          string country_code,
          bool allow_int_calls,
          string pin,
          string hostname,
          string ip,
          string cert_file_path,
          out int call_status,
          out int error_id)
        {
            PfAuthParams pfAuthParams = new PfAuthParams();
            pfAuthParams.Username = username;
            pfAuthParams.CountryCode = country_code;
            pfAuthParams.Phone = phone;
            pfAuthParams.AllowInternationalCalls = allow_int_calls;
            pfAuthParams.Mode = MODE_PIN;
            pfAuthParams.Pin = pin;
            pfAuthParams.Hostname = hostname;
            pfAuthParams.IpAddress = ip;
            pfAuthParams.CertFilePath = cert_file_path;

            return pf_authenticate_internal(
              pfAuthParams,
              false,
              out call_status,
              out error_id);
        }

        // PIN Mode with clear text PIN asynchronous.
        public static bool pf_authenticate_async(
          string request_id,
          string username,
          string phone,
          string country_code,
          bool allow_int_calls,
          string pin,
          string hostname,
          string ip,
          string cert_file_path,
          string response_url,
          out int call_status,
          out int error_id)
        {
            PfAuthParams pfAuthParams = new PfAuthParams();
            pfAuthParams.Username = username;
            pfAuthParams.CountryCode = country_code;
            pfAuthParams.Phone = phone;
            pfAuthParams.AllowInternationalCalls = allow_int_calls;
            pfAuthParams.Mode = MODE_PIN;
            pfAuthParams.Pin = pin;
            pfAuthParams.Hostname = hostname;
            pfAuthParams.IpAddress = ip;
            pfAuthParams.CertFilePath = cert_file_path;
            pfAuthParams.RequestId = request_id;
            pfAuthParams.ResponseUrl = response_url;

            return pf_authenticate_internal(
              pfAuthParams,
              true,
              out call_status,
              out error_id);
        }

        // Deprecated - for backward compatibility only
        public static bool pf_authenticate_async(
          string username,
          string phone,
          string country_code,
          bool allow_int_calls,
          string pin,
          string hostname,
          string ip,
          string cert_file_path,
          string response_url,
          out int call_status,
          out int error_id)
        {
            PfAuthParams pfAuthParams = new PfAuthParams();
            pfAuthParams.Username = username;
            pfAuthParams.CountryCode = country_code;
            pfAuthParams.Phone = phone;
            pfAuthParams.AllowInternationalCalls = allow_int_calls;
            pfAuthParams.Mode = MODE_PIN;
            pfAuthParams.Pin = pin;
            pfAuthParams.Hostname = hostname;
            pfAuthParams.IpAddress = ip;
            pfAuthParams.CertFilePath = cert_file_path;
            pfAuthParams.ResponseUrl = response_url;

            return pf_authenticate_internal(
              pfAuthParams,
              true,
              out call_status,
              out error_id);
        }

        // PIN Mode with SHA1 PIN.
        public static bool pf_authenticate(
          string username,
          string phone,
          string country_code,
          bool allow_int_calls,
          string sha1_pin_hash,
          string sha1_salt,
          string hostname,
          string ip,
          string cert_file_path,
          out int call_status,
          out int error_id)
        {
            PfAuthParams pfAuthParams = new PfAuthParams();
            pfAuthParams.Username = username;
            pfAuthParams.CountryCode = country_code;
            pfAuthParams.Phone = phone;
            pfAuthParams.AllowInternationalCalls = allow_int_calls;
            pfAuthParams.Mode = MODE_PIN;
            pfAuthParams.Sha1PinHash = sha1_pin_hash;
            pfAuthParams.Sha1Salt = sha1_salt;
            pfAuthParams.Hostname = hostname;
            pfAuthParams.IpAddress = ip;
            pfAuthParams.CertFilePath = cert_file_path;

            return pf_authenticate_internal(
              pfAuthParams,
              false,
              out call_status,
              out error_id);
        }

        // PIN Mode with SHA1 PIN asynchronous.
        public static bool pf_authenticate_async(
          string request_id,
          string username,
          string phone,
          string country_code,
          bool allow_int_calls,
          string sha1_pin_hash,
          string sha1_salt,
          string hostname,
          string ip,
          string cert_file_path,
          string response_url,
          out int call_status,
          out int error_id)
        {
            PfAuthParams pfAuthParams = new PfAuthParams();
            pfAuthParams.Username = username;
            pfAuthParams.CountryCode = country_code;
            pfAuthParams.Phone = phone;
            pfAuthParams.AllowInternationalCalls = allow_int_calls;
            pfAuthParams.Mode = MODE_PIN;
            pfAuthParams.Sha1PinHash = sha1_pin_hash;
            pfAuthParams.Sha1Salt = sha1_salt;
            pfAuthParams.Hostname = hostname;
            pfAuthParams.IpAddress = ip;
            pfAuthParams.CertFilePath = cert_file_path;
            pfAuthParams.RequestId = request_id;
            pfAuthParams.ResponseUrl = response_url;

            return pf_authenticate_internal(
              pfAuthParams,
              true,
              out call_status,
              out error_id);
        }

        // Deprecated - for backward compatibility only
        public static bool pf_authenticate_async(
          string username,
          string phone,
          string country_code,
          bool allow_int_calls,
          string sha1_pin_hash,
          string sha1_salt,
          string hostname,
          string ip,
          string cert_file_path,
          string response_url,
          out int call_status,
          out int error_id)
        {
            PfAuthParams pfAuthParams = new PfAuthParams();
            pfAuthParams.Username = username;
            pfAuthParams.CountryCode = country_code;
            pfAuthParams.Phone = phone;
            pfAuthParams.AllowInternationalCalls = allow_int_calls;
            pfAuthParams.Mode = MODE_PIN;
            pfAuthParams.Sha1PinHash = sha1_pin_hash;
            pfAuthParams.Sha1Salt = sha1_salt;
            pfAuthParams.Hostname = hostname;
            pfAuthParams.IpAddress = ip;
            pfAuthParams.CertFilePath = cert_file_path;
            pfAuthParams.ResponseUrl = response_url;

            return pf_authenticate_internal(
              pfAuthParams,
              true,
              out call_status,
              out error_id);
        }

        private static bool pf_authenticate_internal(
          PfAuthParams pfAuthParams,
          bool asynchronous,
          out int call_status,
          out int error_id)
        {
            string otp = "";
            return pf_authenticate_internal(pfAuthParams, asynchronous, out otp, out call_status, out error_id);
        }

        private static bool pf_authenticate_internal(
          PfAuthParams pfAuthParams,
          bool asynchronous,
          out string otp,
          out int call_status,
          out int error_id)
        {
            if (pfAuthParams.CountryCode.Length == 0) pfAuthParams.CountryCode = "1";
            if (pfAuthParams.Hostname.Length == 0) pfAuthParams.Hostname = "pfsdk-hostname";
            if (pfAuthParams.IpAddress.Length == 0) pfAuthParams.IpAddress = "255.255.255.255";
            pfAuthParams.Mode = pfAuthParams.Mode.ToLower();
            if (pfAuthParams.Mode != MODE_STANDARD && pfAuthParams.Mode != MODE_PIN && pfAuthParams.Mode != MODE_VOICEPRINT
              && pfAuthParams.Mode != MODE_SMS_TWO_WAY_OTP && pfAuthParams.Mode != MODE_SMS_TWO_WAY_OTP_PLUS_PIN
              && pfAuthParams.Mode != MODE_SMS_ONE_WAY_OTP && pfAuthParams.Mode != MODE_SMS_ONE_WAY_OTP_PLUS_PIN
              && pfAuthParams.Mode != MODE_PHONE_APP_STANDARD && pfAuthParams.Mode != MODE_PHONE_APP_PIN) pfAuthParams.Mode = MODE_STANDARD;

            otp = "";
            call_status = 0;
            error_id = 0;

            string auth_message = create_authenticate_message(pfAuthParams, asynchronous);

            int tries = 1;
            if (mMutex.WaitOne())
            {
                try { tries = mTargets.Count + 1; }
                finally { mMutex.ReleaseMutex(); }
            }
            for (int i = 0; i < tries; i++)
            {
                string response;
                if (send_message(mCurrentTarget, auth_message, pfAuthParams.CertFilePath, out response))
                {
                    string request_id_out = "";
                    bool authenticated = get_response_status(response, out request_id_out, out otp, out call_status, out error_id);
                    return authenticated;
                }
                else
                {
                    if (mMutex.WaitOne())
                    {
                        try
                        {
                            mTargets.Enqueue(mCurrentTarget);
                            mCurrentTarget = mTargets.Dequeue().ToString();
                        }
                        finally { mMutex.ReleaseMutex(); }
                    }
                }
            }
            return false;
        }

        private static bool reset_voiceprint_internal(
          ResetVoiceprintParams resetVoiceprintParams,
          out int result,
          out int error_id)
        {
            if (resetVoiceprintParams.Hostname.Length == 0) resetVoiceprintParams.Hostname = "pfsdk-hostname";
            if (resetVoiceprintParams.IpAddress.Length == 0) resetVoiceprintParams.IpAddress = "255.255.255.255";
            result = 0;
            error_id = 0;

            string voiceprint_reset_message = create_voiceprint_reset_message(resetVoiceprintParams);

            int tries = 1;
            if (mMutex.WaitOne())
            {
                try { tries = mTargets.Count + 1; }
                finally { mMutex.ReleaseMutex(); }
            }
            for (int i = 0; i < tries; i++)
            {
                string response;
                if (send_message(mCurrentTarget, voiceprint_reset_message, resetVoiceprintParams.CertFilePath, out response))
                {
                    bool success = get_voiceprint_reset_response_status(response, out result, out error_id);
                    return success;
                }
                else
                {
                    if (mMutex.WaitOne())
                    {
                        try
                        {
                            mTargets.Enqueue(mCurrentTarget);
                            mCurrentTarget = mTargets.Dequeue().ToString();
                        }
                        finally { mMutex.ReleaseMutex(); }
                    }
                }
            }
            return false;
        }

        private static bool validate_device_token_internal(
          ValidateDeviceTokenParams validateDeviceTokenParams,
          out int result,
          out int error_id)
        {
            if (validateDeviceTokenParams.Hostname.Length == 0) validateDeviceTokenParams.Hostname = "pfsdk-hostname";
            if (validateDeviceTokenParams.IpAddress.Length == 0) validateDeviceTokenParams.IpAddress = "255.255.255.255";
            result = 0;
            error_id = 0;

            string validate_device_token_message = create_validate_device_token_message(validateDeviceTokenParams);

            int tries = 1;
            if (mMutex.WaitOne())
            {
                try { tries = mTargets.Count + 1; }
                finally { mMutex.ReleaseMutex(); }
            }
            for (int i = 0; i < tries; i++)
            {
                string response;
                if (send_message(mCurrentTarget, validate_device_token_message, validateDeviceTokenParams.CertFilePath, out response))
                {
                    bool success = get_validate_device_token_response_status(response, out result, out error_id);
                    return success;
                }
                else
                {
                    if (mMutex.WaitOne())
                    {
                        try
                        {
                            mTargets.Enqueue(mCurrentTarget);
                            mCurrentTarget = mTargets.Dequeue().ToString();
                        }
                        finally { mMutex.ReleaseMutex(); }
                    }
                }
            }
            return false;
        }

        // 
        // create_authenticate_message: generates an authenticate message to be sent
        // 	to the PhoneFactor backend.
        //  
        // Return value:
        //     a complete authentication xml message ready to be sent to the PhoneFactor backend
        // 
        private static string create_authenticate_message(
          PfAuthParams pfAuthParams,
          bool asynchronous)
        {
            bool sms = pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP || pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP_PLUS_PIN
              || pfAuthParams.Mode == MODE_SMS_ONE_WAY_OTP || pfAuthParams.Mode == MODE_SMS_ONE_WAY_OTP_PLUS_PIN;
            bool two_way = pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP || pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP_PLUS_PIN;
            bool otp = pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP || pfAuthParams.Mode == MODE_SMS_ONE_WAY_OTP;
            bool phone_app = pfAuthParams.Mode == MODE_PHONE_APP_STANDARD || pfAuthParams.Mode == MODE_PHONE_APP_PIN;
            bool phone_app_pin = pfAuthParams.Mode == MODE_PHONE_APP_PIN;

            XmlDocument doc = new XmlDocument();

            // start message
            // <pfpMessage></pfpMessage>
            XmlElement root = doc.CreateElement("pfpMessage");
            root.SetAttribute("version", "1.5");
            doc.AppendChild(root);

            // message header
            // <header></header>
            XmlElement header = doc.CreateElement("header");
            root.AppendChild(header);
            XmlElement source = doc.CreateElement("source");
            header.AppendChild(source);
            XmlElement component = doc.CreateElement("component");
            component.SetAttribute("type", "pfsdk");
            source.AppendChild(component);
            XmlElement element = doc.CreateElement("host");
            element.SetAttribute("ip", pfAuthParams.IpAddress);
            element.SetAttribute("hostname", pfAuthParams.Hostname);
            component.AppendChild(element);

            // request
            // <request></request>
            XmlElement request = doc.CreateElement("request");
            Random random = new Random();
            if (pfAuthParams.RequestId == null || pfAuthParams.RequestId.Length == 0) pfAuthParams.RequestId = random.Next(10000).ToString();
            request.SetAttribute("request-id", pfAuthParams.RequestId);
            request.SetAttribute("language", pfAuthParams.Language);
            request.SetAttribute("async", asynchronous ? "1" : "0");
            request.SetAttribute("response-url", pfAuthParams.ResponseUrl);
            root.AppendChild(request);
            XmlElement auth_request = doc.CreateElement("authenticationRequest");
            if (sms) auth_request.SetAttribute("mode", "sms");
            if (phone_app) auth_request.SetAttribute("mode", "phoneApp");
            request.AppendChild(auth_request);
            XmlElement customer = doc.CreateElement("customer");
            auth_request.AppendChild(customer);
            element = doc.CreateElement("licenseKey");
            element.InnerText = LICENSE_KEY;
            customer.AppendChild(element);
            element = doc.CreateElement("groupKey");
            element.InnerText = GROUP_KEY;
            customer.AppendChild(element);
            element = doc.CreateElement("countryCode");
            element.InnerText = pfAuthParams.CountryCode;
            auth_request.AppendChild(element);
            element = doc.CreateElement("authenticationType");
            element.InnerText = "pfsdk";
            auth_request.AppendChild(element);
            element = doc.CreateElement("username");
            element.InnerText = pfAuthParams.Username;
            auth_request.AppendChild(element);
            element = doc.CreateElement("phonenumber");
            element.SetAttribute("userCanChangePhone", "no");
            element.InnerText = pfAuthParams.Phone;
            if (pfAuthParams.Extension != null && pfAuthParams.Extension.Length != 0)
            {
                element.SetAttribute("extension", pfAuthParams.Extension);
            }
            auth_request.AppendChild(element);
            element = doc.CreateElement("allowInternationalCalls");
            element.InnerText = pfAuthParams.AllowInternationalCalls ? "yes" : "no";
            auth_request.AppendChild(element);
            element = doc.CreateElement("applicationName");
            element.InnerText = pfAuthParams.ApplicationName;
            auth_request.AppendChild(element);

            XmlElement pin_info = doc.CreateElement("pinInfo");
            XmlElement pin_element = doc.CreateElement("pin");
            if (pfAuthParams.Mode == MODE_PIN || pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP_PLUS_PIN)
            {
                string pinFormat;
                string pinFormatted;
                if (pfAuthParams.Sha1PinHash.Length == 0)
                {
                    pinFormat = "plainText";
                    pinFormatted = pfAuthParams.Pin;
                }
                else
                {
                    pinFormat = "sha1";
                    pinFormatted = pfAuthParams.Sha1PinHash;
                }

                pin_element.SetAttribute("pinFormat", pinFormat);
                if (pfAuthParams.Sha1PinHash.Length != 0)
                {
                    pin_element.SetAttribute("sha1Salt", pfAuthParams.Sha1Salt);
                }
                pin_element.SetAttribute("pinChangeRequired", "no");
                pin_element.InnerText = pinFormatted;
            }

            if (sms)
            {
                XmlElement sms_info = doc.CreateElement("smsInfo");
                sms_info.SetAttribute("direction", two_way ? "two-way" : "one-way");
                sms_info.SetAttribute("mode", otp ? "otp" : "otp-pin");
                element = doc.CreateElement("message");
                element.InnerText = pfAuthParams.SmsText;
                sms_info.AppendChild(element);
                if (pfAuthParams.Mode == MODE_SMS_TWO_WAY_OTP_PLUS_PIN) sms_info.AppendChild(pin_element);
                auth_request.AppendChild(sms_info);
                pin_info.SetAttribute("pinMode", MODE_STANDARD);
            }
            else if (phone_app)
            {
                XmlElement phone_app_auth_info = doc.CreateElement("phoneAppAuthInfo");
                phone_app_auth_info.SetAttribute("mode", phone_app_pin ? "pin" : "standard");
                XmlElement device_tokens = doc.CreateElement("deviceTokens");
                element = doc.CreateElement("deviceToken");
                if (pfAuthParams.NotificationType.Length > 0)
                {
                    element.SetAttribute("notificationType", pfAuthParams.NotificationType);
                }
                element.InnerText = pfAuthParams.DeviceToken;
                device_tokens.AppendChild(element);
                phone_app_auth_info.AppendChild(device_tokens);
                element = doc.CreateElement("phoneAppAccountName");
                element.InnerText = pfAuthParams.AccountName;
                phone_app_auth_info.AppendChild(element);
                if (phone_app_pin)
                {
                    element = doc.CreateElement("pin");
                    element.SetAttribute("pinChangeRequired", "0");
                    string pinFormat;
                    string pinFormatted;
                    if (pfAuthParams.Sha1PinHash.Length == 0)
                    {
                        pinFormat = "plainText";
                        pinFormatted = pfAuthParams.Pin;
                    }
                    else
                    {
                        pinFormat = "sha1";
                        pinFormatted = pfAuthParams.Sha1PinHash;
                    }
                    element.SetAttribute("pinFormat", pinFormat);
                    if (pfAuthParams.Sha1PinHash.Length != 0)
                    {
                        element.SetAttribute("sha1Salt", pfAuthParams.Sha1Salt);
                    }
                    element.InnerText = pinFormatted;
                    phone_app_auth_info.AppendChild(element);
                }
                XmlElement phone_app_messages = doc.CreateElement("phoneAppMessages");
                element = doc.CreateElement("message");
                element.SetAttribute("type", "authenticateButton");
                element.InnerText = "Authenticate";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "authenticationDenied");
                element.InnerText = "PhoneFactor authentication denied.";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "authenticationSuccessful");
                element.InnerText = "You have successfully authenticated using PhoneFactor.";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "cancelButton");
                element.InnerText = "Cancel";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "closeButton");
                element.InnerText = "Close";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "denyAndReportFraudButton");
                element.InnerText = "Deny and Report Fraud";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "denyButton");
                element.InnerText = "Deny";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "fraudConfirmationNoBlock");
                element.InnerText = "Your company's fraud response team will be notified.";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "fraudConfirmationWithBlock");
                element.InnerText = "Your account will be blocked preventing further authentications and the company's fraud response team will be notified.";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "fraudReportedNoBlock");
                element.InnerText = "Fraud reported.";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "fraudReportedWithBlock");
                element.InnerText = "Fraud reported and account blocked.";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "notification");
                element.InnerText = "You have received a PhoneFactor authentication request.";
                phone_app_messages.AppendChild(element);
                element = doc.CreateElement("message");
                element.SetAttribute("type", "reportFraudButton");
                element.InnerText = "Report Fraud";
                phone_app_messages.AppendChild(element);

                if (pfAuthParams.Mode == MODE_PHONE_APP_STANDARD)
                {
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "standard");
                    element.InnerText = "Tap Authenticate to complete your authentication.";
                    phone_app_messages.AppendChild(element);
                }
                else
                {
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "confirmPinField");
                    element.InnerText = "Confirm PIN";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "newPinField");
                    element.InnerText = "New PIN";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "pin");
                    element.InnerText = "Enter your PIN and tap Authenticate to complete your authentication.";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "pinAllSameDigits");
                    element.InnerText = "Your PIN cannot contain 3 or more repeating digits.";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "pinExpired");
                    element.InnerText = "Your PIN has expired. Please enter a new PIN to complete your authentication.";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "pinField");
                    element.InnerText = "PIN";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "pinHistoryDuplicate");
                    element.InnerText = "Your PIN cannot be the same as one of your recently used PINs. Please choose a different PIN.";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "pinLength");
                    element.InnerText = "Your PIN must be a minimum of 4 digits.";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "pinMismatch");
                    element.InnerText = "New PIN and Confirm PIN must match.";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "pinRetry");
                    element.InnerText = "Incorrect PIN. Please try again.";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "pinSequentialDigits");
                    element.InnerText = "Your PIN cannot contain 3 or more sequential digits ascending or descending.";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "pinSubsetOfPhone");
                    element.InnerText = "Your PIN cannot contain a 4 digit subset of your phone number or backup phone number.";
                    phone_app_messages.AppendChild(element);
                    element = doc.CreateElement("message");
                    element.SetAttribute("type", "saveButton");
                    element.InnerText = "Save";
                    phone_app_messages.AppendChild(element);
                }
                phone_app_auth_info.AppendChild(phone_app_messages);
                auth_request.AppendChild(phone_app_auth_info);
            }
            else
            {
                pin_info.SetAttribute("pinMode", pfAuthParams.Mode);
                if (pfAuthParams.Mode == MODE_PIN) pin_info.AppendChild(pin_element);
                element = doc.CreateElement("userCanChangePin");
                element.InnerText = "no";
                pin_info.AppendChild(element);
            }
            auth_request.AppendChild(pin_info);

            return doc.InnerXml;
        }

        // 
        // create_voiceprint_reset_message: generates a voiceprint reset message to be sent
        // 	to the PhoneFactor backend.
        //  
        // Return value:
        //     a complete voiceprint reset xml message ready to be sent to the PhoneFactor backend
        // 
        private static string create_voiceprint_reset_message(
          ResetVoiceprintParams resetVoiceprintParams)
        {
            XmlDocument doc = new XmlDocument();

            // start message
            // <pfpMessage></pfpMessage>
            XmlElement root = doc.CreateElement("pfpMessage");
            root.SetAttribute("version", "1.5");
            doc.AppendChild(root);

            // message header
            // <header></header>
            XmlElement header = doc.CreateElement("header");
            root.AppendChild(header);
            XmlElement element = doc.CreateElement("source");
            header.AppendChild(element);
            element = doc.CreateElement("component");
            element.SetAttribute("type", "pfsdk");
            header.AppendChild(element);
            element = doc.CreateElement("host");
            element.SetAttribute("ip", resetVoiceprintParams.IpAddress);
            element.SetAttribute("hostname", resetVoiceprintParams.Hostname);
            header.AppendChild(element);

            // request
            // <request></request>
            XmlElement request = doc.CreateElement("request");
            Random random = new Random();
            string request_id = random.Next(10000).ToString();
            request.SetAttribute("request-id", request_id);
            request.SetAttribute("async", "0");
            request.SetAttribute("response-url", "");
            root.AppendChild(request);
            XmlElement pin_reset_request = doc.CreateElement("setPinResetRequest");
            request.AppendChild(pin_reset_request);
            XmlElement customer = doc.CreateElement("customer");
            pin_reset_request.AppendChild(customer);
            element = doc.CreateElement("licenseKey");
            element.InnerText = LICENSE_KEY;
            customer.AppendChild(element);
            element = doc.CreateElement("groupKey");
            element.InnerText = GROUP_KEY;
            customer.AppendChild(element);
            element = doc.CreateElement("username");
            element.InnerText = resetVoiceprintParams.Username;
            pin_reset_request.AppendChild(element);

            return doc.InnerXml;
        }

        // 
        // create_validate_device_token_message: generates a validate device token message to be sent
        // 	to the PhoneFactor backend.
        //  
        // Return value:
        //     a complete validate device token xml message ready to be sent to the PhoneFactor backend
        // 
        private static string create_validate_device_token_message(
          ValidateDeviceTokenParams validateDeviceTokenParams)
        {
            XmlDocument doc = new XmlDocument();

            // start message
            // <pfpMessage></pfpMessage>
            XmlElement root = doc.CreateElement("pfpMessage");
            root.SetAttribute("version", "1.5");
            doc.AppendChild(root);

            // message header
            // <header></header>
            XmlElement header = doc.CreateElement("header");
            root.AppendChild(header);
            XmlElement element = doc.CreateElement("source");
            header.AppendChild(element);
            element = doc.CreateElement("component");
            element.SetAttribute("type", "pfsdk");
            header.AppendChild(element);
            element = doc.CreateElement("host");
            element.SetAttribute("ip", validateDeviceTokenParams.IpAddress);
            element.SetAttribute("hostname", validateDeviceTokenParams.Hostname);
            header.AppendChild(element);

            // request
            // <request></request>
            XmlElement request = doc.CreateElement("request");
            Random random = new Random();
            string request_id = random.Next(10000).ToString();
            request.SetAttribute("request-id", request_id);
            request.SetAttribute("async", "0");
            request.SetAttribute("response-url", "");
            root.AppendChild(request);
            XmlElement validate_device_token_request = doc.CreateElement("validateDeviceTokenRequest");
            request.AppendChild(validate_device_token_request);
            XmlElement customer = doc.CreateElement("customer");
            validate_device_token_request.AppendChild(customer);
            element = doc.CreateElement("licenseKey");
            element.InnerText = LICENSE_KEY;
            customer.AppendChild(element);
            element = doc.CreateElement("groupKey");
            element.InnerText = GROUP_KEY;
            customer.AppendChild(element);
            element = doc.CreateElement("username");
            element.InnerText = validateDeviceTokenParams.Username;
            validate_device_token_request.AppendChild(element);
            element = doc.CreateElement("deviceToken");
            if (validateDeviceTokenParams.NotificationType.Length > 0)
            {
                element.SetAttribute("notificationType", validateDeviceTokenParams.NotificationType);
            }
            element.InnerText = validateDeviceTokenParams.DeviceToken;
            validate_device_token_request.AppendChild(element);
            element = doc.CreateElement("phoneAppAccountName");
            element.InnerText = validateDeviceTokenParams.AccountName;
            validate_device_token_request.AppendChild(element);

            return doc.InnerXml;
        }

        // 
        // send_message: sends a message to the PhoneFactor backend
        // 
        // Arguments:
        //     1) target: the target URL to send the message to
        //     2) message: the message to be sent
        //     3) cert_file_path: the path and file name of the certificate file
        //     4) body: the response text from the PhoneFactor backend.  This will
        //        likely be an XML message ready to be parsed.
        // 
        // Return value:
        //     True if the request was successfully sent and a response was received.
        //     False if the request failed.
        // 
        private static bool send_message(
          string target,
          string message,
          string cert_file_path,
          out string body)
        {
            body = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(target);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            // Set certificate
            X509Certificate cert = new X509Certificate2(cert_file_path, CERT_PASSWORD, X509KeyStorageFlags.MachineKeySet);
            request.ClientCertificates.Add(cert);
            request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequired;

            try
            {
                // Set Message
                byte[] postBytes = Encoding.UTF8.GetBytes(message);
                request.ContentType = "text/xml; charset=utf-8";
                request.ContentLength = postBytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                // Post message and read response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream);
                body = streamReader.ReadToEnd();
                streamReader.Close();
                stream.Close();
                response.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // 
        // get_response_status: parses the response from the PhoneFactor backend
        // 
        // Arguments:
        //     1) xml: the XML response string to be parsed
        //     2) request_id: a string returned that can be used to match the response
        //        to the original request
        //     3) otp: one-time-passcode returned for one-way SMS.
        //     4) call_status: an integer code returned representing the status of the
        //        phonecall.
        //     5) error_id: an integer code returned if the connection to the PhoneFactor
        //        backend failed.
        // 
        // Return value:
        //     A boolean value representing whether the auth was successful or not.
        //     If true, then the call_status and error_id output arguments can safely be
        //     ignored.
        // 
        public static bool get_response_status(
          string xml,
          out string request_id,
          out string otp,
          out int call_status,
          out int error_id)
        {
            request_id = "";
            otp = "";
            call_status = 0;
            error_id = 0;

            if (xml.Length == 0) return false;

            TextReader textReader = new StringReader(xml);
            XmlDocument doc = new XmlDocument();
            doc.Load(textReader);

            XmlNode response = doc.GetElementsByTagName("response")[0];
            request_id = response.Attributes["request-id"].Value;

            XmlNode status = doc.GetElementsByTagName("status")[0];
            if (status.Attributes["disposition"].Value == "fail")
            {
                error_id = Convert.ToInt32(status["error-id"].InnerText);
                return false;
            }

            XmlNode otpNode = doc.GetElementsByTagName("otp")[0];
            if (otpNode != null)
            {
                otp = otpNode.InnerText;
            }

            XmlNode auth_response = doc.GetElementsByTagName("authenticationResponse")[0];
            call_status = Convert.ToInt32(auth_response["callStatus"].InnerText);

            if (auth_response["authenticated"].InnerText == "no")
            {
                return false;
            }

            return true;
        }

        // Deprecated - for backward compatibility only
        public static bool get_response_status(
          string xml,
          out string request_id,
          out int call_status,
          out int error_id)
        {
            string otp = "";
            return get_response_status(xml, out request_id, out otp, out call_status, out error_id);
        }

        public static bool get_response_status(
          string xml,
          out int call_status,
          out int error_id)
        {
            string request_id = "";
            return get_response_status(xml, out request_id, out call_status, out error_id);
        }

        // 
        // get_voiceprint_reset_response_status: parses the response from the PhoneFactor backend
        // 
        // Arguments:
        //     1) xml: the XML response string to be parsed
        //     2) result: an integer code returned representing the status of the
        //        voiceprint reset.
        //     3) error_id: an integer code returned if the connection to the PhoneFactor
        //        backend failed.
        // 
        // Return value:
        //     A boolean value representing whether the voiceprint reset was successful or not.
        //     If true, then the result and error_id output arguments can safely be
        //     ignored.
        // 
        public static bool get_voiceprint_reset_response_status(
          string xml,
          out int result,
          out int error_id)
        {
            result = 0;
            error_id = 0;

            if (xml.Length == 0) return false;

            TextReader textReader = new StringReader(xml);
            XmlDocument doc = new XmlDocument();
            doc.Load(textReader);

            XmlNode response = doc.GetElementsByTagName("response")[0];

            XmlNode status = doc.GetElementsByTagName("status")[0];
            if (status.Attributes["disposition"].Value == "fail")
            {
                error_id = Convert.ToInt32(status["error-id"].InnerText);
                return false;
            }

            XmlNode pin_reset_response = doc.GetElementsByTagName("setPinResetResponse")[0];
            result = Convert.ToInt32(pin_reset_response["result"].InnerText);

            if (pin_reset_response["result"].InnerText != "1")
            {
                return false;
            }

            return true;
        }

        // 
        // get_validate_device_token_response_status: parses the response from the PhoneFactor backend
        // 
        // Arguments:
        //     1) xml: the XML response string to be parsed
        //     2) result: an integer code returned representing the status of the
        //        device token validation.
        //     3) error_id: an integer code returned if the connection to the PhoneFactor
        //        backend failed.
        // 
        // Return value:
        //     A boolean value representing whether the device token validation was
        //     successful or not.  If true, then the result and error_id output arguments
        //     can safely be ignored.
        // 
        public static bool get_validate_device_token_response_status(
          string xml,
          out int result,
          out int error_id)
        {
            result = 0;
            error_id = 0;

            if (xml.Length == 0) return false;

            TextReader textReader = new StringReader(xml);
            XmlDocument doc = new XmlDocument();
            doc.Load(textReader);

            XmlNode response = doc.GetElementsByTagName("response")[0];

            XmlNode status = doc.GetElementsByTagName("status")[0];
            if (status != null && status.Attributes["disposition"].Value == "fail")
            {
                error_id = Convert.ToInt32(status["error-id"].InnerText);
                return false;
            }

            XmlNode validate_device_token_response = doc.GetElementsByTagName("validateDeviceTokenResponse")[0];
            result = Convert.ToInt32(validate_device_token_response["validationResult"].InnerText);

            if (validate_device_token_response["validationResult"].InnerText != "1")
            {
                return false;
            }

            return true;
        }
    }
}
