using GameNotifcationService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Timers;
using Telegram.Bot;

namespace GameNotificationService
{
    public class ServiceMethods
    {
        private readonly Timer _timer;
        private static Dictionary<int, string> URLs { get; set; }
        private DateTime dateTime;
        private DateTime dateTime_1 = DateTime.Now.AddHours(Convert.ToDouble(ConfigurationManager.AppSettings["SystemMailTimeInHours"]));
        private DateTime dateTime_2 = DateTime.Now.AddHours(Convert.ToDouble(ConfigurationManager.AppSettings["GameMailTimeInHours"]));
        static TelegramBotClient telegramBotClient = new TelegramBotClient(Convert.ToString(ConfigurationManager.AppSettings["TelegramBotToken"]));

        public ServiceMethods()
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            _timer = new Timer(Convert.ToInt32(ConfigurationManager.AppSettings["Timer"]))
            { AutoReset = true };
            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            dateTime = DateTime.Now;
            if (dateTime > dateTime_1)
            {
                dateTime_1 = dateTime.AddHours(Convert.ToDouble(ConfigurationManager.AppSettings["SystemMailTimeInHours"]));
                Utility(dateTime_1);
            }
            if (dateTime > dateTime_2)
            {
                dateTime_2 = dateTime.AddHours(Convert.ToDouble(ConfigurationManager.AppSettings["GameMailTimeInHours"]));
                APIInitiator(dateTime_2);
            }
        }

        public void Start()
        {
            telegramBotClient.SendTextMessageAsync(1715334607, Constant.ServiceStartMessage);
            _timer.Start();
        }

        public void Stop()
        {
            telegramBotClient.SendTextMessageAsync(1715334607, Constant.ServiceStopMessage);
            _timer.Stop();
        }

