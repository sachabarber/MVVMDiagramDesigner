using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using DiagramDesigner.Helpers;
using DiagramDesigner;
using System.ComponentModel;
using System.Windows.Data;
using DemoApp.Persistence.Common;
using System.Threading.Tasks;

namespace DemoApp
{
    public class Window1ViewModel : INPCBase
    {

        private List<int> savedDiagrams = new List<int>();
        private int? savedDiagramId;
        private List<SelectableDesignerItemViewModelBase> itemsToRemove;
        private IMessageBoxService messageBoxService;
        private IDatabaseAccessService databaseAccessService;
        private DiagramViewModel diagramViewModel = new DiagramViewModel();
        private bool isBusy = false;


        public Window1ViewModel()
        {
            messageBoxService = ApplicationServicesProvider.Instance.Provider.MessageBoxService;
            databaseAccessService = ApplicationServicesProvider.Instance.Provider.DatabaseAccessService;

            foreach (var savedDiagram in databaseAccessService.FetchAllDiagram())
            {
                savedDiagrams.Add(savedDiagram.Id);
            }

            ToolBoxViewModel = new ToolBoxViewModel();
            DiagramViewModel = new DiagramViewModel();

            DeleteSelectedItemsCommand = new SimpleCommand(ExecuteDeleteSelectedItemsCommand);
            CreateNewDiagramCommand = new SimpleCommand(ExecuteCreateNewDiagramCommand);
            SaveDiagramCommand = new SimpleCommand(ExecuteSaveDiagramCommand);
            LoadDiagramCommand = new SimpleCommand(ExecuteLoadDiagramCommand);


            //OrthogonalPathFinder is a pretty bad attempt at finding path points, it just shows you, you can swap this out with relative
            //ease if you wish just create a new IPathFinder class and pass it in right here
            ConnectorViewModel.PathFinder = new OrthogonalPathFinder();

        }

        public SimpleCommand DeleteSelectedItemsCommand { get; private set; }
        public SimpleCommand CreateNewDiagramCommand { get; private set; }
        public SimpleCommand SaveDiagramCommand { get; private set; }
        public SimpleCommand LoadDiagramCommand { get; private set; }
        public ToolBoxViewModel ToolBoxViewModel { get; private set; }


