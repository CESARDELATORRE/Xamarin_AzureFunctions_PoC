using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;
using System.Net.Http.Headers;

using Xamarin.Forms;
using Newtonsoft.Json;

namespace Xamarin_AzureFunctions_PoC
{
    
    public class App : Application
    {
        static ListView lstVehicles = new ListView();
        static Label titleLabel = new Label();
        public App()
        {
            MainPage = GetMainPage();
        }
        public static ContentPage GetMainPage()
        {
            titleLabel.HorizontalTextAlignment = TextAlignment.Center;
            titleLabel.Text = "Testing Azure Functions from Xamarin!";
            
            Button newButn = new Button()
            {
                Text = "Connect to Azure Function",
            };
            newButn.Clicked += newButn_Clicked;
            lstVehicles.ItemTemplate = new DataTemplate(typeof(TextCell));
            lstVehicles.ItemTemplate.SetBinding(TextCell.TextProperty, "Model");

            return new ContentPage
            {
                Content = new StackLayout()
                {
                    Children = {
                     titleLabel,
                     newButn,
                     lstVehicles
                    }
                }
            };
        }
        static async void newButn_Clicked(object sender, EventArgs e)
        {
            string previousTextValue = titleLabel.Text;
            titleLabel.Text = "******* Executing the Azure Function! ********";
            
            VehiclesAzureFunction vehiclesAzureFunction = new VehiclesAzureFunction("https://helloworldfunctcdltll.azurewebsites.net/");

            //Consume the Azure Function with hardcoded method easier to see as an example
            var vehiclesList = await vehiclesAzureFunction.GetVehiclesHardCodedAsync();

            //Consume the Azure Function with HttpClient reusable code within the Function/Service Agent
            //var vehiclesList = await vehiclesAzureFunction.GetVehiclesAsync("Chevrolet");
            lstVehicles.ItemsSource = vehiclesList;
           
            titleLabel.Text = previousTextValue;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }

    public class Vehicle
    {
        public string Make { get; set; }

        public string Model { get; set; }
    }


    public class VehiclesAzureFunction : BaseRequest
    {
        public VehiclesAzureFunction(string urlPrefix)
            : base(urlPrefix)
        {

        }

        //(CDLTLL) Hardcoded code easier to see the example --
        public async Task<List<Vehicle>> GetVehiclesHardCodedAsync()
        {
            var client = new System.Net.Http.HttpClient();            

            string url = $"https://helloworldfunctcdltll.azurewebsites.net/api/HttpTriggerVehicleList?code=mi11r1elo9fwd2cafw5ozctgy8yug8fr0cjc&make=Chevrolet";
            var response = await client.GetAsync(url);
            var vehiclesJson = response.Content.ReadAsStringAsync().Result;

            List<Vehicle> listOfVehicles = JsonConvert.DeserializeObject<List<Vehicle>>(vehiclesJson);            
            return listOfVehicles;
        }

        //(CDLTLL) Using Base class so code is re-used across requests --
        public async Task<List<Vehicle>> GetVehiclesAsync(string make)
        {
            string url = $"{_UrlPrefix}api/HttpTriggerVehicleList?code=mi11r1elo9fwd2cafw5ozctgy8yug8fr0cjc&make={make}";

            return await GetAsync<List<Vehicle>>(url);
        }
    }

    public class BaseRequest
    {
        protected string _UrlPrefix = string.Empty;        

        public string UrlPrefix
        {
            get
            {
                return _UrlPrefix;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value) ||
                    !Uri.IsWellFormedUriString(value, UriKind.Absolute))
                {
                    return;
                }

                _UrlPrefix = value;
            }
        }

        public BaseRequest(string urlPrefix)
        {
            if (String.IsNullOrEmpty(urlPrefix))
                throw new ArgumentNullException("urlPrefix");

            if (!urlPrefix.EndsWith("/"))
                urlPrefix = string.Concat(urlPrefix, "/");

            _UrlPrefix = urlPrefix;            
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));            
            return httpClient;
        }

        protected async Task<T> GetAsync<T>(string url)
            where T : new()
        {
            HttpClient httpClient = CreateHttpClient();
            T result;

            try
            {
                var response = await httpClient.GetStringAsync(url);
                result = await Task.Run(() => JsonConvert.DeserializeObject<T>(response));
            }
            catch
            {
                result = new T();
            }

            return result;
        }

    }

}
