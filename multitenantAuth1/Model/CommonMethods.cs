using Microsoft.Graph.CallRecords;

namespace multitenantAuth1.Model
{
    public static class CommonMethods
    {
        public static Session s = new Session();
        public static Session GetSession()
        {
            return s;
        }
    }
}
