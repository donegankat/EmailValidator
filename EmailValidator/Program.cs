using EmailValidator.Models;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace EmailValidator
{
    class Program
    {
        private static string _currentDirectory = Directory.GetCurrentDirectory();
        private static string _rootProjectDirectory = _currentDirectory.Remove(_currentDirectory.IndexOf("\\bin"));
        private static string _sourceFolder = "SourcesToCheck";

        private static string _mailgunApiKey = "YOUR_PUBLIC_API_KEY_HERE";

        public static void Main(string[] args)
        {
            var emailList = _loadFile("EmailList.txt"); // Prompt for this or read from a config file if necessary someday. Right now we just need quick and dirty hardcoding.            
            var splitList = emailList.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            // Clean up our email list.
            for (int i = 0; i < splitList.Count(); i++)
            {
                splitList[i] = splitList[i].Trim(); // We already split on \n but the \r's are still hanging around. Trim them off.
            }

            // Validate the list of emails.
            var badEmails = new List<EmailResult>();
            foreach (var email in splitList)
            {
                var results = _validate(email);
                if (!results.Item1 || !results.Item2)
                {
                    badEmails.Add(new EmailResult(email, results.Item1, results.Item2));                    
                }
            }

            // Print the validation results.
            if (badEmails.Count > 0)
            {
                Console.WriteLine($"Bad emails found:");
                foreach (var email in badEmails)
                {
                    Console.WriteLine($"{email.EmailAddress} - Regex: {email.IsValidRegex} SMTP: {email.IsValidSmtp}");
                    Console.WriteLine();
                    Console.WriteLine();
                }

                Console.WriteLine($"SQL:");

                // I'm cheating here and basically making my bad email list SQL-friendly so I can just copy/paste out of the console window.
                foreach (var email in badEmails)
                {
                    Console.WriteLine($"'{email.EmailAddress}',");
                }
            }
            else
            {
                Console.WriteLine("No bad emails found");
            }
            
            Console.ReadKey(); // Wait for user input so we don't automatically quit out and lose everything we just did.
        }

        private static Tuple<bool, bool> _validate(string emailAddress)
        {
            var emailRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
            
            // Assume both validation checks will pass.
            var isValidRegex = true;
            var isValidSmtp = true;

            // Validate the email via regex.
            try
            {
                isValidRegex = Regex.IsMatch(emailAddress, emailRegex);
            }
            catch (Exception ex)
            {
                isValidRegex = false;
            }

            // Validate the email via SMTP.
            try
            {
                MailAddress addr = new MailAddress(emailAddress); // Problems will be caught in the catch block.
            }
            catch (Exception ex) // These was a problem with the email address
            {
                isValidSmtp = false;
            }

            return new Tuple<bool, bool>(isValidRegex, isValidSmtp);
        }

        #region Mailgun Validation
        /// <summary>
        /// Controls which emails we send to the Mailgun API for validation.
        /// Currently I don't know the cost structure for using this so it's not implemented.
        /// Documentation: https://documentation.mailgun.com/en/latest/api-email-validation.html#example
        /// </summary>
        /// <param name="emailList"></param>
        private static void _validateMailgun(List<string> emailList)
        {
            var lastIndex = 0;
            var takeAmount = 50;

            while (emailList.Skip(lastIndex).Take(takeAmount).Count() > 0)
            {
                var currentBatch = emailList.Skip(lastIndex).Take(takeAmount); // Get a group of emails to validate all at once. Otherwise this takes forever when we do it 1 by 1.

                var response = _getParse(String.Join(',', currentBatch)).Content.ToString();

                if (!response.Contains("\"unparseable\": []")) // Only bother printing the results if there was a problem.
                {
                    Console.WriteLine();
                    Console.WriteLine($"Bad email found: {response}");
                }

                lastIndex += takeAmount;
            }
        }

        /// <summary>
        /// This method makes a call to the Mailgun email validation API.
        /// Currently I don't know the cost structure for using this so it's not implemented.
        /// Documentation: https://documentation.mailgun.com/en/latest/api-email-validation.html#example
        /// </summary>
        /// <param name="addressList"></param>
        /// <returns></returns>
        private static IRestResponse _getParse(string addressList)
        {
            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.mailgun.net/v3");
            client.Authenticator = new HttpBasicAuthenticator("api", _mailgunApiKey);

            RestRequest request = new RestRequest();
            request.Resource = "/address/parse"; // This is the endpoint for bulk parsing validation. A different endpoint is used for more detailed 1-by-1 address validation.
            request.AddParameter("addresses", addressList); // The "addresses" parameter is only used for bulk parsing validation.

            return client.Execute(request);
        }
        #endregion

        #region File Helpers
        private static string _loadFile(string fileName)
        {
            return File.ReadAllText($"{_rootProjectDirectory}/{_sourceFolder}/{fileName}", Encoding.UTF8);
        }

        private static void _saveFile(string fileName, string fileText)
        {
            File.WriteAllText($"{_rootProjectDirectory}/{_sourceFolder}/{fileName}", fileText);
        }
        #endregion
    }
}
