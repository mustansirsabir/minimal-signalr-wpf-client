#region snippet_MainWindowClass
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Windows;

namespace SignalRWPFClient
{
    public partial class MainWindow : Window
    {
        private HubConnection connection;
        private string HubUrl;

        public MainWindow()
        {
            InitializeComponent();
            HubUrl = "";
        }

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            connection = new HubConnectionBuilder()
                .WithUrl(HubUrl)
                .Build();

            #region snippet_ClosedRestart
            connection.Closed += async (error) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    UpdateSignalRConnectionStatus(false);
                });
            };
            #endregion

            #region snippet_ConnectionOn

            connection.On<string, string>("MessageFromServer", (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    JToken parsedJson = JToken.Parse(message);
                    var beautified = parsedJson.ToString(Formatting.Indented); ;
                    OutputBlock.Text += $"{user}:\n{beautified}\n";
                });
            });
            #endregion

            try
            {
                await connection.StartAsync();
                UpdateSignalRConnectionStatus(true);
                string connectionId = await connection.InvokeAsync<string>("GetConnectionId");
                OutputBlock.Text += "Connection Id: " + connectionId + "\n";

            }
            catch (Exception ex)
            {
                OutputBlock.Text += ex.Message + "\n";
            }
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            #region snippet_ErrorHandling
            try
            {
                #region snippet_InvokeAsync
                await connection.InvokeAsync("SendMessage",
                    userTextBox.Text, messageTextBox.Text);
                #endregion
            }
            catch (Exception ex)
            {
                OutputBlock.Text += ex.Message + "\n";
            }
            #endregion
        }

        private async void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            #region snippet_ErrorHandling
            try
            {
                await connection.StopAsync();
                await connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                OutputBlock.Text += ex.Message + "\n";
            }
            #endregion
        }
        private async void clearOutputButton_Click(object sender, RoutedEventArgs e)
        {
            OutputBlock.Text = "";
        }

        private void UpdateSignalRConnectionStatus(bool isConnected)
        {
            connectButton.IsEnabled = !isConnected;
            connectButton.Visibility = isConnected ? Visibility.Collapsed : Visibility.Visible;
            disconnectButton.IsEnabled = isConnected;
            disconnectButton.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
            sendButton.IsEnabled = isConnected;

            userTextBox.IsEnabled = isConnected;
            messageTextBox.IsEnabled = isConnected;

            if (isConnected)
            {
                userTextBox.Text = "WPF Client";
                messageTextBox.Text = "This is a message from WPF Client";

                OutputBlock.Text += "*************************************Connected*************************************\n";
            }
            else
            {
                userTextBox.Text = "disconnected";
                messageTextBox.Text = "disconnected";

                OutputBlock.Text += "*************************************Disconnected**********************************\n";
            }
        }
    }
}
#endregion
