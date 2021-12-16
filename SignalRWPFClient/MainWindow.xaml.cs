#region snippet_MainWindowClass
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
                    var beautified = parsedJson.ToString(Formatting.Indented);;
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

        private void UpdateSignalRConnectionStatus(bool isConnected)
        {
            if (isConnected)
            {
                connectButton.IsEnabled = false;
                connectButton.Visibility = Visibility.Collapsed;
                disconnectButton.IsEnabled = true;
                disconnectButton.Visibility = Visibility.Visible;
                sendButton.IsEnabled = true;

                userTextBox.Text = "WPF Client";
                userTextBox.IsEnabled =true;
                messageTextBox.Text = "This is a message from WPF Client";
                messageTextBox.IsEnabled = true;

                OutputBlock.Text += "*************************************Connected*************************************\n";
            }
            else
            {
                connectButton.IsEnabled = true;
                connectButton.Visibility = Visibility.Visible;
                disconnectButton.IsEnabled = false;
                disconnectButton.Visibility = Visibility.Collapsed;
                sendButton.IsEnabled = false;

                userTextBox.Text = "disconnected";
                userTextBox.IsEnabled =false;
                messageTextBox.Text = "disconnected";
                messageTextBox.IsEnabled = false;

                OutputBlock.Text += "*************************************Disconnected**********************************\n";
            }
        }

        private async void clearOutputButton_Click(object sender, RoutedEventArgs e)
        {
            OutputBlock.Text = "";
        }

        }
    }
#endregion
