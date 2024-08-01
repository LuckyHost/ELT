using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ElectroTools
{
    internal class NetWork
    {
        private static string _pathFload = "Работа/ElectroTools.Admin/";


      
        public async Task<bool> showUpdateWindows()
        {
            bool isClosed = false;
            MessageBoxButtons buttons;
            NetWork.FileInfo dataYandexDisk = await getDataInfoinYa("Info.txt");
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;


            if (dataYandexDisk != null)
            {
                //Проверка на версию
                if (dataYandexDisk.CustomProperties.Version != version.ToString())
                {
                    //Запрет на запуск по имени
                    if (dataYandexDisk.CustomProperties.IsPersonStop.Trim() == "1")
                    {
                        string userName = Environment.UserName;

                        foreach (string item in dataYandexDisk.CustomProperties.PersonStop)
                        {
                            if(userName == item)
                            {
                                MessageBox.Show("Заблокированы");
                                return true;
                            }
                            
                        }
                    }
                   
                    //Вывод Сообщения
                    if (dataYandexDisk.CustomProperties.IsAddtext.Trim() == "1")
                    {
                        MessageBox.Show(dataYandexDisk.CustomProperties.TextAddText);
                    }

                    //Запрет на запуск
                    if (dataYandexDisk.CustomProperties.IsAutoCloseDoc.Trim() == "1")
                    {
                        return true;
                    }
                    //Запрет на необновлние 
                    if (dataYandexDisk.CustomProperties.IsActOldVersion.Trim() == "1")
                    {
                        buttons = MessageBoxButtons.OK;
                        isClosed = true;
                    }
                    else
                    {
                        //Позволяет не обновляться.
                        buttons = MessageBoxButtons.YesNo;
                    }
                    DialogResult result = MessageBox.Show("Вышла новая версия: " + dataYandexDisk.CustomProperties.Version + " Скачать обновление?", "Обновление.", buttons);
                    switch (result)
                    {
                        case DialogResult.Yes:
                            OpenUrlInDefaultBrowser(dataYandexDisk.CustomProperties.urlWebsite);
                            OpenUrlInDefaultBrowser(dataYandexDisk.CustomProperties.urlNewFile);
                            break;
                        case DialogResult.No:
                            break;

                        case DialogResult.OK:
                            OpenUrlInDefaultBrowser(dataYandexDisk.CustomProperties.urlWebsite);
                            OpenUrlInDefaultBrowser(dataYandexDisk.CustomProperties.urlNewFile);
                            break;

                        default:
                            break;
                    }
                    return isClosed;
                }
            }
            return isClosed;
        }


        public static async Task<FileInfo> getDataInfoinYa(string nameFile)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;
            try
            {

                //Проверка на доступ к Ya.ru
                if (IsInternetAvailable())
                {
                   
                    string accessToken = "y0_AgAAAAAC82upAADLWwAAAADlltdLOtUDPw_5RoqBUKqEbmjZZScvdtg";
                    //MessageBox.Show(accessToken.ToString());
              

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", accessToken);

                        // Запрос на получение списка файлов в корневой директории
                        string originalPath = _pathFload + nameFile;
                        HttpResponseMessage response = await httpClient.GetAsync("https://cloud-api.yandex.net/v1/disk/resources?path=" + Uri.EscapeDataString(originalPath));

                        if (response.IsSuccessStatusCode)
                        {
                            //Имя сохранение
                            string loadPath = _pathFload+"UserName/"+ Process.GetCurrentProcess().ProcessName+" "+Environment.UserName +" "+ DateTime.Now.ToString().Replace(':','.') + " .txt";
                            //Получаем ссылку
                            HttpResponseMessage responseName = await httpClient.GetAsync("https://cloud-api.yandex.net/v1/disk/resources/upload?path=" + loadPath);
                            if (responseName.IsSuccessStatusCode) 
                            {
                                //Получаем ссылку из Json
                                string url= ExtractLinkFromJson( await responseName.Content.ReadAsStringAsync());
                                //Загружаем
                                HttpResponseMessage loadFile = await httpClient.PutAsync(url, new StringContent("Версия модуля: "+ version.ToString())  );
                            }

                            string content = await response.Content.ReadAsStringAsync();
                            FileInfo YandexData = JsonConvert.DeserializeObject<FileInfo>(content);
                            
                            return YandexData;
                        }
                        else
                        {
                            MessageBox.Show("Что-то пошло не так....");
                            return null;
                        }
                    }
                }
                else { return null; }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
                // Обработка ошибок
                // ed.WriteMessage("Error updating MText: {0}\n", ex.Message);
                return null;
            }




        }
        static string ExtractLinkFromJson(string jsonString)
        {
            // Используем регулярное выражение для извлечения значения из ключа "href"
            Regex regex = new Regex("\"href\":\"(.*?)\"");
            Match match = regex.Match(jsonString);

            if (match.Success)
            {
                // Извлекаем значение из группы
                return match.Groups[1].Value;
            }
            else
            {
                return "Ссылка не найдена в JSON.";
            }
        }

        //Проверка интернета 
        static bool IsInternetAvailable()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    // Попытка скачать страницу с ya.ru (или любого другого ресурса)
                    string result = client.DownloadString("https://ya.ru");

                    // Если успешно, считаем, что есть доступ
                    return true;
                }
            }
            catch (WebException)
            {
                // Если возникла ошибка при попытке доступа, считаем, что нет доступа
                MessageBox.Show("Отсутствует подключение к интернету...");
                return false;
            }
        }

        //Для открытия URL
        static void OpenUrlInDefaultBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при открытии URL: {ex.Message}");
            }
        }


        //Классы
        public class CommentIds
        {
            public string PrivateResource { get; set; }
            public string PublicResource { get; set; }
        }

        public class CustomProperties
        {
            private string _serializePersonStop;
            [JsonProperty("isPersonStop")]
            public string IsPersonStop { get; set; }

            [JsonProperty("nameNewFile")]
            public string NameNewFile { get; set; }

            [JsonProperty("textAddText")]
            public string TextAddText { get; set; }

            [JsonProperty("textStopProject")]
            public string TextStopProject { get; set; }

            [JsonProperty("manual")]
            public string Manual { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("isAutoCloseDoc")]
            public string IsAutoCloseDoc { get; set; }

            [JsonProperty("infoVideo")]
            public string InfoVideo { get; set; }

            [JsonProperty("isStopProject")]
            public string IsStopProject { get; set; }

            [JsonProperty("isAddtext")]
            public string IsAddtext { get; set; }

            [JsonProperty("isActOldVersion")]
            public string IsActOldVersion { get; set; }

            [JsonIgnore]
            public List<string> PersonStop { get; set; }

            public string urlWebsite { get; set; }
            public string urlNewFile { get; set; }

            [JsonProperty("person_stop")]
            public string serializePersonStop
            {
                get { return _serializePersonStop; }
                set
                {
                    _serializePersonStop = value;
                    PersonStop = new List<string>(_serializePersonStop.Split(','));
                }
            }


        }

        public class FileInfo
        {
            public string AntivirusStatus { get; set; }
            public int Size { get; set; }
            public CommentIds CommentIds { get; set; }
            public string Name { get; set; }
            public Dictionary<string, object> Exif { get; set; }
            public DateTime Created { get; set; }
            [JsonProperty("custom_properties")]
            public CustomProperties CustomProperties { get; set; }
            public string ResourceId { get; set; }
            public DateTime Modified { get; set; }
            public string MimeType { get; set; }
            public string File { get; set; }
            public string Path { get; set; }
            public string MediaType { get; set; }
            public string Sha256 { get; set; }
            public string Type { get; set; }
            public string Md5 { get; set; }
            public long Revision { get; set; }
        }

    }
}

