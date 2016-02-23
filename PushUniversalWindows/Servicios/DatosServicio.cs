using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using PushUniversalWindows.Model;
using PushUniversalWindows.Utils;

namespace PushUniversalWindows.Servicios
{
    public class DatosServicio
    {
        private readonly MobileServiceClient _mobileService;


        public DatosServicio()
        {
            _mobileService = new MobileServiceClient(Constantes.MobileServiceUrl, Constantes.MobileServiceKey);
        }

        public async Task<List<Smartphone>> GetSmartphones()
        {
            var data = await _mobileService.GetTable<Smartphone>().ToListAsync();
            return data;
        }

        public async Task<Smartphone> AddSmartphone(Smartphone model)
        {
            await _mobileService.GetTable<Smartphone>().InsertAsync(model);
            return model;
        }

        public async void DeleteSmartphone(Smartphone model)
        {
            await _mobileService.GetTable<Smartphone>().DeleteAsync(model);
        }
    }
}