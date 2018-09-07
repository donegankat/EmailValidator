# EmailValidator
C# .NET Core console application that performs a simple regular expression, SMTP, and Mailgun API validation on a list of email addresses.

Email addresses can be provided by replacing the contents of [/SourcesToCheck/EmailList.txt](EmailValidator/SourcesToCheck/EmailList.txt) with a line-separated list of email addresses.

## Mailgun Notes
By default this program currently only performs the regular expression and SMTP validation checks. Some tweaking would be necessary to add the Mailgun API validation in but the fundamental framework is in place and worked fine in testing.

If you do want to add validation via the Mailgun API, you must provide your own public Mailgun API key in place of:

`private static string _mailgunApiKey = "YOUR_PUBLIC_API_KEY_HERE";`
