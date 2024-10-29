
namespace OceansApp.Utility.NotificationTemplates
{
    public class EmailTemplates
    {
        public string EmailTemplate(string title, string body)
        {
            var template = @"<!DOCTYPE html>
<html>
<head>
</head>
<body>
    <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
        <tr>
            <td align=""left"">
                <table cellspacing=""0"" cellpadding=""0"" width=""500"" style=""background-color: #fff;"">
                    <tr>
                        <td align=""center"">
                            <table cellspacing=""0"" cellpadding=""0"" width=""100%"" style=""background-color: #e7eaef; padding: 10px;"">
                                <tr>
                                    <td align=""center"">
                                        <img src=""https://oceansapp.azurewebsites.net/img/logo-color.png"" alt=""Oceans Code Experts"" width=""150"" style=""display: block; margin: auto;"" />
                                    </td>
                                </tr>
                            </table>
                            <table cellspacing=""0"" cellpadding=""0"" width=""500"" style=""background-color: #fff; margin: 0 auto; border-bottom-left-radius: 10px; border-bottom-right-radius: 10px; position: relative; padding: 0 30px 30px 30px"">
                                <tr>
                                    <td>
                                        <div>
                                            <div>
                                                <table cellspacing=""0"" cellpadding=""0"" width=""100%"" style=""font-family: 'nexa', Arial, sans-serif;"">
                                                    <tr>
                                                        <td>
                                                            <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                <tr>
                                                                    <td>
                                                                        <h2 style=""text-align: center; margin-bottom: 0; margin-top: 25px; color: #058993;"">
                                                                            " + title + @"</h2>
                                                                        <div style=""background-color: #baf7f4; width: 100%; height: 1px;margin-top: 20px;""></div>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                            " + body + @"
                                                            <p style=""margin-bottom: 2px; color: #058993;""><strong>Thank you!</strong></p>
                                                            <p style=""margin: 0;"">Oceans Code Experts</p>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                            <table cellspacing=""0"" cellpadding=""0"" width=""100%"" style=""background-color: #e7eaef; padding: 10px;"">
                                <tr>
                                    <td>
                                        <table cellspacing=""0"" cellpadding=""0"" width=""500"" style=""margin: 0 auto; border-bottom-left-radius: 10px; border-bottom-right-radius: 10px; position: relative; padding: 10px"">
                                            <tr>
                                                <td align=""center"">
                                                    <p style=""margin: 0;""><strong>This is an automatically generated email.</strong></p>
                                                    <p style=""margin: 0;""><strong>Please do not reply to it.</strong></p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            return template;
        }
        public string ForgotPasswordBody(string url, string userName)
        {
            return @"<div style=""text-align: center;"">
                        <p> Hello " + userName + @",</p>
                            <p> It seems that you have forgotten the
                           password for your Ripple by Oceans App account.
                       </p>
                       <p> Don't worry, you can set a new one here.
                       </p>
                   </div>
                   <table cellspacing=""0"" cellpadding=""0""
                       width=""100%"">
                       <tr>
                           <td align=""center"">
                               <div
                                   style=""margin-top: 20px; margin-bottom: 20px;"">
                                   <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""border-collapse: separate;"">
                             <tr>
       <td style=""border-radius: 25px; background-color: #5bbb71; padding: 10px 40px;"" align=""center"">
          <a href=""" + url + @""" style=""font-family: 'Montserrat', sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; cursor: pointer; display: inline-block;"">
               Change Password
             </a>
           </td>
         </tr>
       </table>
                                      </div>
                               <div
                                   style=""background-color: #baf7f4; width: 100%; height: 1px;margin-top: 20px;"">
                               </div>
                           </td>
                       </tr>
                   </table>
                   <p>
                       If you didn't request to change your
                       password you don't have to do anything.
                       Ignore this email, your old password will be
                       kept.
                   </p>";
        }
        public string CreatePasswordBody(string url, string userName)
        {
            return @"<div style=""text-align: center;"">
                        <p> Hello " + userName + @",</p>
                       <p>Oceans Code Experts is inviting you to be part of the Team.
                       </p>
                       <p> Please click on the button below to create your password and join the Team.
                       </p>
                   </div>
                   <table cellspacing=""0"" cellpadding=""0""
                       width=""100%"">
                       <tr>
                           <td align=""center"">
                               <div
                                   style=""margin-top: 20px; margin-bottom: 20px;"">
                                   <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""border-collapse: separate;"">
                             <tr>
       <td style=""border-radius: 25px; background-color: #5bbb71; padding: 10px 40px;"" align=""center"">
          <a href=""" + url + @""" style=""font-family: 'Montserrat', sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; cursor: pointer; display: inline-block;"">
               Create Password
             </a>
           </td>
         </tr>
       </table>
                                      </div>
                               <div
                                   style=""background-color: #baf7f4; width: 100%; height: 1px;margin-top: 20px;"">
                               </div>
                           </td>
                       </tr>
                   </table>
                   <p>
                       We are delighted to extend a warm welcome to you!
                   </p>";
        }
        public string SubmissionReminderBody(string url, string userName, string period, List<string> projectsList)
        {
            string projectsSection = @"<div style=""text-align: left;""><ul>";
            foreach (var project in projectsList)
            {
                projectsSection += $"<li><strong>{project}</strong></li>";
            }
            projectsSection += "</ul><div>";
            return @"<div style=""text-align: center;"">
                        <p> Hello " + userName + @",</p>
                        <p> Ready for your payment? </p>
                        <p> Oceans needs you to send your worked hours report to make your payment for period: <strong>" + period + @"</strong></p>
                        <div style=""text-align: left;"">
                            <p>See below the projects pending your hours submission:</p>
                        </div>
                        " + projectsSection + @"
                   </div>
                   <table cellspacing=""0"" cellpadding=""0""
                       width=""100%"">
                       <tr>
                           <td align=""center"">
                               <div
                                   style=""margin-top: 20px; margin-bottom: 20px;"">
                                   <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""border-collapse: separate;"">
                             <tr>
                        <td style=""border-radius: 25px; background-color: #5bbb71; padding: 10px 40px;"" align=""center"">
                           <a href=""" + url + @""" style=""font-family: 'Montserrat', sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; cursor: pointer; display: inline-block;"">
                                Go to submit my hours
                              </a>
                            </td>
                          </tr>
                        </table>
                                      </div>
                               <div
                                   style=""background-color: #baf7f4; width: 100%; height: 1px;margin-top: 20px;"">
                               </div>
                           </td>
                       </tr>
                   </table>
                   <p>
                       Feel free to contact your Success Manager or the Financial Team if you have any questions.
                   </p>";
        }
        public string SubmissionHoursNotificationBody(string url, string consultantName, string period, string projectName)
        {

            return @"<div style=""text-align: center;"">
                        <p> Hello Oceans Finance Team,</p>
                        <p><strong>" + consultantName + @"</strong> has submitted their time report for the <strong>'" + projectName + @"'</strong> project for the period of <strong>" + period + @"</strong></p>
                        <p>You can go to the ""Payment Sheets"" section to review it and approve or reject it.</p>
                   </div>
                   <table cellspacing=""0"" cellpadding=""0""
                       width=""100%"">
                       <tr>
                           <td align=""center"">
                               <div
                                   style=""margin-top: 20px; margin-bottom: 20px;"">
                                   <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""border-collapse: separate;"">
                             <tr>
                        <td style=""border-radius: 25px; background-color: #5bbb71; padding: 10px 40px;"" align=""center"">
                           <a href=""" + url + @""" style=""font-family: 'Montserrat', sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; cursor: pointer; display: inline-block;"">
                                Go to Approve/Reject submission
                              </a>
                            </td>
                          </tr>
                        </table>
                                      </div>
                               <div
                                   style=""background-color: #baf7f4; width: 100%; height: 1px;margin-top: 20px;"">
                               </div>
                           </td>
                       </tr>
                   </table>";
        }
        public string ApprovedRejectedSubmissionBody(string url, string consultantName, string period, string projectName, string status,
    string userActionedBy, string? rejectedComment)
        {
            string statusColor = status == "Approved" ? "#5bbb71" : "#d70b00";

            string rejectedMessage = $@"
      <p>Please see more details about why your time submission was rejected.</p>
       <label><strong>Comment:</strong></label>
     <table role=""presentation"" width=""100%"" style=""background-color:#e8b4b4; text-align:left; border-collapse:collapse;"">
     <tr>
      <td style=""padding:6px"">
       <p>" + rejectedComment + @"</p>
      </td>
     </tr>
     </table>

      <div style=""text-align:left;margin-top:4px;"">
        <label><strong>Rejected by: </strong>" + userActionedBy + @"</label>
      </div>";

            string approvedMessage = $@"
     <table role=""presentation"" width=""100%"" style=""background-color:#5bbb71; color:#f5fcf5; font-weight:bold; text-align:center; border-collapse:collapse;"">
     <tr>
      <td style=""padding:6px"">
       <p>Thanks for your submission!</p>
      </td>
     </tr>
     </table>";

            string submissionStatus = $@"<span style=""color:{statusColor}""><strong>{status}</strong></span>";

            string body = $@"
        <div style=""text-align: center;"">
            <p>Hello {consultantName},</p>
            <p>Your time submission for the <strong>'{projectName}'</strong> project in the period <strong>{period}</strong> was {submissionStatus}.</p>
            {(status == "Approved" ? approvedMessage : rejectedMessage)}
        </div>";

            if (status != "Approved")
            {
                body += $@"
           <table cellspacing=""0"" cellpadding=""0"" width=""100%"">
               <tr>
                   <td align=""center"">
                       <div style=""margin-top: 20px; margin-bottom: 20px;"">
                           <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""border-collapse: separate;"">
                               <tr>
                                   <td style=""border-radius: 25px; background-color: #5bbb71; padding: 10px 40px;"" align=""center"">
                                       <a href=""{url}"" style=""font-family: 'Montserrat', sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; cursor: pointer; display: inline-block;"">
                                           Review my timesheet to re-submit
                                       </a>
                                   </td>
                               </tr>
                           </table>
                       </div>
                   </td>
               </tr>
           </table>";
            }

            return body;
        }

    }
}
