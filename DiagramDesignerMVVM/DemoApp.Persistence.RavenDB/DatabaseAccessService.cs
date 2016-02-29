using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Embedded;
using Raven.Client;
using DemoApp.Persistence.Common;

namespace DemoApp.Persistence.RavenDB
{
    /// <summary>
    /// I decided to use RavenDB instead of SQL, to save people having to have SQL Server, and also
    /// it just takes less time to do with Raven. This is ALL the CRUD code. Simple no?
    /// 
    /// Thing is the IDatabaseAccessService and the items it persists could easily be applied to helper methods that
    /// use StoredProcedures or ADO code, the data being stored would be exactly the same. You would just need to store
    /// the individual property values in tables rather than store objects.
    /// </summary>
    public class DatabaseAccessService : IDatabaseAccessService
    {
        EmbeddableDocumentStore documentStore = null;

        public DatabaseAccessService()
        {
            documentStore = new EmbeddableDocumentStore
            {
                DataDirectory = "Data"
            };
            documentStore.Initialize();
        }
            
        public void DeleteConnection(int connectionId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                IEnumerable<Connection> conns = session.Query<Connection>().Where(x => x.Id == connectionId);
                foreach (var conn in conns)
                {
                    session.Delete<Connection>(conn);
                }
                session.SaveChanges();
            }
        }

        public void DeletePersistDesignerItem(int persistDesignerId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                IEnumerable<PersistDesignerItem> persistItems = session.Query<PersistDesignerItem>().Where(x => x.Id == persistDesignerId);
                foreach (var persistItem in persistItems)
                {
                    session.Delete<PersistDesignerItem>(persistItem);
                }
                session.SaveChanges();
            }
        }

        public void DeleteSettingDesignerItem(int settingsDesignerItemId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                IEnumerable<SettingsDesignerItem> settingItems = session.Query<SettingsDesignerItem>().Where(x => x.Id == settingsDesignerItemId);
                foreach (var settingItem in settingItems)
                {
                    session.Delete<SettingsDesignerItem>(settingItem);
                }
                session.SaveChanges();
            }
        }

        public int SaveDiagram(DiagramItem diagram)
        {
            return SaveItem(diagram);
        }

        public int SavePersistDesignerItem(PersistDesignerItem persistDesignerItemToSave)
        {
            return SaveItem(persistDesignerItemToSave);
        }

        public int SaveSettingDesignerItem(SettingsDesignerItem settingsDesignerItemToSave)
        {
            return SaveItem(settingsDesignerItemToSave);
        }

        public int SaveGroupingDesignerItem(GroupDesignerItem groupDesignerItemToSave)
        {
            return SaveItem(groupDesignerItemToSave);
        }

        public int SaveConnection(Connection connectionToSave)
        {
            return SaveItem(connectionToSave);
        }

        public IEnumerable<DiagramItem> FetchAllDiagram()
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<DiagramItem>().ToList();
            }
        }

        public DiagramItem FetchDiagram(int diagramId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<DiagramItem>().Single(x => x.Id == diagramId);
            }
        }

        public PersistDesignerItem FetchPersistDesignerItem(int settingsDesignerItemId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<PersistDesignerItem>().Single(x => x.Id == settingsDesignerItemId);
            }
        }

        public SettingsDesignerItem FetchSettingsDesignerItem(int settingsDesignerItemId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<SettingsDesignerItem>().Single(x => x.Id == settingsDesignerItemId);
            }
        }

        public GroupDesignerItem FetchGroupingDesignerItem(int groupDesignerItemId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<GroupDesignerItem>().Single(x => x.Id == groupDesignerItemId);
            }
        }
        public Connection FetchConnection(int connectionId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Query<Connection>().Single(x => x.Id == connectionId);
            }
        }

        private int SaveItem(PersistableItemBase item)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                session.Store(item);
                session.SaveChanges();
            }
            return item.Id;
        }
    }
}
