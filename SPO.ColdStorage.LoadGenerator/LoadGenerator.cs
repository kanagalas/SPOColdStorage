using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Migration.Engine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.LoadGenerator
{
    internal class LoadGenerator
    {
        private readonly ClientContext _spClient;

        public LoadGenerator(ClientContext context)
        {
            this._spClient = context;
        }

        public async Task Go(int fileCount)
        {
            int filesAdded = 0;
            var targetLists = await GetAllListsAllWebs();
            while (filesAdded < fileCount)
            {
                foreach (var list in targetLists)
                {
                    if (filesAdded == fileCount)
                    {
                        break;
                    }

                    if (list.BaseType == BaseType.GenericList)
                    {
                        await AddFileToCustomList(list);
                    }
                    else if (list.BaseType == BaseType.DocumentLibrary)
                    {
                        await AddFileToDocLib(list);
                    }
                    filesAdded++;
                    Console.WriteLine(filesAdded);
                }
            }
            
        }

        public async Task<IEnumerable<List>> GetAllListsAllWebs()
        {
            var results = new List<List>();
            var rootWeb = _spClient.Web;
            _spClient.Load(rootWeb);
            _spClient.Load(rootWeb.Webs);
            await _spClient.ExecuteQueryAsync();

            results.AddRange(await GetAllLists(rootWeb));

            foreach (var subSweb in rootWeb.Webs)
            {
                results.AddRange(await GetAllLists(subSweb));
            }

            return results;
        }

        private async Task<IEnumerable<List>> GetAllLists(Web web)
        {
            var results = new List<List>();
            _spClient.Load(web.Lists);
            _spClient.Load(web.Webs);
            await _spClient.ExecuteQueryAsync();

            foreach (var list in web.Lists)
            {

                _spClient.Load(list, l => l.BaseType, l => l.IsSystemList);
                _spClient.Load(list.RootFolder);
                _spClient.Load(list, l => l.RootFolder.Name);
                await _spClient.ExecuteQueryAsync();

                // Only upload to safe lists
                if (!list.IsSystemList && !list.Hidden)
                {
                    results.Add(list);
                    Console.WriteLine(list.RootFolder.ServerRelativeUrl);
                }
            }

            return results;
        }

        private async Task AddFileToDocLib(List list)
        {
            await list.SaveNewFile(_spClient, $"test{DateTime.Now.Ticks}.txt", System.Text.Encoding.UTF8.GetBytes("bum"));
        }

        private async Task AddFileToCustomList(List list)
        {
            var newName = DateTime.Now.Ticks.ToString();
            var newItemCreateInfo = new ListItemCreationInformation();
            var oListItem = list.AddItem(newItemCreateInfo);
            oListItem["Title"] = newName;

            oListItem.Update();

            await _spClient.ExecuteQueryAsync();

            var attInfo = new AttachmentCreationInformation();
            attInfo.FileName = newName + ".txt";
            attInfo.ContentStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("bum"));

            Attachment att = oListItem.AttachmentFiles.Add(attInfo); //Add to File

            _spClient.Load(att);

            await _spClient.ExecuteQueryAsync();

            //await list.SaveNewFile(_spClient, $"{oListItem.Id}/test{DateTime.Now.Ticks}.txt", System.Text.Encoding.UTF8.GetBytes("bum"));
        }
    }
}
