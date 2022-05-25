using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System;
using System.Net;
using Newtonsoft.Json;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Network;
using Lexplosion.Logic.Objects;
using Lexplosion.Logic.Objects.Curseforge;
using Lexplosion.Logic.Objects.CommonClientData;
using Lexplosion.Global;

namespace Lexplosion.Logic.Management
{
    static class ManageLogic
    {
        public static AuthCode Auth(string login, string password, bool saveUser)
        {
            Dictionary<string, string> response = ToServer.Authorization(login, password);

            if (response != null)
            {
                if (response["status"] == "OK")
                {
                    UserData.Login = response["login"];
                    UserData.UUID = response["UUID"];
                    UserData.AccessToken = response["accesToken"];

                    if (saveUser)
                    {
                        DataFilesManager.SaveAccount(login, password);
                    }

                    UserData.IsAuthorized = true;
                    UserStatusSetter.SetBaseStatus(UserStatusSetter.Statuses.Online);

                    return AuthCode.Successfully;
                }
                else
                {
                    return AuthCode.DataError;
                }
            }
            else
            {
                return AuthCode.NoConnect;
            }
        }

        //public static ImportResult ImportInstance(string zipFile, out List<string> errors, ProgressHandlerCallback ProgressHandler)
        //{ // TODO : этот метод полная хуйня блять, надо доделать, может даже переделать
        //    string instanceId;
        //    ImportResult res = WithDirectory.ImportInstance(zipFile, out errors, out instanceId);
        //    LocalInstance instance = new LocalInstance(instanceId);

        //    InstanceInit result = instance.Check(out string gameVersion); // TODO: тут вовзращать ошибки

        //    if (result == InstanceInit.Successful)
        //    {
        //        string javaPath;
        //        using (JavaChecker javaCheck = new JavaChecker(gameVersion))
        //        {
        //            if (javaCheck.Check(out JavaChecker.CheckResult checkResult, out JavaVersion javaVersion))
        //            {
        //                bool downloadResult = javaCheck.Update(delegate (int percent)
        //                {
        //                    ProgressHandler(DownloadStageTypes.Java, 0, 0, percent);
        //                });

        //                if (!downloadResult)
        //                {
        //                    return ImportResult.JavaDownloadError;
        //                }
        //            }

        //            if (checkResult == JavaChecker.CheckResult.Successful)
        //            {
        //                javaPath = WithDirectory.DirectoryPath + "/java/" + javaVersion.JavaName + javaVersion.ExecutableFile;
        //            }
        //            else
        //            {
        //                return ImportResult.JavaDownloadError;
        //            }
        //        }

        //        instance.Update(javaPath, ProgressHandler);
        //    }

        //    // TODO: Тут вырезал строку
        //    /*
        //    if (Gui.PageType.Right.Menu.InstanceContainerPage.obj != null)
        //    {
        //        Uri logoPath = new Uri("pack://application:,,,/assets/images/icons/non_image.png");
        //        Gui.PageType.Right.Menu.InstanceContainerPage.obj.BuildInstanceForm(instanceId, UserData.InstancesList.Count - 1, logoPath, UserData.InstancesList[instanceId].Name, "NightWorld", "test", new List<string>());
        //    }
        //    */

        //    return res;
        //}
    }
}
