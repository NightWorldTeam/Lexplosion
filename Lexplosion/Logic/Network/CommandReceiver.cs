using System;
using System.Net;
using System.IO;
using System.Text;

namespace Lexplosion.Logic.Network
{
    static class CommandReceiver
    {
        public static event Action<string> OpenModpackPage;

        public static void StartCommandServer()
        {
            if (HttpListener.IsSupported)
            {
                Lexplosion.Run.TaskRun(delegate ()
                {
                    try
                    {
                        using (HttpListener listener = new HttpListener())
                        {
                            listener.Prefixes.Add("http://127.0.0.1:54352/");
                            listener.Start();

                            while (true)
                            {
                                HttpListenerRequest request;
                                HttpListenerResponse response;

                                try
                                {
                                    HttpListenerContext context = listener.GetContext();
                                    request = context.Request;
                                    response = context.Response;
                                }
                                catch
                                {
                                    break;
                                }

                                try
                                {
                                    string url = request.Url.LocalPath;
                                    if (url.Contains("/openModpackPage"))
                                    {
                                        OpenModpackPage(url.Replace("/openModpackPage/", ""));
                                    }

                                    byte[] buffer = Encoding.UTF8.GetBytes("OK");
                                    response.ContentLength64 = buffer.Length;

                                    using (Stream output = response.OutputStream)
                                    {
                                        output.Write(buffer, 0, buffer.Length);
                                        output.Close();
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                });
            }
        }
    }
}
