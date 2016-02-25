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
using PushUniversalWindows.Utiles;

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

            // tags
            var tags = new List<string>() { "NuevoSmartphone" };

            var servicioNotificacion = new ServicioNotificationHub(
                Constantes.NotificationHubPath,
                Constantes.ConnectionString,
                tags);

            Task.Run(() => servicioNotificacion.Registrar());

            var lista = Task.Run(() => new ServicioDatos().GetSmartphones()).Result;
            Smartphones = new ObservableCollection<Smartphone>(lista);
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
            if (PageSizeStatesGroup.CurrentState == NarrowState)
            {
                VisualStateManager.GoToState(this, MasterState.Name, true);
            }
            else if (PageSizeStatesGroup.CurrentState == WideState)
            {
                VisualStateManager.GoToState(this, MasterDetailsState.Name, true);
                MasterListView.SelectionMode = ListViewSelectionMode.Extended;
                MasterListView.SelectedItem = smartphoneSeleccionado;
            }
            else
            {
                new InvalidOperationException();
            }
        }


        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            smartphoneSeleccionado = e.ClickedItem as Smartphone;
            if (PageSizeStatesGroup.CurrentState == NarrowState)
            {
                Frame.Navigate(typeof(DetailsPage), smartphoneSeleccionado, new DrillInNavigationTransitionInfo());
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
                else if (MasterListView.SelectedItems.Count > 1
                     && MasterDetailsStatesGroup.CurrentState == MasterDetailsState)
                {
                    VisualStateManager.GoToState(this, ExtendedSelectionState.Name, true);
                }
            }
            if (MasterDetailsStatesGroup.CurrentState == ExtendedSelectionState &&
                MasterListView.SelectedItems.Count == 1)
            {
                VisualStateManager.GoToState(this, MasterDetailsState.Name, true);
            }
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


            var dialogo = GenerarDialogo();

            // Lanzar dialogo
            await dialogo.ShowAsync();

            if (MasterListView.SelectedIndex == -1)
            {
                MasterListView.SelectedIndex = 0;
                smartphoneSeleccionado = MasterListView.SelectedItem as Smartphone;
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
                    Precio = Convert.ToDouble(precioBox.Text)
                };

                var mobileService = new ServicioDatos();
                var s = Task.Run(() => mobileService.AddSmartphone(nuevo)).Result;

                Smartphones.Add(s);
            };
            return dialogo;
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            if (smartphoneSeleccionado != null)
            {
                var mobileService = new ServicioDatos();
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
                var mobileService = new ServicioDatos();
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