        public DiagramViewModel DiagramViewModel
        {
            get
            {
                return diagramViewModel;
            }
            set
            {
                if (diagramViewModel != value)
                {
                    diagramViewModel = value;
                    NotifyChanged("DiagramViewModel");
                }
            }
        }

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    NotifyChanged("IsBusy");
                }
            }
        }


        public List<int> SavedDiagrams
        {
            get
            {
                return savedDiagrams;
            }
            set
            {
                if (savedDiagrams != value)
                {
                    savedDiagrams = value;
                    NotifyChanged("SavedDiagrams");
                }
            }
        }

        public int? SavedDiagramId
        {
            get
            {
                return savedDiagramId;
            }
            set
            {
                if (savedDiagramId != value)
                {
                    savedDiagramId = value;
                    NotifyChanged("SavedDiagramId");
                }
            }
        }



        private void ExecuteDeleteSelectedItemsCommand(object parameter)
        {
            itemsToRemove = DiagramViewModel.SelectedItems;
            List<SelectableDesignerItemViewModelBase> connectionsToAlsoRemove = new List<SelectableDesignerItemViewModelBase>();

            foreach (var connector in DiagramViewModel.Items.OfType<ConnectorViewModel>())
            {
                if (ItemsToDeleteHasConnector(itemsToRemove, connector.SourceConnectorInfo))
                {
                    connectionsToAlsoRemove.Add(connector);
                }

                if (ItemsToDeleteHasConnector(itemsToRemove, (FullyCreatedConnectorInfo)connector.SinkConnectorInfo))
                {
                    connectionsToAlsoRemove.Add(connector);
                }

            }
            itemsToRemove.AddRange(connectionsToAlsoRemove);
            foreach (var selectedItem in itemsToRemove)
            {
                DiagramViewModel.RemoveItemCommand.Execute(selectedItem);
            }
        }

        private void ExecuteCreateNewDiagramCommand(object parameter)
        {
            //ensure that itemsToRemove is cleared ready for any new changes within a session
            itemsToRemove = new List<SelectableDesignerItemViewModelBase>();
            SavedDiagramId = null;
            DiagramViewModel.CreateNewDiagramCommand.Execute(null);
        }

        private void ExecuteSaveDiagramCommand(object parameter)
        {
            if (!DiagramViewModel.Items.Any())
            {
                messageBoxService.ShowError("There must be at least one item in order save a diagram");
                return;
            }

            IsBusy = true;
            DiagramItem wholeDiagramToSave = null;

            Task<int> task = Task.Factory.StartNew<int>(() =>
                {

                    if (SavedDiagramId != null)
                    {
                        int currentSavedDiagramId = (int)SavedDiagramId.Value;
                        wholeDiagramToSave = databaseAccessService.FetchDiagram(currentSavedDiagramId);

                        //If we have a saved diagram, we need to make sure we clear out all the removed items that
                        //the user deleted as part of this work sesssion
                        foreach (var itemToRemove in itemsToRemove)
                        {
                            DeleteFromDatabase(wholeDiagramToSave, itemToRemove);
                        }
                        //start with empty collections of connections and items, which will be populated based on current diagram
                        wholeDiagramToSave.ConnectionIds = new List<int>();
                        wholeDiagramToSave.DesignerItems = new List<DiagramItemData>();
                    }
                    else
                    {
                        wholeDiagramToSave = new DiagramItem();
                    }

                    //ensure that itemsToRemove is cleared ready for any new changes within a session
                    itemsToRemove = new List<SelectableDesignerItemViewModelBase>();

                    //Save all PersistDesignerItemViewModel
                    foreach (var persistItemVM in DiagramViewModel.Items.OfType<PersistDesignerItemViewModel>())
                    {
                        PersistDesignerItem persistDesignerItem = new PersistDesignerItem(persistItemVM.Id, persistItemVM.Left, persistItemVM.Top, persistItemVM.HostUrl);
                        persistItemVM.Id = databaseAccessService.SavePersistDesignerItem(persistDesignerItem);
                        wholeDiagramToSave.DesignerItems.Add(new DiagramItemData(persistDesignerItem.Id, typeof(PersistDesignerItem)));
                    }
                    //Save all PersistDesignerItemViewModel
                    foreach (var settingsItemVM in DiagramViewModel.Items.OfType<SettingsDesignerItemViewModel>())
                    {
                        SettingsDesignerItem settingsDesignerItem = new SettingsDesignerItem(settingsItemVM.Id, settingsItemVM.Left, settingsItemVM.Top, settingsItemVM.Setting1);
                        settingsItemVM.Id = databaseAccessService.SaveSettingDesignerItem(settingsDesignerItem);
                        wholeDiagramToSave.DesignerItems.Add(new DiagramItemData(settingsDesignerItem.Id, typeof(SettingsDesignerItem)));
                    }
                    //Save all connections which should now have their Connection.DataItems filled in with correct Ids
                    foreach (var connectionVM in DiagramViewModel.Items.OfType<ConnectorViewModel>())
                    {
                        FullyCreatedConnectorInfo sinkConnector = connectionVM.SinkConnectorInfo as FullyCreatedConnectorInfo;

                        Connection connection = new Connection(
                            connectionVM.Id,
                            connectionVM.SourceConnectorInfo.DataItem.Id,
                            GetOrientationFromConnector(connectionVM.SourceConnectorInfo.Orientation),
                            GetTypeOfDiagramItem(connectionVM.SourceConnectorInfo.DataItem),
                            sinkConnector.DataItem.Id,
                            GetOrientationFromConnector(sinkConnector.Orientation),
                            GetTypeOfDiagramItem(sinkConnector.DataItem));

                        connectionVM.Id = databaseAccessService.SaveConnection(connection);
                        wholeDiagramToSave.ConnectionIds.Add(connectionVM.Id);
                    }

                    wholeDiagramToSave.Id = databaseAccessService.SaveDiagram(wholeDiagramToSave);
                    return wholeDiagramToSave.Id;
                });
            task.ContinueWith((ant) =>
            {
                int wholeDiagramToSaveId = ant.Result;
                if (!savedDiagrams.Contains(wholeDiagramToSaveId))
                {
                    List<int> newDiagrams = new List<int>(savedDiagrams);
                    newDiagrams.Add(wholeDiagramToSaveId);
                    SavedDiagrams = newDiagrams;

                }
                IsBusy = false;
                messageBoxService.ShowInformation(string.Format("Finished saving Diagram Id : {0}", wholeDiagramToSaveId));

            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void ExecuteLoadDiagramCommand(object parameter)
        {
            IsBusy = true;
            DiagramItem wholeDiagramToLoad = null;
            if (SavedDiagramId == null)
            {
                messageBoxService.ShowError("You need to select a diagram to load");
                return;
            }

            Task<DiagramViewModel> task = Task.Factory.StartNew<DiagramViewModel>(() =>
                {
                    //ensure that itemsToRemove is cleared ready for any new changes within a session
                    itemsToRemove = new List<SelectableDesignerItemViewModelBase>();
                    DiagramViewModel diagramViewModel = new DiagramViewModel();

                    wholeDiagramToLoad = databaseAccessService.FetchDiagram((int)SavedDiagramId.Value);

                    //load diagram items
                    foreach (DiagramItemData diagramItemData in wholeDiagramToLoad.DesignerItems)
                    {
                        if (diagramItemData.ItemType == typeof(PersistDesignerItem))
                        {
                            PersistDesignerItem persistedDesignerItem = databaseAccessService.FetchPersistDesignerItem(diagramItemData.ItemId);
                            PersistDesignerItemViewModel persistDesignerItemViewModel =
                                new PersistDesignerItemViewModel(persistedDesignerItem.Id, diagramViewModel, persistedDesignerItem.Left, persistedDesignerItem.Top, persistedDesignerItem.HostUrl);
                            diagramViewModel.Items.Add(persistDesignerItemViewModel);
                        }
                        if (diagramItemData.ItemType == typeof(SettingsDesignerItem))
                        {
                            SettingsDesignerItem settingsDesignerItem = databaseAccessService.FetchSettingsDesignerItem(diagramItemData.ItemId);
                            SettingsDesignerItemViewModel settingsDesignerItemViewModel =
                                new SettingsDesignerItemViewModel(settingsDesignerItem.Id, diagramViewModel, settingsDesignerItem.Left, settingsDesignerItem.Top, settingsDesignerItem.Setting1);
                            diagramViewModel.Items.Add(settingsDesignerItemViewModel);
                        }
                    }
                    //load connection items
                    foreach (int connectionId in wholeDiagramToLoad.ConnectionIds)
                    {
                        Connection connection = databaseAccessService.FetchConnection(connectionId);

                        DesignerItemViewModelBase sourceItem = GetConnectorDataItem(diagramViewModel, connection.SourceId, connection.SourceType);
                        ConnectorOrientation sourceConnectorOrientation = GetOrientationForConnector(connection.SourceOrientation);
                        FullyCreatedConnectorInfo sourceConnectorInfo = GetFullConnectorInfo(connection.Id, sourceItem, sourceConnectorOrientation);

                        DesignerItemViewModelBase sinkItem = GetConnectorDataItem(diagramViewModel, connection.SinkId, connection.SinkType);
                        ConnectorOrientation sinkConnectorOrientation = GetOrientationForConnector(connection.SinkOrientation);
                        FullyCreatedConnectorInfo sinkConnectorInfo = GetFullConnectorInfo(connection.Id, sinkItem, sinkConnectorOrientation);

                        ConnectorViewModel connectionVM = new ConnectorViewModel(connection.Id, diagramViewModel, sourceConnectorInfo, sinkConnectorInfo);
                        diagramViewModel.Items.Add(connectionVM);
                    }

                    return diagramViewModel;
                });
            task.ContinueWith((ant) =>
                {
                    this.DiagramViewModel = ant.Result;
                    IsBusy = false;
                    messageBoxService.ShowInformation(string.Format("Finished loading Diagram Id : {0}", wholeDiagramToLoad.Id));
 
                },TaskContinuationOptions.OnlyOnRanToCompletion);
        }


        private FullyCreatedConnectorInfo GetFullConnectorInfo(int connectorId, DesignerItemViewModelBase dataItem, ConnectorOrientation connectorOrientation)
        {
            switch(connectorOrientation)
            {
                case ConnectorOrientation.Top:
                    return dataItem.TopConnector;
                case ConnectorOrientation.Left:
                    return dataItem.LeftConnector;
                case ConnectorOrientation.Right:
                    return dataItem.RightConnector;
                case ConnectorOrientation.Bottom:
                    return dataItem.BottomConnector;

                default:
                    throw new InvalidOperationException(
                        string.Format("Found invalid persisted Connector Orientation for Connector Id: {0}", connectorId));
            }
        }

        private Type GetTypeOfDiagramItem(DesignerItemViewModelBase vmType)
        {
            if (vmType is PersistDesignerItemViewModel)
                return typeof(PersistDesignerItem);
            if (vmType is SettingsDesignerItemViewModel)
                return typeof(SettingsDesignerItem);

            throw new InvalidOperationException(string.Format("Unknown diagram type. Currently only {0} and {1} are supported",
                typeof(PersistDesignerItem).AssemblyQualifiedName,
                typeof(SettingsDesignerItemViewModel).AssemblyQualifiedName
                ));

        }

        private DesignerItemViewModelBase GetConnectorDataItem(DiagramViewModel diagramViewModel, int conectorDataItemId, Type connectorDataItemType)
        {
            DesignerItemViewModelBase dataItem = null;

            if (connectorDataItemType == typeof(PersistDesignerItem))
            {
                dataItem = diagramViewModel.Items.OfType<PersistDesignerItemViewModel>().Single(x => x.Id == conectorDataItemId);
            }

            if (connectorDataItemType == typeof(SettingsDesignerItem))
            {
                dataItem = diagramViewModel.Items.OfType<SettingsDesignerItemViewModel>().Single(x => x.Id == conectorDataItemId);
            }
            return dataItem;
        }


        private Orientation GetOrientationFromConnector(ConnectorOrientation connectorOrientation)
        {
            Orientation result = Orientation.None;
            switch (connectorOrientation)
            {
                case ConnectorOrientation.Bottom:
                    result = Orientation.Bottom;
                    break;
                case ConnectorOrientation.Left:
                    result = Orientation.Left;
                    break;
                case ConnectorOrientation.Top:
                    result = Orientation.Top;
                    break;
                case ConnectorOrientation.Right:
                    result = Orientation.Right;
                    break;
            }
            return result;
        }


        private ConnectorOrientation GetOrientationForConnector(Orientation persistedOrientation)
        {
            ConnectorOrientation result = ConnectorOrientation.None;
            switch (persistedOrientation)
            {
                case Orientation.Bottom:
                    result = ConnectorOrientation.Bottom;
                    break;
                case Orientation.Left:
                    result = ConnectorOrientation.Left;
                    break;
                case Orientation.Top:
                    result = ConnectorOrientation.Top;
                    break;
                case Orientation.Right:
                    result = ConnectorOrientation.Right;
                    break;
            }
            return result;
        }

        private bool ItemsToDeleteHasConnector(List<SelectableDesignerItemViewModelBase> itemsToRemove, FullyCreatedConnectorInfo connector)
        {
            return itemsToRemove.Contains(connector.DataItem);
        }



        private void DeleteFromDatabase(DiagramItem wholeDiagramToAdjust, SelectableDesignerItemViewModelBase itemToDelete)
        {

            //make sure the item is removes from Diagram as well as removing them as individual items from database
            if (itemToDelete is PersistDesignerItemViewModel)
            {
                DiagramItemData diagramItemToRemoveFromParent = wholeDiagramToAdjust.DesignerItems.Where(x => x.ItemId == itemToDelete.Id && x.ItemType == typeof(PersistDesignerItem)).Single();
                wholeDiagramToAdjust.DesignerItems.Remove(diagramItemToRemoveFromParent);
                databaseAccessService.DeletePersistDesignerItem(itemToDelete.Id);
            }
            if (itemToDelete is SettingsDesignerItemViewModel)
            {
                DiagramItemData diagramItemToRemoveFromParent = wholeDiagramToAdjust.DesignerItems.Where(x => x.ItemId == itemToDelete.Id && x.ItemType == typeof(SettingsDesignerItem)).Single();
                wholeDiagramToAdjust.DesignerItems.Remove(diagramItemToRemoveFromParent);
                databaseAccessService.DeleteSettingDesignerItem(itemToDelete.Id);
            }
            if (itemToDelete is ConnectorViewModel)
            {
                wholeDiagramToAdjust.ConnectionIds.Remove(itemToDelete.Id);
                databaseAccessService.DeleteConnection(itemToDelete.Id);
            }

            databaseAccessService.SaveDiagram(wholeDiagramToAdjust);


        }
        
    }
}
