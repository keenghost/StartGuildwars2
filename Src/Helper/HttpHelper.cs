using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StartGuildwars2.Model;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace StartGuildwars2.Helper
{
    public class HttpHelper
    {
        public static string StartGuildwars2ApiHost = "https://gw2.keenghost.com";

        public static HttpClient HttpClient = GenerateDefaultHttpClient();

        private static HttpClient GenerateDefaultHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(StartGuildwars2ApiHost)
            };

            client.DefaultRequestHeaders.Add("Accept", "application/json");

            return client;
        }

        public static void GetAsync<T>(RequestGetModel<T> config)
        {
            var queryObject = HttpUtility.ParseQueryString(string.Empty);

            foreach (var item in config.Query)
            {
                queryObject[item.Key] = item.Value;
            }
            queryObject["_"] = UtilHelper.GetUniqueID();

            HttpClient.GetAsync(config.Path + "?" + queryObject.ToString()).ContinueWith(res =>
            {
                try
                {
                    var resultString = res.Result.Content.ReadAsStringAsync().Result;

                    if (res.Result.IsSuccessStatusCode)
                    {
                        try
                        {
                            config.SuccessCallback.Invoke(JsonConvert.DeserializeObject<ResponseDataModel<T>>(resultString));
                        }
                        catch
                        {
                            config.ErrorCallback.Invoke(new ResponseExceptionModel
                            {
                                Status = -1,
                                ErrorDetail = new ResponseExceptionErrorDetail
                                {
                                    code = -1,
                                    message = "error parsing success response data",
                                    extra = null,
                                },
                            });
                        }
                    }
                    else
                    {
                        try
                        {
                            config.ErrorCallback.Invoke(new ResponseExceptionModel
                            {
                                Status = (int)res.Result.StatusCode,
                                ErrorDetail = JsonConvert.DeserializeObject<ResponseExceptionErrorDetail>(resultString),
                            });
                        }
                        catch
                        {
                            config.ErrorCallback.Invoke(new ResponseExceptionModel
                            {
                                Status = -1,
                                ErrorDetail = new ResponseExceptionErrorDetail
                                {
                                    code = -1,
                                    message = "error parsing bad response data",
                                    extra = null,
                                },
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    config.ErrorCallback.Invoke(new ResponseExceptionModel
                    {
                        Status = -1,
                        ErrorDetail = new ResponseExceptionErrorDetail
                        {
                            code = -1,
                            message = ex.Message,
                            extra = null,
                        },
                    });
                }

                config.CompleteCallback.Invoke();
            });
        }

        public static void PostAsync<T>(RequestPostModel<T> config)
        {
            var jObject = new JObject();

            foreach (var item in config.Body)
            {
                jObject[item.Key] = JToken.FromObject(item.Value);
            }

            var content = new StringContent(JsonConvert.SerializeObject(jObject));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpClient.PostAsync(config.Path, content).ContinueWith(res =>
            {
                try
                {
                    var resultString = res.Result.Content.ReadAsStringAsync().Result;

                    if (res.Result.IsSuccessStatusCode)
                    {
                        try
                        {
                            config.SuccessCallback.Invoke(JsonConvert.DeserializeObject<ResponseDataModel<T>>(resultString));
                        }
                        catch
                        {
                            config.ErrorCallback.Invoke(new ResponseExceptionModel
                            {
                                Status = -1,
                                ErrorDetail = new ResponseExceptionErrorDetail
                                {
                                    code = -1,
                                    message = "error parsing success response data",
                                    extra = null,
                                },
                            });
                        }
                    }
                    else
                    {
                        try
                        {
                            config.ErrorCallback.Invoke(new ResponseExceptionModel
                            {
                                Status = (int)res.Result.StatusCode,
                                ErrorDetail = JsonConvert.DeserializeObject<ResponseExceptionErrorDetail>(resultString),
                            });
                        }
                        catch
                        {
                            config.ErrorCallback.Invoke(new ResponseExceptionModel
                            {
                                Status = -1,
                                ErrorDetail = new ResponseExceptionErrorDetail
                                {
                                    code = -1,
                                    message = "error parsing bad response data",
                                    extra = null,
                                },
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    config.ErrorCallback.Invoke(new ResponseExceptionModel
                    {
                        Status = -1,
                        ErrorDetail = new ResponseExceptionErrorDetail
                        {
                            code = -1,
                            message = ex.Message,
                            extra = null,
                        },
                    });
                }

                config.CompleteCallback.Invoke();
            });
        }

        public static void DownloadFileAsync(RequestDownloadFileModel config)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(config.LocalPath));

                WebClient webClient = new WebClient();

                webClient.DownloadProgressChanged += (sender, e) =>
                {
                    config.ProgressCallback.Invoke(e.ProgressPercentage);
                };

                webClient.DownloadFileCompleted += (sender, e) =>
                {
                    if (e.Error != null)
                    {
                        IOHelper.DeleteFileOrDirectory(config.LocalPath);
                        config.ErrorCallback.Invoke(e.Error);
                    }
                    else if (!e.Cancelled)
                    {
                        config.SuccessCallback.Invoke();
                    }

                    config.CompleteCallback.Invoke();
                };

                webClient.DownloadFileAsync(new Uri(config.RemoteUrl), config.LocalPath);
            }
            catch (Exception e)
            {
                config.ErrorCallback.Invoke(e);
                config.CompleteCallback.Invoke();
            }
        }
    }
}