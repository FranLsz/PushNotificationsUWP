using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.PushNotifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure.Messaging;
using Microsoft.WindowsAzure.MobileServices;
using PushUniversalWindows.Model;
using PushUniversalWindows.Pages;
using PushUniversalWindows.Servicios;
using PushUniversalWindows.Utils;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PushUniversalWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Smartphone smartphoneSeleccionado;

        private ObservableCollection<Smartphone> Smartphones;

        public MainPage()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            List<Smartphone> lista = Task.Run(() => new DatosServicio().GetSmartphones()).Result;

            Smartphones = new ObservableCollection<Smartphone>(lista);
            MasterListView.ItemsSource = Smartphones;

        }



        private void OnCurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            bool isNarrow = e.NewState == NarrowState;
            if (isNarrow)
            {
                Frame.Navigate(typeof(DetailsPage), smartphoneSeleccionado, new SuppressNavigationTransitionInfo());
            }
            else
            {
                VisualStateManager.GoToState(this, MasterDetailsState.Name, true);
                MasterListView.SelectionMode = ListViewSelectionMode.Extended;
                MasterListView.SelectedItem = smartphoneSeleccionado;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (smartphoneSeleccionado == null && Smartphones.Count > 0)
            {
                smartphoneSeleccionado = Smartphones[0];
                MasterListView.SelectedIndex = 0;
            }
            // If the app starts in narrow mode - showing only the Master listView - 
            // it is necessary to set the commands and the selection mode.
            if (PageSizeStatesGroup.CurrentState == NarrowState)
            {
                VisualStateManager.GoToState(this, MasterState.Name, true);
            }
            else if (PageSizeStatesGroup.CurrentState == WideState)
            {
                // In this case, the app starts is wide mode, Master/Details view, 
                // so it is necessary to set the commands and the selection mode.
                VisualStateManager.GoToState(this, MasterDetailsState.Name, true);
                MasterListView.SelectionMode = ListViewSelectionMode.Extended;
                MasterListView.SelectedItem = smartphoneSeleccionado;
            }
            else
            {
                new InvalidOperationException();
            }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            // The clicked item it is the new selected contact
            smartphoneSeleccionado = e.ClickedItem as Smartphone;
            if (PageSizeStatesGroup.CurrentState == NarrowState)
            {
                // Go to the details page and display the item 
                Frame.Navigate(typeof(DetailsPage), smartphoneSeleccionado, new DrillInNavigationTransitionInfo());
            }
            //else
            {
                // Play a refresh animation when the user switches detail items.
                //EnableContentTransitions();
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageSizeStatesGroup.CurrentState == WideState)
            {
                if (MasterListView.SelectedItems.Count == 1)
                {
                    smartphoneSeleccionado = MasterListView.SelectedItem as Smartphone;
                    EnableContentTransitions();
                }
                // Entering in Extended selection
                else if (MasterListView.SelectedItems.Count > 1
                     && MasterDetailsStatesGroup.CurrentState == MasterDetailsState)
                {
                    VisualStateManager.GoToState(this, ExtendedSelectionState.Name, true);
                }
            }
            // Exiting Extended selection
            if (MasterDetailsStatesGroup.CurrentState == ExtendedSelectionState &&
                MasterListView.SelectedItems.Count == 1)
            {
                VisualStateManager.GoToState(this, MasterDetailsState.Name, true);
            }
        }
        private void ShowSliptView(object sender, RoutedEventArgs e)
        {
            // Clearing the cache
            int cacheSize = ((Frame)Parent).CacheSize;
            ((Frame)Parent).CacheSize = 0;
            ((Frame)Parent).CacheSize = cacheSize;

            // MySamplesPane.SamplesSplitView.IsPaneOpen = !MySamplesPane.SamplesSplitView.IsPaneOpen;
        }

        private void SelectItems(object sender, RoutedEventArgs e)
        {
            if (MasterListView.Items.Count > 0)
            {
                VisualStateManager.GoToState(this, MultipleSelectionState.Name, true);
            }
        }

        private void EnableContentTransitions()
        {
            DetailContentPresenter.ContentTransitions.Clear();
            DetailContentPresenter.ContentTransitions.Add(new EntranceThemeTransition());
        }


        private async void AddItem(object sender, RoutedEventArgs e)
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            var hub = new NotificationHub(Constantes.NotificationHubPath, Constantes.ConnectionString);
            var result = await hub.RegisterNativeAsync(channel.Uri);

            var dialogo = GenerarDialogo();

            // Lanzar dialogo
            await dialogo.ShowAsync();


            




            // Select this item in case that the list is empty
            if (MasterListView.SelectedIndex == -1)
            {
                MasterListView.SelectedIndex = 0;
                smartphoneSeleccionado = MasterListView.SelectedItem as Smartphone;
                // Details view is collapsed, in case there is not items.
                // You should show it just in case. 
                DetailContentPresenter.Visibility = Visibility.Visible;
            }
        }

        private ContentDialog GenerarDialogo()
        {
            #region Creacion del dialogo

            var dialogo = new ContentDialog()
            {
                Title = "Añadir nuevo smartphone",
                //RequestedTheme = ElementTheme.Dark,
                //FullSizeDesired = true,
                MaxWidth = this.ActualWidth // Required for Mobile!
            };

            // Setup Content
            var panel = new StackPanel();


            var modeloLabel = new TextBlock()
            {
                Text = "Modelo"
            };
            var modeloBox = new TextBox();
            {
                Name = "Modelo";
            }
            ;

            var fabricanteLabel = new TextBlock()
            {
                Text = "Fabricante"
            };
            var fabricanteBox = new TextBox();
            {
                Name = "Fabricante";
            }
            ;

            var precioLabel = new TextBlock()
            {
                Text = "Precio (€)"
            };
            var precioBox = new TextBox();
            {
                Name = "Precio";
            }
            ;

            panel.Children.Add(modeloLabel);
            panel.Children.Add(modeloBox);
            panel.Children.Add(fabricanteLabel);
            panel.Children.Add(fabricanteBox);
            panel.Children.Add(precioLabel);
            panel.Children.Add(precioBox);

            dialogo.Content = panel;

            #endregion

            // Se añaden los botones de accion
            dialogo.PrimaryButtonText = "Agregar";
            dialogo.SecondaryButtonText = "Cancelar";

            // Al pulsar en "Agregar"
            dialogo.PrimaryButtonClick += delegate
            {
                Smartphone nuevo = new Smartphone()
                {
                    Modelo = modeloBox.Text,
                    Fabricante = fabricanteBox.Text,
                    Precio = Convert.ToDecimal(precioBox.Text)
                };

                var mobileService = new DatosServicio();
                var s = Task.Run(() => mobileService.AddSmartphone(nuevo)).Result;

                Smartphones.Add(s);
            };
            return dialogo;
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            if (smartphoneSeleccionado != null)
            {
                var mobileService = new DatosServicio();
                Smartphones.Remove(smartphoneSeleccionado);
                mobileService.DeleteSmartphone(smartphoneSeleccionado);

                if (MasterListView.Items.Count > 0)
                {
                    MasterListView.SelectedIndex = 0;
                    smartphoneSeleccionado = MasterListView.SelectedItem as Smartphone;
                }
                else
                {
                    DetailContentPresenter.Visibility = Visibility.Collapsed;
                    smartphoneSeleccionado = null;
                }
            }
        }
        private void DeleteItems(object sender, RoutedEventArgs e)
        {
            if (MasterListView.SelectedIndex != -1)
            {
                List<Smartphone> selectedItems = new List<Smartphone>();
                var mobileService = new DatosServicio();
                foreach (Smartphone sm in MasterListView.SelectedItems)
                {
                    selectedItems.Add(sm);
                }
                foreach (Smartphone sm in selectedItems)
                {
                    Smartphones.Remove(sm);
                    mobileService.DeleteSmartphone(sm);
                }
                if (MasterListView.Items.Count > 0)
                {
                    MasterListView.SelectedIndex = 0;
                    smartphoneSeleccionado = MasterListView.SelectedItem as Smartphone;
                }
                else
                {
                    DetailContentPresenter.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CancelSelection(object sender, RoutedEventArgs e)
        {
            if (PageSizeStatesGroup.CurrentState == NarrowState)
            {
                VisualStateManager.GoToState(this, MasterState.Name, true);
            }
            else
            {
                VisualStateManager.GoToState(this, MasterDetailsState.Name, true);
            }
        }
    }
}
