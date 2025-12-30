namespace JobWebService.ORM.Repositories
{
    public class Repository
    {
        protected DBHelperOledb helperOleDb;
        protected ModelCreators modelCreators;

        public Repository(DBHelperOledb helperOledb)
        {
            helperOleDb = helperOledb;
        }

        public Repository(DBHelperOledb helperOleDb, ModelCreators modelCreators)
        {
            this.helperOleDb = helperOleDb;
            this.modelCreators = modelCreators;
        }
    }
}
