using Microsoft.AspNetCore.SignalR.Client;

namespace Itm.Store.Mobile;

    public partial class MainPage : ContentPage
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private HubConnection? _hubConnection;    

        public MainPage(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory;

            InitializeSignalR();
        }

        private async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://10.0.2.2:5000/hubs/notifications")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string>("TicketReady", (mensajeDelServidor) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ResultLabel.Text = $" ALERTA EN VIVO: {mensajeDelServidor}";
                    ResultLabel.TextColor = Colors.Purple;
                });
            });

            try
            {
                await _hubConnection.StartAsync();
            }   catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar con el Hub de SignalR: {ex.Message}");
            }
        }



        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string simulatedToken = "eyJhbGciOiJIUzI1NiIsInR... (token simulado)";

            await SecureStorage.Default.SetAsync("jwt_token", simulatedToken);

            ResultLabel.Text = "¡Token JWT guardado seguro en el dispositivo!";
            ResultLabel.TextColor = Colors.Green;
        }

        private async void OnGetDataClicked(object sender, EventArgs e)
        {
            try
            {
                ResultLabel.Text = "Consultando Gateway...";
                ResultLabel.TextColor = Colors.Orange;

                var client = _httpClientFactory.CreateClient("GatewayClient");

                var response = await client.GetAsync("/api/products/1/check-stock");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    ResultLabel.Text = $"ÉXITO:\n{data}";
                    ResultLabel.TextColor = Colors.Green;
                }
                else
                {
                    ResultLabel.Text = $"ERROR {response.StatusCode}:\n{await response.Content.ReadAsStringAsync()}";
                    ResultLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                ResultLabel.Text = $"ERROR DE RED:\n{ex.Message}";
                ResultLabel.TextColor = Colors.Red;
            }
        }
    }