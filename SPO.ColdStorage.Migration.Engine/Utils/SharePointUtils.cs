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
        public static async Task<ListItem> SaveNewFile(this List targetList, ClientContext ctx, string fileName, byte[] contents)
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

            // Get new file info
            ctx.Load(uploadFile, i => i.UniqueId);
            var uploadedItem = targetList.GetItemByUniqueId(uploadFile.UniqueId);
            ctx.Load(uploadedItem);

            await ctx.ExecuteQueryAsync();

            return uploadedItem;
        }
    }
}