        public async static void Utility(DateTime nextUpdate)
        {
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                List<string> list = new List<string>();
                #region Battery Info
                System.Windows.Forms.PowerStatus pwr = System.Windows.Forms.SystemInformation.PowerStatus;
                list.Add("Please find the below system details.");
                list.Add("Battery Charge Status: " + pwr.BatteryChargeStatus.ToString());
                list.Add("Battery Life Percent: " + (int)(pwr.BatteryLifePercent * 100) + "%");
                list.Add("Battery Life Remaining: " + pwr.BatteryLifeRemaining.ToString());
                list.Add("Power Line Status: " + pwr.PowerLineStatus.ToString());
                #endregion

                #region Internet Info
                System.Net.WebClient wc = new System.Net.WebClient();
                DateTime dt1 = DateTime.Now;
                byte[] data = wc.DownloadData("http://google.com");
                DateTime dt2 = DateTime.Now;

                list.Add("Internet Status: " + Math.Round((data.Length / 1024) / (dt2 - dt1).TotalSeconds, 2) + "Kb/Sec");
                if (Dns.GetHostName().ToString().ToLower() == "batman")
                {
                    list.Add("Host Name: Device I - " + Dns.GetHostName().ToString());
                }
                else
                {
                    list.Add("Host Name: Device II - " + Dns.GetHostName().ToString());
                }
                //list.Add("Host Address: " + Dns.GetHostAddresses(Dns.GetHostName()).GetValue(0).ToString());
                #endregion

                PerformanceCounter cpuCounter;
                PerformanceCounter ramCounter;
                PerformanceCounter diskCounter;

                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
                ramCounter.NextValue(); ramCounter.NextValue();
                cpuCounter.NextValue(); cpuCounter.NextValue();
                diskCounter.NextValue(); diskCounter.NextValue();
                list.Add("CPU Usage: " + cpuCounter.NextValue() + "%");
                list.Add("RAM Usage: " + ramCounter.NextValue() + " MB");
                list.Add("DISK Usage: " + diskCounter.NextValue() + "%");
                list.Add("***** Game Notification Service is running *****");
                list.Add("Next Utility update at: " + nextUpdate.ToString(Constant.DateTimeFormat));
                //SendMail(list.ToArray(), "System");
                await telegramBotClient.SendTextMessageAsync(1715334607, StringBuilder(list.ToArray()));
            }
            catch(Exception ex)
            {
                #region Exception
                LogWrite("Class name: ServiceMethods, Method name: Utility");
                LogWrite("Message:  " + ex?.Message?.ToString());
                LogWrite("StackTrace:  " + ex?.StackTrace?.ToString());
                LogWrite("InnerException.Message:  " + Convert.ToString(ex?.InnerException?.Message));
                LogWrite("InnerException.StackTrace:  " + Convert.ToString(ex?.InnerException?.StackTrace));
                List<string> list = new List<string>();
                list.Add("Class name: ServiceMethods, Method name: TelegramBotGameAPIInitiator");
                list.Add("Message:  " + ex?.Message?.ToString());
                list.Add("StackTrace:  " + ex?.StackTrace?.ToString());
                list.Add("InnerException.Message:  " + Convert.ToString(ex?.InnerException?.Message));
                list.Add("InnerException.StackTrace:  " + Convert.ToString(ex?.InnerException?.StackTrace));
                await telegramBotClient.SendTextMessageAsync(1715334607, exceptionStringBuilder(list.ToArray()));
                #endregion
            }
        }

        public async static void APIInitiator(DateTime? nextUpdate = null)
        {
            try
            {
                string file = Convert.ToString(ConfigurationManager.AppSettings["GameNames"]); //-- Important file, without this games will not be triggered.
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                URLs = new Dictionary<int, string>();
                string urlParameter = "https://store-site-backend-static.ak.epicgames.com/freeGamesPromotions?locale=en-US&country=IN&allowCountries=IN";
                var task = client.GetAsync(urlParameter);
                task.Wait();
                var response = task.Result;
                List<List<string>> finalList = new List<List<string>>();
                string addFileEntry = string.Empty;
                if (response.IsSuccessStatusCode)
                {
                    var readTask = response.Content.ReadAsStringAsync();
                    readTask.Wait();
                    var result = JsonConvert.DeserializeObject<EpicApiModel>(readTask.Result).Data.Catalog.SearchStore;
                    foreach (var i in result.Elements)
                    {
                        List<string> list = new List<string>();
                        if (i.Promotions != null)
                        {
                            foreach (var i_2 in i.Promotions?.PromotionalOffers)
                            {
                                list.Add("Please find the below details for free game:");
                                list.Add("Game Name: " + i.Title);
                                addFileEntry = i.Title;
                                list.Add("Game Description: " + i.Description);
                                foreach (var i_3 in i_2.PromotionalOffers)
                                {
                                    list.Add("Start Date: " + i_3.StartDate?.LocalDateTime.ToString(Constant.DateTimeFormat));
                                    list.Add("End Date: " + i_3.EndDate?.LocalDateTime.ToString(Constant.DateTimeFormat));
                                }
                                list.Add("URL: " + "https://www.epicgames.com/store/en-US/");
                            }
                        }
                        else
                        {
                            LogWrite(i.Title + " has no Promotions object");
                            await telegramBotClient.SendTextMessageAsync(1715334607, i.Title + " has no Promotions object");
                        }
                        if (list.Count > 0 && (i.Promotions.PromotionalOffers.Length > 0 || i.Promotions.UpcomingPromotionalOffers.Length > 0))
                        {
                            if (File.Exists(file))
                            {
                                string[] gn = File.ReadAllLines(file);
                                if (gn.Any(a => String.Equals(a, addFileEntry, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    //Do anything
                                    LogWrite("Game: '" + addFileEntry + "' exist in Game Names File. " + "Date: " + DateTime.Now.ToString(Constant.DateTimeFormat));
                                    await telegramBotClient.SendTextMessageAsync(1715334607, "Game: '"+ addFileEntry + "' exist in Game Names File. " + "Date: " + DateTime.Now.ToString(Constant.DateTimeFormat));
                                }
                                else
                                {
                                    finalList.Add(list);
                                    using (StreamWriter w = File.AppendText(file))
                                    {
                                        w.WriteLine(addFileEntry);
                                    }
                                }
                            }
                        }
                    }
                }
                if (finalList.Count > 0)
                {
                    SendMail(null, null, finalList.ToList());
                }
                else
                {
                    LogWrite("No new game...");
                    await telegramBotClient.SendTextMessageAsync(1715334607, "No new game... " + "Date: " + DateTime.Now.ToString(Constant.DateTimeFormat));
                }
                await telegramBotClient.SendTextMessageAsync(1715334607, "Next API update at: " + nextUpdate?.ToString(Constant.DateTimeFormat));
            }
            catch (Exception ex)
            {
                #region Exception
                LogWrite("Class name: ServiceMethods, Method name: APIInitiator");
                LogWrite("Message:  " + ex?.Message?.ToString());
                LogWrite("StackTrace:  " + ex?.StackTrace?.ToString());
                LogWrite("InnerException.Message:  " + Convert.ToString(ex?.InnerException?.Message));
                LogWrite("InnerException.StackTrace:  " + Convert.ToString(ex?.InnerException?.StackTrace));
                #endregion

                #region Exception
                List<string> list = new List<string>();
                list.Add("Service: GameNotoficationService, Class name: ServiceMethods, Method name: APIInitiator");
                list.Add("Message:  " + ex?.Message?.ToString());
                list.Add("StackTrace:  " + ex?.StackTrace?.ToString());
                list.Add("InnerException.Message:  " + Convert.ToString(ex?.InnerException?.Message));
                list.Add("InnerException.StackTrace:  " + Convert.ToString(ex?.InnerException?.StackTrace));
                await telegramBotClient.SendTextMessageAsync(1715334607, exceptionStringBuilder(list.ToArray()));
                #endregion
            }
        }

        public static bool SendMail(String[] str = null, string emailType = "", List<List<string>> listString = null)
        {
            bool isSendSuccess = false;
            try
            {
                string fromaddr = Convert.ToString(ConfigurationManager.AppSettings["EmailFrom"]);
                string _GameNotificationTo = Convert.ToString(ConfigurationManager.AppSettings["GameNotificationTo"]);
                string password = Convert.ToString(ConfigurationManager.AppSettings["EmailPassword"]);
                string _SystemEmailTo = Convert.ToString(ConfigurationManager.AppSettings["SystemEmailTo"]);

                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(fromaddr);

                msg.Subject = "Free game alert at: " + System.DateTime.Now.ToString();
                msg.Body = emailStringBuilder(null, listString);
                foreach (var address in _GameNotificationTo.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    LogWrite("******************* Email sent to: " + address);
                    telegramBotClient.SendTextMessageAsync(1715334607, "Game Email sent to: " + address + ", Date: " + DateTime.Now.ToString(Constant.DateTimeFormat));
                    msg.Bcc.Add(address);
                }
                emailType = string.Empty;

                //msg.To.Add(address);
                //msg.IsBodyHtml = true;
                //msg.To.Add(new MailAddress(toaddr));
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                NetworkCredential nc = new NetworkCredential(fromaddr, password);
                smtp.Credentials = nc;
                if (msg.Bcc.Count > 0)
                    smtp.Send(msg);
                isSendSuccess = true;
                SystemSounds.Beep.Play();
            }
            catch (Exception ex)
            {
                LogWrite("Class name: ServiceMethods, Method name: SendMail");
                LogWrite("Email Exception Message: " + ex.Message);
                LogWrite("Email Exception StackTrace: " + ex.StackTrace);
                isSendSuccess = false;
            }

            return isSendSuccess;
        }

        #region String Builders
        public static string emailStringBuilder(String[] str = null, List<List<string>> listString = null)
        {
            StringBuilder errMsg = new StringBuilder();
            errMsg.AppendLine();
            errMsg.AppendLine("Hi Gamer,");
            errMsg.AppendLine();
            foreach (var l in listString)
            {
                foreach (var i in l)
                {
                    errMsg.AppendLine(i);
                }
                errMsg.AppendLine();
                errMsg.AppendLine();
            }
            errMsg.AppendLine("Regards,");
            errMsg.AppendLine("EpicGame Bot");
            return errMsg.ToString();
        }

        public static string StringBuilder(String[] str)
        {
            StringBuilder errMsg = new StringBuilder();
            errMsg.AppendLine();
            errMsg.AppendLine();
            foreach (var i in str)
            {
                errMsg.AppendLine(i);
            }
            errMsg.AppendLine();
            return errMsg.ToString();
        }

        public static string exceptionStringBuilder(String[] str = null)
        {
            StringBuilder errMsg = new StringBuilder();
            errMsg.AppendLine();
            errMsg.AppendLine("Exception occured at: " + DateTime.Now);
            errMsg.AppendLine();
            foreach (var l in str)
            {
                errMsg.AppendLine(l);
                errMsg.AppendLine();
            }
            errMsg.AppendLine();
            return errMsg.ToString();
        }
        #endregion

        #region Log Writing
        public static void LogWrite(string logMessage)
        {
            if (Convert.ToInt32(ConfigurationManager.AppSettings["isLogging"]) == 1)
            {
                string m_exePath = @"C:\Services\Game";
                try
                {
                    using (StreamWriter w = File.AppendText(m_exePath + "\\" + "GameNotificationService_" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt"))
                    {
                        Log(logMessage, w);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        public static void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\n" + logMessage + " Date: " + DateTime.Now.ToString(Constant.DateTimeFormat));
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }
}