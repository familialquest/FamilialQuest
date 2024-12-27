using Proyecto26;

namespace Code.Models.REST
{
    public class FQResponse
    {
        public FQResponseInfo ri;

        public FQResponse(ResponseHelper response)
        {
            ri = new FQResponseInfo(response.Text);
        }

        //public virtual FQResponse Parse(ResponseHelper response)
        //{
        //    return new FQResponse(response);
        //}
    }
}
