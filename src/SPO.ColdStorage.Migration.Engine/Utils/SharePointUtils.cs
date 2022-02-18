using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine.Utils
{
    public static class SharePointUtils
    {
        public static async Task SaveNewFile(this List targetList, ClientContext ctx, string fileName, byte[] contents)
        {
            var fileCreationInfo = new FileCreationInformation
            {
                Content = contents,
                Overwrite = true,
                Url = fileName
            };
            var uploadFile = targetList.RootFolder.Files.Add(fileCreationInfo);
            ctx.Load(uploadFile);
            await ctx.ExecuteQueryAsync();

        }
    }
}
