using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WakeOnLan;

namespace FrpGUI.Util
{
    public class HttpServerHelper
    {
        private HttpListener listener;

        public async Task Start()
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add($"http://{Config.GetAddress()}/");
                listener.Start();

                var requests = new HashSet<Task>();
                for (int i = 0; i < 10; i++)
                    requests.Add(listener.GetContextAsync());

                while (true)
                {
                    Task t = await Task.WhenAny(requests);
                    requests.Remove(t);

                    if (t is Task<HttpListenerContext>)
                    {
                        var context = (t as Task<HttpListenerContext>).Result;
                        requests.Add(ProcessRequestAsync(context));
                        requests.Add(listener.GetContextAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("启动远程管理错误", ex);
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                var response = context.Response;
                var request = context.Request;
                string responseString = "";
                switch (request.HttpMethod)
                {
                    case "POST":
                        using (Stream body = request.InputStream)
                        {
                            using StreamReader reader = new StreamReader(body, request.ContentEncoding);
                            string value = reader.ReadToEnd();
                            JObject json = JObject.Parse(value);
                            string password = json["password"].Value<string>();
                            int id = json["id"].Value<int>();
                            if (!string.IsNullOrEmpty(password) || !string.IsNullOrEmpty(Config.GetPassword()))
                            {
                                if (password != Config.GetPassword())
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                    return;
                                }
                            }
                            var hosts = Config.GetHosts();
                            if (id < 0 || id >= hosts.Count)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                return;
                            }
                            await WOL.WakeOnLan(hosts[id].Mac);
                        }
                        break;

                    case "GET":

                        if (context.Request.RawUrl == "/")
                        {
                            responseString = File.ReadAllText("html/admin.html").Replace("{{url}}", $"http://{Config.GetAddress()}");
                            StringBuilder items = new StringBuilder();
                            int i = 0;
                            foreach (var host in Config.GetHosts())
                            {
                                items.Append($"<a class=\"text-center label\">{host.Name}  {host.Mac}</a> ")
                                    .Append($"<button class=\"btn btn-primary \" data-id=\"{i++}\">唤醒</button>")
                                    .Append("<p></p>");
                            }
                            responseString = responseString.Replace("{{items}}", items.ToString());
                        }
                        else
                        {
                            responseString = File.ReadAllText("html" + context.Request.RawUrl);
                        }
                        break;
                }

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("远程管理服务器错误：" + ex.Message);
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }
    }
}